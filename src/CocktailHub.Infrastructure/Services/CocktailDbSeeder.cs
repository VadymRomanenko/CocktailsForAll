using System.Text.Json;
using CocktailHub.Core.Entities;
using CocktailHub.Infrastructure.Data;
using CocktailHub.Infrastructure.Options;
using CocktailHub.Infrastructure.SeedData;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CocktailHub.Infrastructure.Services;

public class CocktailDbSeeder
{
    private const int MaxCocktailsToSeed = 500;
    private readonly AppDbContext _db;
    private readonly TheCocktailDbClient _client;
    private readonly TranslationService? _translation;
    private readonly ILogger<CocktailDbSeeder> _logger;
    private readonly CocktailHubOptions _options;

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

    private static readonly string[] PopularIngredients = { "Vodka", "Gin", "Rum", "Tequila", "Whiskey", "Brandy", "Lime", "Lemon", "Sugar", "Mint" };

    private static readonly int[] FallbackCountryIds = { 1, 2, 3, 4, 5, 6, 7, 8 };

    public CocktailDbSeeder(AppDbContext db, TheCocktailDbClient client, ILogger<CocktailDbSeeder> logger, IOptions<CocktailHubOptions> options, TranslationService? translation = null)
    {
        _db = db;
        _client = client;
        _logger = logger;
        _options = options.Value;
        _translation = translation;
    }

    public async Task SeedAsync(CancellationToken ct = default)
    {
        if (await _db.Cocktails.AnyAsync(ct))
        {
            _logger.LogInformation("Cocktails already seeded, skipping");
            return;
        }

        if (await TrySeedFromJsonAsync(ct))
            return;

        await SeedIngredientsAsync(ct);
        await SeedCocktailsAsync(ct);
    }

