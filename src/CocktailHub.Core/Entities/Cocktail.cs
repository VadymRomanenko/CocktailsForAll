namespace CocktailHub.Core.Entities;

public class Cocktail
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Instructions { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public int CountryId { get; set; }
    public bool IsModerated { get; set; }
    public int? CreatedByUserId { get; set; }

    public Country Country { get; set; } = null!;
    public User? CreatedByUser { get; set; }
    public ICollection<CocktailIngredient> CocktailIngredients { get; set; } = new List<CocktailIngredient>();
    public ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();
    public ICollection<CocktailTranslation> Translations { get; set; } = new List<CocktailTranslation>();
}
