using CocktailHub.Core.Entities;
using CocktailHub.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CocktailHub.Infrastructure.Services;

public class CocktailDbSeeder
{
    private readonly AppDbContext _db;
    private readonly TheCocktailDbClient _client;
    private readonly ILogger<CocktailDbSeeder> _logger;

    private static readonly Dictionary<string, int> CategoryToCountryId = new()
    {
        ["Shot"] = 4,
        ["Ordinary Drink"] = 1,
        ["Cocktail"] = 1,
        ["Punch / Party Drink"] = 1,
        ["Soft Drink"] = 1,
        ["Homemade Liqueur"] = 6,
        ["Coffee / Tea"] = 7,
        ["Milk / Float / Shake"] = 1,
        ["Other/Unknown"] = 1,
        ["Cocoa"] = 4,
        ["Beer"] = 10,
        ["Optional alcohol"] = 1,
    };

    private static readonly Dictionary<string, int> IbaToCountryId = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Mojito"] = 5,
        ["Margarita"] = 4,
        ["Daiquiri"] = 5,
        ["Piña Colada"] = 11,
        ["Old Fashioned"] = 1,
        ["Negroni"] = 6,
        ["Manhattan"] = 1,
        ["Martini"] = 1,
        ["Cosmopolitan"] = 1,
        ["French 75"] = 3,
        ["Sangria"] = 9,
        ["Bloody Mary"] = 1,
        ["Irish Coffee"] = 10,
        ["Whiskey Sour"] = 1,
        ["Mai Tai"] = 1,
        ["Long Island Iced Tea"] = 1,
        ["Tom Collins"] = 1,
        ["Mint Julep"] = 1,
    };

    private static readonly int[] FallbackCountryIds = { 1, 2, 3, 4, 5, 6, 7, 8 };

    public CocktailDbSeeder(AppDbContext db, TheCocktailDbClient client, ILogger<CocktailDbSeeder> logger)
    {
        _db = db;
        _client = client;
        _logger = logger;
    }

    public async Task SeedAsync(CancellationToken ct = default)
    {
        if (await _db.Cocktails.AnyAsync(ct))
        {
            _logger.LogInformation("Cocktails already seeded, skipping");
            return;
        }

        await SeedIngredientsAsync(ct);
        await SeedCocktailsAsync(ct);
    }

    private async Task SeedIngredientsAsync(CancellationToken ct)
    {
        if (await _db.Ingredients.AnyAsync(ct))
        {
            _logger.LogInformation("Ingredients already seeded, skipping");
            return;
        }

        var response = await _client.GetIngredientsListAsync(ct);
        if (response?.Drinks == null) return;

        var names = response.Drinks
            .Where(x => !string.IsNullOrWhiteSpace(x.StrIngredient1))
            .Select(x => x.StrIngredient1!.Trim())
            .Distinct()
            .ToList();

        foreach (var name in names)
        {
            if (await _db.Ingredients.AnyAsync(i => i.Name == name, ct)) continue;
            _db.Ingredients.Add(new Ingredient { Name = name });
        }
        await _db.SaveChangesAsync(ct);
        _logger.LogInformation("Seeded {Count} ingredients", names.Count);
    }

    private async Task SeedCocktailsAsync(CancellationToken ct)
    {
        var ingredientLookup = await _db.Ingredients.ToDictionaryAsync(i => i.Name.ToLowerInvariant(), i => i.Id, ct);
        var random = new Random();

        for (var c = 'a'; c <= 'z'; c++)
        {
            var listResponse = await _client.SearchByFirstLetterAsync(c, ct);
            if (listResponse?.Drinks == null) continue;

            foreach (var summary in listResponse.Drinks)
            {
                if (string.IsNullOrEmpty(summary.IdDrink)) continue;

                try
                {
                    await Task.Delay(250, ct);
                    var detailResponse = await _client.LookupCocktailAsync(summary.IdDrink, ct);
                    var drink = detailResponse?.Drinks?.FirstOrDefault();
                    if (drink == null || string.IsNullOrEmpty(drink.StrDrink)) continue;

                    if (await _db.Cocktails.AnyAsync(x => x.Name == drink.StrDrink, ct)) continue;

                    var countryId = ResolveCountryId(drink, random);
                    var cocktail = new Cocktail
                    {
                        Name = drink.StrDrink,
                        Description = drink.StrCategory,
                        Instructions = drink.StrInstructions ?? "",
                        ImageUrl = drink.StrDrinkThumb,
                        CountryId = countryId,
                        IsModerated = true,
                        CreatedByUserId = null
                    };
                    _db.Cocktails.Add(cocktail);
                    await _db.SaveChangesAsync(ct);

                    var ingredients = GetIngredientMeasures(drink);
                    foreach (var (name, measure) in ingredients)
                    {
                        if (string.IsNullOrWhiteSpace(name)) continue;
                        var key = name.Trim().ToLowerInvariant();
                        if (!ingredientLookup.TryGetValue(key, out var ingId))
                        {
                            var newIng = new Ingredient { Name = name.Trim() };
                            _db.Ingredients.Add(newIng);
                            await _db.SaveChangesAsync(ct);
                            ingId = newIng.Id;
                            ingredientLookup[key] = ingId;
                        }
                        _db.CocktailIngredients.Add(new CocktailIngredient
                        {
                            CocktailId = cocktail.Id,
                            IngredientId = ingId,
                            Measure = measure
                        });
                    }
                    await _db.SaveChangesAsync(ct);
                    _logger.LogDebug("Seeded cocktail: {Name}", cocktail.Name);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to seed cocktail {Id}", summary.IdDrink);
                }
            }
        }

        _logger.LogInformation("Cocktail seeding completed");
    }

    private static int ResolveCountryId(DrinkDetail drink, Random random)
    {
        if (!string.IsNullOrEmpty(drink.StrIBA) && IbaToCountryId.TryGetValue(drink.StrIBA.Trim(), out var ibaId))
            return ibaId;
        if (!string.IsNullOrEmpty(drink.StrCategory) && CategoryToCountryId.TryGetValue(drink.StrCategory.Trim(), out var catId))
            return catId;
        return FallbackCountryIds[random.Next(FallbackCountryIds.Length)];
    }

    private static List<(string Name, string? Measure)> GetIngredientMeasures(DrinkDetail d)
    {
        var ingredients = new[]
        {
            (d.StrIngredient1, d.StrMeasure1), (d.StrIngredient2, d.StrMeasure2), (d.StrIngredient3, d.StrMeasure3),
            (d.StrIngredient4, d.StrMeasure4), (d.StrIngredient5, d.StrMeasure5), (d.StrIngredient6, d.StrMeasure6),
            (d.StrIngredient7, d.StrMeasure7), (d.StrIngredient8, d.StrMeasure8), (d.StrIngredient9, d.StrMeasure9),
            (d.StrIngredient10, d.StrMeasure10), (d.StrIngredient11, d.StrMeasure11), (d.StrIngredient12, d.StrMeasure12),
            (d.StrIngredient13, d.StrMeasure13), (d.StrIngredient14, d.StrMeasure14), (d.StrIngredient15, d.StrMeasure15)
        };
        return ingredients
            .Where(x => !string.IsNullOrWhiteSpace(x.Item1))
            .Select(x => (x.Item1!.Trim(), string.IsNullOrWhiteSpace(x.Item2) ? null : x.Item2?.Trim()))
            .ToList();
    }
}
