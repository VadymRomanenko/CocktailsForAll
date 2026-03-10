namespace CocktailHub.Api.DTOs.Cocktail;

public record CreateCocktailRequest(
    string Name,
    string? Description,
    string Instructions,
    string? ImageUrl,
    int CountryId,
    IReadOnlyList<CocktailIngredientInput> Ingredients
);

public record CocktailIngredientInput(int IngredientId, string? Measure);
