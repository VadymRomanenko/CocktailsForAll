namespace CocktailHub.Core.Entities;

public class Ingredient
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public ICollection<CocktailIngredient> CocktailIngredients { get; set; } = new List<CocktailIngredient>();
}
