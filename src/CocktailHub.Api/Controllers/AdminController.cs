using System.Security.Claims;
using CocktailHub.Api.DTOs.Cocktail;
using CocktailHub.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CocktailHub.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly AppDbContext _db;

    public AdminController(AppDbContext db)
    {
        _db = db;
    }

    private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet("pending")]
    public async Task<IActionResult> GetPendingCocktails(CancellationToken ct)
    {
        var cocktails = await _db.Cocktails
            .Where(c => !c.IsModerated)
            .Include(c => c.Country)
            .Include(c => c.CocktailIngredients).ThenInclude(ci => ci.Ingredient)
            .OrderBy(c => c.Name)
            .ToListAsync(ct);

        var favIds = (await _db.Favorites
            .Where(f => f.UserId == UserId && cocktails.Select(c => c.Id).Contains(f.CocktailId))
            .Select(f => f.CocktailId)
            .ToListAsync(ct)).ToHashSet();

        var items = cocktails.Select(c => new
        {
            c.Id,
            c.Name,
            c.Description,
            c.Instructions,
            c.ImageUrl,
            c.CountryId,
            CountryName = c.Country.Name,
            c.IsModerated,
            IsFavorite = favIds.Contains(c.Id),
            Ingredients = c.CocktailIngredients.Select(ci => new IngredientMeasureDto(ci.IngredientId, ci.Ingredient.Name, ci.Measure)).ToList()
        }).ToList();
        return Ok(items);
    }

    [HttpPut("cocktails/{id:int}/approve")]
    public async Task<IActionResult> ApproveCocktail(int id, CancellationToken ct)
    {
        var cocktail = await _db.Cocktails.FindAsync([id], ct);
        if (cocktail == null) return NotFound();
        cocktail.IsModerated = true;
        await _db.SaveChangesAsync(ct);
        return Ok();
    }
}
