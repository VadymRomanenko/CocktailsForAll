using CocktailHub.Core.Entities;
using CocktailHub.Infrastructure.SeedData;
using CocktailHub.Infrastructure.Services;
using System.Text.Json;

var limit = args.Length > 0 && int.TryParse(args[0], out var n) ? n : 5000;
var translationEmail = await ReadTranslationEmailAsync();
if (args.Length > 1) translationEmail = args[1]; // CLI arg overrides config
var langs = new[] { "uk", "pl" };

var outputPath = SeedDataPath("cocktails.json");
Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);

SeedDataRoot seedData;

if (File.Exists(outputPath))
{
    Console.WriteLine($"cocktails.json already exists — loading from file, skipping API fetch.");
    seedData = JsonSerializer.Deserialize<SeedDataRoot>(await File.ReadAllTextAsync(outputPath))!;
}
else
{
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

    seedData = new SeedDataRoot
    {
        Version = 1,
        GeneratedAt = DateTime.UtcNow.ToString("O"),
        Ingredients = allIngredientNames.OrderBy(x => x, StringComparer.OrdinalIgnoreCase).ToList(),
        Cocktails = cocktails
    };

    await using var fs = File.Create(outputPath);
    await JsonSerializer.SerializeAsync(fs, seedData, new JsonSerializerOptions { WriteIndented = true });
    Console.WriteLine($"\nWrote {seedData.Cocktails.Count} cocktails and {seedData.Ingredients.Count} ingredients to {outputPath}");
}

Console.WriteLine($"Loaded {seedData.Cocktails.Count} cocktails and {seedData.Ingredients.Count} ingredients.");

// ── Translation generation ─────────────────────────────────────────────────

var translationsPath = SeedDataPath("cocktails-translations.json");
TranslationDataRoot? existingRoot = null;
if (File.Exists(translationsPath))
{
    var existingJson = await File.ReadAllTextAsync(translationsPath);
    if (!string.IsNullOrWhiteSpace(existingJson))
        existingRoot = JsonSerializer.Deserialize<TranslationDataRoot>(existingJson);
}

var alreadyTranslatedCocktails = existingRoot?.Cocktails
    .Where(x => !String.Equals(x.Translations[0].Name, x.Translations[1].Name))
    .Select(e => e.Name).ToHashSet(StringComparer.OrdinalIgnoreCase) ?? new HashSet<string>(StringComparer.OrdinalIgnoreCase);
var alreadyTranslatedIngredients = existingRoot?.Ingredients
    .Where(x => !String.Equals(x.Translations[0].Name, x.Translations[1].Name))
    .Select(e => e.Name).ToHashSet(StringComparer.OrdinalIgnoreCase) ?? new HashSet<string>(StringComparer.OrdinalIgnoreCase);

var cocktailEntries = existingRoot?.Cocktails.ToList() ?? new List<CocktailTranslationEntry>();
var ingredientEntries = existingRoot?.Ingredients.ToList() ?? new List<IngredientTranslationEntry>();

var translationHttp = new HttpClient();
var translator = new TranslationService(translationHttp, translationEmail);

var pendingCocktails = seedData.Cocktails.Where(c => !alreadyTranslatedCocktails.Contains(c.Name)).ToList();
var pendingIngredients = seedData.Ingredients.Where(n => !alreadyTranslatedIngredients.Contains(n)).ToList();

Console.WriteLine($"\nTranslating {pendingCocktails.Count} new cocktails and {pendingIngredients.Count} new ingredients into {string.Join(", ", langs)}...");
if (string.IsNullOrWhiteSpace(translationEmail))
    Console.WriteLine("No email provided — using anonymous limit (5,000 chars/day). Pass email as 2nd arg for 50,000 chars/day.");

