using System.Text.Json;
using CocktailHub.Infrastructure.SeedData;
using CocktailHub.Infrastructure.Services;

var limit = args.Length > 0 && int.TryParse(args[0], out var n) ? n : 500;
var http = new HttpClient();
var client = new TheCocktailDbClient(http);
var ids = await CollectDrinkIdsAsync(client, limit);
var random = new Random();

var allIngredientNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
var cocktails = new List<SeedCocktail>();

foreach (var id in ids)
{
    await Task.Delay(200);
    var detail = await client.LookupCocktailAsync(id);
    var drink = detail?.Drinks?.FirstOrDefault();
    if (drink == null || string.IsNullOrEmpty(drink.StrDrink)) continue;

    var countryId = ResolveCountryId(drink, random);
    var ingredients = GetIngredientMeasuresDeduplicated(drink);

    foreach (var (name, _) in ingredients)
        if (!string.IsNullOrWhiteSpace(name))
            allIngredientNames.Add(name.Trim());

    cocktails.Add(new SeedCocktail
    {
        Name = drink.StrDrink,
        Description = drink.StrCategory,
        Instructions = drink.StrInstructions ?? "",
        ImageUrl = drink.StrDrinkThumb,
        CountryId = countryId,
        Ingredients = ingredients.Select(x => new SeedCocktailIngredient { Name = x.Name.Trim(), Measure = x.Measure }).ToList()
    });

    Console.WriteLine($"Fetched {cocktails.Count}/{ids.Count}: {drink.StrDrink}");
}

var seedData = new SeedDataRoot
{
    Version = 1,
    GeneratedAt = DateTime.UtcNow.ToString("O"),
    Ingredients = allIngredientNames.OrderBy(x => x, StringComparer.OrdinalIgnoreCase).ToList(),
    Cocktails = cocktails
};

var outputPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "src", "CocktailHub.Infrastructure", "SeedData", "cocktails.json"));
var dir = Path.GetDirectoryName(outputPath)!;
Directory.CreateDirectory(dir);

await using var fs = File.Create(outputPath);
await JsonSerializer.SerializeAsync(fs, seedData, new JsonSerializerOptions { WriteIndented = true });

Console.WriteLine($"\nWrote {cocktails.Count} cocktails and {allIngredientNames.Count} ingredients to {outputPath}");

static async Task<List<string>> CollectDrinkIdsAsync(TheCocktailDbClient client, int maxCocktails)
{
    var ids = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

    for (var c = 'a'; c <= 'z' && ids.Count < maxCocktails; c++)
    {
        var resp = await client.SearchByFirstLetterAsync(c);
        if (resp?.Drinks == null) continue;
        foreach (var d in resp.Drinks)
        {
            if (!string.IsNullOrEmpty(d.IdDrink)) ids.Add(d.IdDrink);
            if (ids.Count >= maxCocktails) break;
        }
        await Task.Delay(150);
    }

    if (ids.Count < maxCocktails)
    {
        var catResp = await client.GetCategoriesListAsync();
        if (catResp?.Drinks != null)
        {
            foreach (var cat in catResp.Drinks.Take(10))
            {
                if (string.IsNullOrEmpty(cat.StrCategory)) continue;
                var listResp = await client.FilterByCategoryAsync(cat.StrCategory);
                if (listResp?.Drinks == null) continue;
                foreach (var d in listResp.Drinks)
                {
                    if (!string.IsNullOrEmpty(d.IdDrink)) ids.Add(d.IdDrink);
                    if (ids.Count >= maxCocktails) break;
                }
                await Task.Delay(150);
                if (ids.Count >= maxCocktails) break;
            }
        }
    }

    var popular = new[] { "Vodka", "Gin", "Rum", "Tequila", "Whiskey", "Brandy", "Lime", "Lemon", "Sugar", "Mint" };
    if (ids.Count < maxCocktails)
    {
        foreach (var ing in popular)
        {
            var listResp = await client.FilterByIngredientAsync(ing);
            if (listResp?.Drinks == null) continue;
            foreach (var d in listResp.Drinks)
            {
                if (!string.IsNullOrEmpty(d.IdDrink)) ids.Add(d.IdDrink);
                if (ids.Count >= maxCocktails) break;
            }
            await Task.Delay(150);
            if (ids.Count >= maxCocktails) break;
        }
    }

    return ids.Take(maxCocktails).ToList();
}

static int ResolveCountryId(DrinkDetail drink, Random random)
{
    var ibaMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
    {
        ["Mojito"] = 5, ["Margarita"] = 4, ["Daiquiri"] = 5, ["Piña Colada"] = 11,
        ["Old Fashioned"] = 1, ["Negroni"] = 6, ["Manhattan"] = 1, ["Martini"] = 1,
        ["Cosmopolitan"] = 1, ["French 75"] = 3, ["Sangria"] = 9, ["Bloody Mary"] = 1,
        ["Irish Coffee"] = 10, ["Whiskey Sour"] = 1, ["Mai Tai"] = 1,
        ["Long Island Iced Tea"] = 1, ["Tom Collins"] = 1, ["Mint Julep"] = 1
    };
    var catMap = new Dictionary<string, int>
    {
        ["Shot"] = 4, ["Ordinary Drink"] = 1, ["Cocktail"] = 1, ["Punch / Party Drink"] = 1,
        ["Soft Drink"] = 1, ["Homemade Liqueur"] = 6, ["Coffee / Tea"] = 7,
        ["Milk / Float / Shake"] = 1, ["Other/Unknown"] = 1, ["Cocoa"] = 4, ["Beer"] = 10,
        ["Optional alcohol"] = 1
    };
    int[] fallback = { 1, 2, 3, 4, 5, 6, 7, 8 };

    if (!string.IsNullOrEmpty(drink.StrIBA) && ibaMap.TryGetValue(drink.StrIBA.Trim(), out var ibaId)) return ibaId;
    if (!string.IsNullOrEmpty(drink.StrCategory) && catMap.TryGetValue(drink.StrCategory.Trim(), out var catId)) return catId;
    return fallback[random.Next(fallback.Length)];
}

/// <summary>
/// Deduplicates ingredients by name (case-insensitive). First occurrence wins.
/// Prevents PK_CocktailIngredients violation when API returns duplicate ingredients.
/// </summary>
static List<(string Name, string? Measure)> GetIngredientMeasuresDeduplicated(DrinkDetail d)
{
    var pairs = new[]
    {
        (d.StrIngredient1, d.StrMeasure1), (d.StrIngredient2, d.StrMeasure2), (d.StrIngredient3, d.StrMeasure3),
        (d.StrIngredient4, d.StrMeasure4), (d.StrIngredient5, d.StrMeasure5), (d.StrIngredient6, d.StrMeasure6),
        (d.StrIngredient7, d.StrMeasure7), (d.StrIngredient8, d.StrMeasure8), (d.StrIngredient9, d.StrMeasure9),
        (d.StrIngredient10, d.StrMeasure10), (d.StrIngredient11, d.StrMeasure11), (d.StrIngredient12, d.StrMeasure12),
        (d.StrIngredient13, d.StrMeasure13), (d.StrIngredient14, d.StrMeasure14), (d.StrIngredient15, d.StrMeasure15)
    };
    var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    var result = new List<(string Name, string? Measure)>();

    foreach (var (name, measure) in pairs)
    {
        if (string.IsNullOrWhiteSpace(name)) continue;
        var n = name.Trim();
        if (!seen.Add(n)) continue; // skip duplicate
        result.Add((n, string.IsNullOrWhiteSpace(measure) ? null : measure?.Trim()));
    }
    return result;
}
