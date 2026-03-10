namespace CocktailHub.Core.Entities;

public class CocktailIngredient
{
    public int CocktailId { get; set; }
    public int IngredientId { get; set; }
    public string? Measure { get; set; }

    public Cocktail Cocktail { get; set; } = null!;
    public Ingredient Ingredient { get; set; } = null!;
}
