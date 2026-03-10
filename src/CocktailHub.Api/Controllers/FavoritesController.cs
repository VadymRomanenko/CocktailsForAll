using System.Security.Claims;
using CocktailHub.Core.Entities;
using CocktailHub.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CocktailHub.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FavoritesController : ControllerBase
{
    private readonly AppDbContext _db;

    public FavoritesController(AppDbContext db)
    {
        _db = db;
    }

    private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpPost("{cocktailId:int}")]
    public async Task<IActionResult> Add(int cocktailId, CancellationToken ct)
    {
        var exists = await _db.Cocktails.AnyAsync(c => c.Id == cocktailId, ct);
        if (!exists) return NotFound();

        if (await _db.Favorites.AnyAsync(f => f.UserId == UserId && f.CocktailId == cocktailId, ct))
            return Ok();

        _db.Favorites.Add(new Favorite { UserId = UserId, CocktailId = cocktailId });
        await _db.SaveChangesAsync(ct);
        return Ok();
    }

    [HttpDelete("{cocktailId:int}")]
    public async Task<IActionResult> Remove(int cocktailId, CancellationToken ct)
    {
        var fav = await _db.Favorites.FirstOrDefaultAsync(f => f.UserId == UserId && f.CocktailId == cocktailId, ct);
        if (fav != null)
        {
            _db.Favorites.Remove(fav);
            await _db.SaveChangesAsync(ct);
        }
        return Ok();
    }

    [HttpGet]
    public async Task<IActionResult> GetMyFavorites(CancellationToken ct)
    {
        var ids = await _db.Favorites
            .Where(f => f.UserId == UserId)
            .Select(f => f.CocktailId)
            .ToListAsync(ct);
        return Ok(ids);
    }
}
