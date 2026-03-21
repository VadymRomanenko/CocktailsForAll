namespace CocktailHub.Core.Entities;

public class IngredientTranslation
{
    public int IngredientId { get; set; }
    public string LangCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;

    public Ingredient Ingredient { get; set; } = null!;
}
