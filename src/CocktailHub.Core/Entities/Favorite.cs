namespace CocktailHub.Core.Entities;

public class Favorite
{
    public int UserId { get; set; }
    public int CocktailId { get; set; }

    public User User { get; set; } = null!;
    public Cocktail Cocktail { get; set; } = null!;
}
