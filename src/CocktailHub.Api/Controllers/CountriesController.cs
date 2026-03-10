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

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var items = await _db.Countries
            .OrderBy(c => c.Name)
            .Select(c => new { c.Id, c.Name, c.IsoCode })
            .ToListAsync(ct);
        return Ok(items);
    }
}
