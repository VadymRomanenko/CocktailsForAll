using System.Security.Claims;
using CocktailHub.Api.DTOs.Cocktail;
using CocktailHub.Core.Entities;
using CocktailHub.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CocktailHub.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CocktailsController : ControllerBase
{
    private readonly AppDbContext _db;

    public CocktailsController(AppDbContext db)
    {
        _db = db;
    }

    private int? UserId => int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;

    [HttpGet]
    public async Task<IActionResult> GetList(
        [FromQuery] string? name,
        [FromQuery] int? countryId,
        [FromQuery] int[]? ingredientIds,
        [FromQuery] string[]? freeTextTags,
        [FromQuery] bool matchAllIngredients = false,
        [FromQuery] string? lang = "en",
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var langCode = lang is "uk" or "pl" ? lang : "en";
        var query = _db.Cocktails
            .Where(c => c.IsModerated)
            .Include(c => c.Country)
            .Include(c => c.Translations)
            .Include(c => c.CocktailIngredients).ThenInclude(ci => ci.Ingredient)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(name))
        {
            var term = name.Trim().ToLower();
            query = query.Where(c =>
                c.Name.ToLower().Contains(term) ||
                c.Translations.Any(t => t.LangCode == langCode && t.Name.ToLower().Contains(term)));
        }

        if (countryId.HasValue)
            query = query.Where(c => c.CountryId == countryId.Value);

        if (ingredientIds is { Length: > 0 })
        {
            if (matchAllIngredients)
                query = query.Where(c => ingredientIds.All(id => c.CocktailIngredients.Any(ci => ci.IngredientId == id)));
            else
                query = query.Where(c => c.CocktailIngredients.Any(ci => ingredientIds.Contains(ci.IngredientId)));
        }

        if (freeTextTags is { Length: > 0 })
        {
            foreach (var tag in freeTextTags.Where(t => !string.IsNullOrWhiteSpace(t)))
            {
                var term = tag.Trim().ToLower();
                query = query.Where(c => c.CocktailIngredients.Any(ci =>
                    ci.Ingredient.Name.ToLower().Contains(term)));
            }
        }

        var total = await query.CountAsync(ct);
        var cocktails = await query
            .OrderBy(c => c.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        var favIds = new HashSet<int>();
        if (UserId.HasValue)
        {
            favIds = (await _db.Favorites
                .Where(f => f.UserId == UserId && cocktails.Select(c => c.Id).Contains(f.CocktailId))
                .Select(f => f.CocktailId)
                .ToListAsync(ct)).ToHashSet();
        }

        var items = cocktails.Select(c =>
        {
            var tr = c.Translations.FirstOrDefault(t => t.LangCode == langCode);
            var displayName = tr?.Name ?? c.Name;
            return new CocktailListResponse(c.Id, displayName, c.ImageUrl, c.Country.Name, favIds.Contains(c.Id));
        }).ToList();
        return Ok(new { items, total, page, pageSize });
    }

    [HttpGet("cocktail-of-the-day")]
    public async Task<IActionResult> GetCocktailOfTheDay([FromQuery] string? lang = "en", CancellationToken ct = default)
    {
        var langCode = lang is "uk" or "pl" ? lang : "en";
        var total = await _db.Cocktails.CountAsync(c => c.IsModerated, ct);
        if (total == 0) return NotFound();

        var epoch = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var index = (int)(DateTimeOffset.UtcNow - epoch).TotalDays % total;

        var cocktail = await _db.Cocktails
            .Where(c => c.IsModerated)
            .Include(c => c.Country)
            .Include(c => c.Translations)
            .OrderBy(c => c.Id)
            .Skip(index)
            .Take(1)
            .FirstOrDefaultAsync(ct);

        if (cocktail == null) return NotFound();

        var tr = cocktail.Translations.FirstOrDefault(t => t.LangCode == langCode);
        return Ok(new
        {
            cocktail.Id,
            Name = tr?.Name ?? cocktail.Name,
            Description = tr?.Description ?? cocktail.Description,
            cocktail.ImageUrl,
            cocktail.CountryId,
            CountryName = cocktail.Country.Name
        });
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, [FromQuery] string? lang = "en", CancellationToken ct = default)
    {
        var langCode = lang is "uk" or "pl" ? lang : "en";
        var cocktail = await _db.Cocktails
            .Include(c => c.Country)
            .Include(c => c.Translations)
            .Include(c => c.CocktailIngredients).ThenInclude(ci => ci.Ingredient)
            .FirstOrDefaultAsync(c => c.Id == id, ct);
        if (cocktail == null) return NotFound();
        if (!cocktail.IsModerated && cocktail.CreatedByUserId != UserId)
            return NotFound();

        var isFav = UserId.HasValue && await _db.Favorites
            .AnyAsync(f => f.UserId == UserId && f.CocktailId == id, ct);

        var tr = cocktail.Translations.FirstOrDefault(t => t.LangCode == langCode);
        var name = tr?.Name ?? cocktail.Name;
        var description = tr?.Description ?? cocktail.Description;
        var instructions = tr?.Instructions ?? cocktail.Instructions;
        var ingredients = cocktail.CocktailIngredients
            .Select(ci => new IngredientMeasureDto(ci.IngredientId, ci.Ingredient.Name, ci.Measure))
            .ToList();
        return Ok(new CocktailResponse(
            cocktail.Id, name, description, instructions,
            cocktail.ImageUrl, cocktail.CountryId, cocktail.Country.Name, cocktail.IsModerated,
            isFav, ingredients));
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create([FromBody] CreateCocktailRequest request, CancellationToken ct)
    {
        if (!UserId.HasValue) return Unauthorized();

        var countryExists = await _db.Countries.AnyAsync(c => c.Id == request.CountryId, ct);
        if (!countryExists) return BadRequest(new { message = "Invalid country" });

        var cocktail = new Cocktail
        {
            Name = request.Name,
            Description = request.Description,
            Instructions = request.Instructions,
            ImageUrl = request.ImageUrl,
            CountryId = request.CountryId,
            IsModerated = false,
            CreatedByUserId = UserId
        };
        _db.Cocktails.Add(cocktail);
        await _db.SaveChangesAsync(ct);

        foreach (var ing in request.Ingredients)
        {
            if (await _db.Ingredients.AnyAsync(i => i.Id == ing.IngredientId, ct))
            {
                _db.CocktailIngredients.Add(new CocktailIngredient
                {
                    CocktailId = cocktail.Id,
                    IngredientId = ing.IngredientId,
                    Measure = ing.Measure
                });
            }
        }
        await _db.SaveChangesAsync(ct);

        return CreatedAtAction(nameof(GetById), new { id = cocktail.Id }, new { id = cocktail.Id });
    }
}
