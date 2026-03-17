using System.Text.Json.Serialization;

namespace CocktailHub.Infrastructure.SeedData;

/// <summary>
/// Root schema for static seed data (generated from TheCocktailDB).
/// </summary>
public record SeedDataRoot
{
    [JsonPropertyName("version")]
    public int Version { get; init; } = 1;

    [JsonPropertyName("generatedAt")]
    public string GeneratedAt { get; init; } = "";

    [JsonPropertyName("ingredients")]
    public List<string> Ingredients { get; init; } = new();

    [JsonPropertyName("cocktails")]
    public List<SeedCocktail> Cocktails { get; init; } = new();
}

public record SeedCocktail
{
    [JsonPropertyName("name")]
    public string Name { get; init; } = "";

    [JsonPropertyName("description")]
    public string? Description { get; init; }

    [JsonPropertyName("instructions")]
    public string Instructions { get; init; } = "";

    [JsonPropertyName("imageUrl")]
    public string? ImageUrl { get; init; }

    [JsonPropertyName("countryId")]
    public int CountryId { get; init; }

    [JsonPropertyName("ingredients")]
    public List<SeedCocktailIngredient> Ingredients { get; init; } = new();
}

/// <summary>
/// Per-cocktail ingredient. Names are deduplicated (first occurrence wins).
/// </summary>
public record SeedCocktailIngredient
{
    [JsonPropertyName("name")]
    public string Name { get; init; } = "";

    [JsonPropertyName("measure")]
    public string? Measure { get; init; }
}
