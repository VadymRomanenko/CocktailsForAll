using CocktailHub.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CocktailHub.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CountriesController : ControllerBase
{
    private readonly AppDbContext _db;

    public CountriesController(AppDbContext db)
    {
        _db = db;
    }

    public record CountriesFilter([FromQuery] bool ShowNonEmptyOnly = false, [FromQuery] bool ShowCoctailCountsInName = false);

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] CountriesFilter filter, CancellationToken ct)
    {
        var query = _db.Countries.AsQueryable();
        if (filter.ShowNonEmptyOnly)
        {
            query = query.Where(c => c.Cocktails.Any());
        }
        var items = await query
            .OrderBy(c => c.Name)
            .Select(c => new {
                c.Id,
                Name = filter.ShowCoctailCountsInName
                    ? $"{c.Name} ({c.Cocktails.Count})" 
                    : c.Name,
                c.IsoCode })
            .ToListAsync(ct);
        return Ok(items);
    }

}
