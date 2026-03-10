namespace CocktailHub.Core.Entities;

public class Country
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string IsoCode { get; set; } = string.Empty;

    public ICollection<Cocktail> Cocktails { get; set; } = new List<Cocktail>();
}