var done = 0;
foreach (var cocktail in pendingCocktails)
{
    var langEntries = new List<CocktailLangEntry>();
    var errorOccured = false;
    foreach (var lang in langs)
    {
        var name = await translator.TranslateAsync(cocktail.Name, "en", lang) ?? cocktail.Name;
        if (String.Equals(name, TranslationService.ERROR_STRING, StringComparison.OrdinalIgnoreCase))
        {
            errorOccured = true;
            break; // Skip further translations for this cocktail if name translation failed
        }
        await Task.Delay(600);
        var desc = string.IsNullOrWhiteSpace(cocktail.Description) ? null
            : await translator.TranslateAsync(cocktail.Description, "en", lang);
        if (String.Equals(desc, TranslationService.ERROR_STRING, StringComparison.OrdinalIgnoreCase))
        {
            errorOccured = true;
            break;
        }
        if (desc != null) await Task.Delay(600);
        var instr = await translator.TranslateAsync(cocktail.Instructions, "en", lang) ?? cocktail.Instructions;
        if (String.Equals(instr, TranslationService.ERROR_STRING, StringComparison.OrdinalIgnoreCase))
        {
            errorOccured = true;
            break;
        }
        await Task.Delay(600);

        langEntries.Add(new CocktailLangEntry
        {
            Lang = lang,
            Name = name,
            Description = desc,
            Instructions = instr
        });
    }
    if (errorOccured)
    {
        Console.WriteLine($"  Error occurred while translating '{cocktail.Name}' -> it seems tranlation resources are depleted — skipping all next cocktails.");
        await SaveTranslationsAsync(translationsPath, cocktailEntries, ingredientEntries);
        break;
    }
    var existingItem = cocktailEntries.FirstOrDefault(x => String.Equals(x.Name, cocktail.Name, StringComparison.OrdinalIgnoreCase));
    if (existingItem != null)
    {
        cocktailEntries.Remove(existingItem);
    }
    cocktailEntries.Add(new CocktailTranslationEntry { Name = cocktail.Name, Translations = langEntries });
    done++;

    if (done % 10 == 0)
    {
        await SaveTranslationsAsync(translationsPath, cocktailEntries, ingredientEntries);
        Console.WriteLine($"  Checkpoint saved ({done}/{pendingCocktails.Count} cocktails)");
    }
}

done = 0;
foreach (var ingName in pendingIngredients)
{
    var errorOccured = false;
    var langEntries = new List<IngredientLangEntry>();
    foreach (var lang in langs)
    {
        var translated = await translator.TranslateAsync(ingName, "en", lang) ?? ingName;
        if (String.Equals(translated, TranslationService.ERROR_STRING, StringComparison.OrdinalIgnoreCase))
        {
            errorOccured = true;
            break;
        }
        await Task.Delay(400);
        langEntries.Add(new IngredientLangEntry { Lang = lang, Name = translated });
    }
    if (errorOccured)
    {
        Console.WriteLine($"  Error occurred while translating '{ingName}' -> it seems tranlation resources are depleted — skipping all next ingredients.");
        await SaveTranslationsAsync(translationsPath, cocktailEntries, ingredientEntries);
        break;
    }
    var existingItem = ingredientEntries.FirstOrDefault(x => String.Equals(x.Name, ingName, StringComparison.OrdinalIgnoreCase));
    if (existingItem != null)
    {
        ingredientEntries.Remove(existingItem);
    }
    ingredientEntries.Add(new IngredientTranslationEntry { Name = ingName, Translations = langEntries });
    done++;

    if (done % 20 == 0)
    {
        await SaveTranslationsAsync(translationsPath, cocktailEntries, ingredientEntries);
        Console.WriteLine($"  Checkpoint saved ({done}/{pendingIngredients.Count} ingredients)");
    }
}

await SaveTranslationsAsync(translationsPath, cocktailEntries, ingredientEntries);
Console.WriteLine($"\nTranslation file written to {translationsPath}");
return;

// ── Helpers ────────────────────────────────────────────────────────────────

static async Task<string> ReadTranslationEmailAsync()
{
    var configPath = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
    if (!File.Exists(configPath)) return "";
    var doc = JsonDocument.Parse(await File.ReadAllTextAsync(configPath));
    return doc.RootElement.TryGetProperty("TranslationEmail", out var prop)
        ? prop.GetString() ?? ""
        : "";
}

static string SeedDataPath(string fileName) =>
    Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..",
        "src", "CocktailHub.Infrastructure", "SeedData", fileName));

static async Task SaveTranslationsAsync(string path, List<CocktailTranslationEntry> cocktails, List<IngredientTranslationEntry> ingredients)
{
    var root = new TranslationDataRoot
    {
        Version = 1,
        GeneratedAt = DateTime.UtcNow.ToString("O"),
        Cocktails = cocktails,
        Ingredients = ingredients
    };
    await using var fs = File.Create(path);
    await JsonSerializer.SerializeAsync(fs, root, new JsonSerializerOptions { WriteIndented = true });
}

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
        if (!seen.Add(n)) continue;
        result.Add((n, string.IsNullOrWhiteSpace(measure) ? null : measure?.Trim()));
    }
    return result;
}
