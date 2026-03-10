namespace CocktailHub.Api.DTOs.Cocktail;

public record CocktailResponse(
    int Id,
    string Name,
    string? Description,
    string Instructions,
    string? ImageUrl,
    int CountryId,
    string CountryName,
    bool IsModerated,
    bool IsFavorite,
    IReadOnlyList<IngredientMeasureDto> Ingredients
);

public record IngredientMeasureDto(int IngredientId, string IngredientName, string? Measure);