    private async Task<bool> TrySeedFromJsonAsync(CancellationToken ct)
    {
        var path = Path.Combine(AppContext.BaseDirectory, "SeedData", "cocktails.json");
        if (!File.Exists(path))
        {
            _logger.LogInformation("Seed data file not found at {Path}, using API", path);
            return false;
        }

        var json = await File.ReadAllTextAsync(path, ct);
        var root = JsonSerializer.Deserialize<SeedDataRoot>(json);
        if (root?.Cocktails == null || root.Cocktails.Count == 0)
        {
            _logger.LogInformation("Seed data file is empty, using API");
            return false;
        }

        _logger.LogInformation("Seeding from static JSON ({Count} cocktails, {Ingredients} ingredients)",
            root.Cocktails.Count, root.Ingredients.Count);

        var ingredientLookup = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        foreach (var name in root.Ingredients)
        {
            if (string.IsNullOrWhiteSpace(name)) continue;
            var ing = new Ingredient { Name = name.Trim() };
            _db.Ingredients.Add(ing);
        }
        await _db.SaveChangesAsync(ct);

        foreach (var ing in await _db.Ingredients.ToListAsync(ct))
            ingredientLookup[ing.Name] = ing.Id;

        foreach (var sc in root.Cocktails)
        {
            if (string.IsNullOrWhiteSpace(sc.Name)) continue;
            var cocktail = new Cocktail
            {
                Name = sc.Name,
                Description = sc.Description,
                Instructions = sc.Instructions ?? "",
                ImageUrl = sc.ImageUrl,
                CountryId = sc.CountryId is >= 1 and <= 33 ? sc.CountryId : 1,
                IsModerated = true,
                CreatedByUserId = null
            };
            _db.Cocktails.Add(cocktail);
            await _db.SaveChangesAsync(ct);

            _db.CocktailTranslations.Add(new CocktailTranslation
            {
                CocktailId = cocktail.Id,
                LangCode = "en",
                Name = cocktail.Name,
                Description = cocktail.Description,
                Instructions = cocktail.Instructions
            });

            var seenIngredients = new HashSet<int>();
            foreach (var si in sc.Ingredients)
            {
                if (string.IsNullOrWhiteSpace(si.Name)) continue;
                if (!ingredientLookup.TryGetValue(si.Name.Trim(), out var ingId))
                {
                    var newIng = new Ingredient { Name = si.Name.Trim() };
                    _db.Ingredients.Add(newIng);
                    await _db.SaveChangesAsync(ct);
                    ingId = newIng.Id;
                    ingredientLookup[si.Name.Trim()] = ingId;
                }
                if (!seenIngredients.Add(ingId)) continue; // dedupe: same ingredient twice
                _db.CocktailIngredients.Add(new CocktailIngredient
                {
                    CocktailId = cocktail.Id,
                    IngredientId = ingId,
                    Measure = si.Measure
                });
            }
            await _db.SaveChangesAsync(ct);
        }

        _logger.LogInformation("Seeded {Count} cocktails from JSON", root.Cocktails.Count);
        return true;
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

    private async Task<List<string>> CollectDrinkIdsAsync(CancellationToken ct)
    {
        var ids = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        for (var c = 'a'; c <= 'z' && ids.Count < MaxCocktailsToSeed; c++)
        {
            var listResponse = await _client.SearchByFirstLetterAsync(c, ct);
            if (listResponse?.Drinks == null) continue;
            foreach (var d in listResponse.Drinks)
            {
                if (!string.IsNullOrEmpty(d.IdDrink)) ids.Add(d.IdDrink);
                if (ids.Count >= MaxCocktailsToSeed) break;
            }
            await Task.Delay(150, ct);
        }

        if (ids.Count < MaxCocktailsToSeed)
        {
            var catResponse = await _client.GetCategoriesListAsync(ct);
            if (catResponse?.Drinks != null)
            {
                foreach (var cat in catResponse.Drinks.Take(10))
                {
                    if (string.IsNullOrEmpty(cat.StrCategory)) continue;
                    var listResponse = await _client.FilterByCategoryAsync(cat.StrCategory, ct);
                    if (listResponse?.Drinks == null) continue;
                    foreach (var d in listResponse.Drinks)
                    {
                        if (!string.IsNullOrEmpty(d.IdDrink)) ids.Add(d.IdDrink);
                        if (ids.Count >= MaxCocktailsToSeed) break;
                    }
                    await Task.Delay(150, ct);
                    if (ids.Count >= MaxCocktailsToSeed) break;
                }
            }
        }

        if (ids.Count < MaxCocktailsToSeed)
        {
            foreach (var ing in PopularIngredients)
            {
                var listResponse = await _client.FilterByIngredientAsync(ing, ct);
                if (listResponse?.Drinks == null) continue;
                foreach (var d in listResponse.Drinks)
                {
                    if (!string.IsNullOrEmpty(d.IdDrink)) ids.Add(d.IdDrink);
                    if (ids.Count >= MaxCocktailsToSeed) break;
                }
                await Task.Delay(150, ct);
                if (ids.Count >= MaxCocktailsToSeed) break;
            }
        }

        return ids.Take(MaxCocktailsToSeed).ToList();
    }

    private async Task SeedCocktailsAsync(CancellationToken ct)
    {
        var ids = await CollectDrinkIdsAsync(ct);
        var ingredientLookup = await _db.Ingredients.ToDictionaryAsync(i => i.Name.ToLowerInvariant(), i => i.Id, ct);
        var random = new Random();
        var seedTranslations = _options.SeedTranslations;

        foreach (var id in ids)
        {
            try
            {
                await Task.Delay(200, ct);
                var detailResponse = await _client.LookupCocktailAsync(id, ct);
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

                _db.CocktailTranslations.Add(new CocktailTranslation
                {
                    CocktailId = cocktail.Id,
                    LangCode = "en",
                    Name = cocktail.Name,
                    Description = cocktail.Description,
                    Instructions = cocktail.Instructions
                });

                if (seedTranslations && _translation != null)
                {
                    foreach (var lang in new[] { "uk", "pl" })
                    {
                        var name = await _translation.TranslateAsync(cocktail.Name, "en", lang, ct);
                        var desc = string.IsNullOrEmpty(cocktail.Description) ? null : await _translation.TranslateAsync(cocktail.Description, "en", lang, ct);
                        var instr = await _translation.TranslateAsync(cocktail.Instructions, "en", lang, ct);
                        _db.CocktailTranslations.Add(new CocktailTranslation
                        {
                            CocktailId = cocktail.Id,
                            LangCode = lang,
                            Name = name ?? cocktail.Name,
                            Description = desc,
                            Instructions = instr ?? cocktail.Instructions
                        });
                        await Task.Delay(1500, ct);
                    }
                }

                var ingredients = GetIngredientMeasuresDeduplicated(drink);
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
                _logger.LogWarning(ex, "Failed to seed cocktail {Id}", id);
            }
        }

        _logger.LogInformation("Cocktail seeding completed");
    }

    public async Task ApplyTranslationsFromFileAsync(CancellationToken ct = default)
    {
        var path = Path.Combine(AppContext.BaseDirectory, "SeedData", "cocktails-translations.json");
        if (!File.Exists(path))
        {
            _logger.LogInformation("Translation file not found at {Path}, skipping", path);
            return;
        }

        var json = await File.ReadAllTextAsync(path, ct);
        var root = JsonSerializer.Deserialize<TranslationDataRoot>(json);
        if (root == null || (root.Cocktails.Count == 0 && root.Ingredients.Count == 0))
        {
            _logger.LogInformation("Translation file is empty, skipping");
            return;
        }

        _logger.LogInformation("Applying translations from file ({Cocktails} cocktails, {Ingredients} ingredients)",
            root.Cocktails.Count, root.Ingredients.Count);

        var cocktailIdByName = await _db.Cocktails
            .ToDictionaryAsync(c => c.Name.ToLowerInvariant(), c => c.Id, ct);

        var existingCocktailTranslations = await _db.CocktailTranslations
            .Select(t => new { t.CocktailId, t.LangCode })
            .ToListAsync(ct);
        var existingCocktailKeys = existingCocktailTranslations
            .Select(t => (t.CocktailId, t.LangCode))
            .ToHashSet();

        var addedCocktails = 0;
        foreach (var entry in root.Cocktails)
        {
            if (!cocktailIdByName.TryGetValue(entry.Name.ToLowerInvariant(), out var cocktailId))
                continue;

            foreach (var tr in entry.Translations)
            {
                if (existingCocktailKeys.Contains((cocktailId, tr.Lang)))
                    continue;

                _db.CocktailTranslations.Add(new CocktailTranslation
                {
                    CocktailId = cocktailId,
                    LangCode = tr.Lang,
                    Name = tr.Name,
                    Description = tr.Description,
                    Instructions = tr.Instructions
                });
                addedCocktails++;
            }
        }

        var ingredientIdByName = await _db.Ingredients
            .ToDictionaryAsync(i => i.Name.ToLowerInvariant(), i => i.Id, ct);

        var existingIngredientTranslations = await _db.IngredientTranslations
            .Select(t => new { t.IngredientId, t.LangCode })
            .ToListAsync(ct);
        var existingIngredientKeys = existingIngredientTranslations
            .Select(t => (t.IngredientId, t.LangCode))
            .ToHashSet();

        var addedIngredients = 0;
        foreach (var entry in root.Ingredients)
        {
            if (!ingredientIdByName.TryGetValue(entry.Name.ToLowerInvariant(), out var ingredientId))
                continue;

            foreach (var tr in entry.Translations)
            {
                if (existingIngredientKeys.Contains((ingredientId, tr.Lang)))
                    continue;

                _db.IngredientTranslations.Add(new IngredientTranslation
                {
                    IngredientId = ingredientId,
                    LangCode = tr.Lang,
                    Name = tr.Name
                });
                addedIngredients++;
            }
        }

        await _db.SaveChangesAsync(ct);
        _logger.LogInformation("Applied {Cocktails} cocktail translations and {Ingredients} ingredient translations",
            addedCocktails, addedIngredients);
    }

    private static int ResolveCountryId(DrinkDetail drink, Random random)
    {
        if (!string.IsNullOrEmpty(drink.StrIBA) && IbaToCountryId.TryGetValue(drink.StrIBA.Trim(), out var ibaId))
            return ibaId;
        if (!string.IsNullOrEmpty(drink.StrCategory) && CategoryToCountryId.TryGetValue(drink.StrCategory.Trim(), out var catId))
            return catId;
        return FallbackCountryIds[random.Next(FallbackCountryIds.Length)];
    }

    /// <summary>
    /// Deduplicates ingredients by name (case-insensitive). Prevents PK_CocktailIngredients
    /// violation when TheCocktailDB returns the same ingredient multiple times.
    /// </summary>
    private static List<(string Name, string? Measure)> GetIngredientMeasuresDeduplicated(DrinkDetail d)
    {
        var ingredients = new[]
        {
            (d.StrIngredient1, d.StrMeasure1), (d.StrIngredient2, d.StrMeasure2), (d.StrIngredient3, d.StrMeasure3),
            (d.StrIngredient4, d.StrMeasure4), (d.StrIngredient5, d.StrMeasure5), (d.StrIngredient6, d.StrMeasure6),
            (d.StrIngredient7, d.StrMeasure7), (d.StrIngredient8, d.StrMeasure8), (d.StrIngredient9, d.StrMeasure9),
            (d.StrIngredient10, d.StrMeasure10), (d.StrIngredient11, d.StrMeasure11), (d.StrIngredient12, d.StrMeasure12),
            (d.StrIngredient13, d.StrMeasure13), (d.StrIngredient14, d.StrMeasure14), (d.StrIngredient15, d.StrMeasure15)
        };
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        return ingredients
            .Where(x => !string.IsNullOrWhiteSpace(x.Item1))
            .Select(x => (x.Item1!.Trim(), string.IsNullOrWhiteSpace(x.Item2) ? null : x.Item2?.Trim()))
            .Where(x => seen.Add(x.Item1))
            .ToList();
    }
}
