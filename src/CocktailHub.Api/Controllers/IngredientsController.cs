using CocktailHub.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CocktailHub.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class IngredientsController : ControllerBase
{
    private readonly AppDbContext _db;

    public IngredientsController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> Search([FromQuery] string? search, [FromQuery] int limit = 20, CancellationToken ct = default)
    {
        var query = _db.Ingredients.AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(i => i.Name.ToLower().Contains(term));
        }
        var items = await query
            .OrderBy(i => i.Name)
            .Take(limit)
            .Select(i => new { i.Id, i.Name })
            .ToListAsync(ct);
        return Ok(items);
    }
}
