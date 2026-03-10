namespace CocktailHub.Api.DTOs.Cocktail;

public record CocktailListResponse(
    int Id,
    string Name,
    string? ImageUrl,
    string CountryName,
    bool IsFavorite
);
