using System.Text.Json.Serialization;

namespace CocktailHub.Infrastructure.SeedData;

/// <summary>
/// Root schema for the static translation file (cocktails-translations.json).
/// Keyed by English cocktail/ingredient name.
/// </summary>
public record TranslationDataRoot
{
    [JsonPropertyName("version")]
    public int Version { get; init; } = 1;

    [JsonPropertyName("generatedAt")]
    public string GeneratedAt { get; init; } = "";

    [JsonPropertyName("cocktails")]
    public List<CocktailTranslationEntry> Cocktails { get; init; } = new();

    [JsonPropertyName("ingredients")]
    public List<IngredientTranslationEntry> Ingredients { get; init; } = new();
}

public record CocktailTranslationEntry
{
    /// <summary>English name — used to look up the cocktail in the DB.</summary>
    [JsonPropertyName("name")]
    public string Name { get; init; } = "";

    [JsonPropertyName("translations")]
    public List<CocktailLangEntry> Translations { get; init; } = new();
}

public record CocktailLangEntry
{
    [JsonPropertyName("lang")]
    public string Lang { get; init; } = "";

    [JsonPropertyName("name")]
    public string Name { get; init; } = "";

    [JsonPropertyName("description")]
    public string? Description { get; init; }

    [JsonPropertyName("instructions")]
    public string Instructions { get; init; } = "";
}

public record IngredientTranslationEntry
{
    /// <summary>English name — used to look up the ingredient in the DB.</summary>
    [JsonPropertyName("name")]
    public string Name { get; init; } = "";

    [JsonPropertyName("translations")]
    public List<IngredientLangEntry> Translations { get; init; } = new();
}

public record IngredientLangEntry
{
    [JsonPropertyName("lang")]
    public string Lang { get; init; } = "";

    [JsonPropertyName("name")]
    public string Name { get; init; } = "";
}
