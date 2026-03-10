namespace CocktailHub.Core.Entities;

public enum UserRole
{
    User,
    Admin
}

public class User
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; }

    public ICollection<Cocktail> CreatedCocktails { get; set; } = new List<Cocktail>();
    public ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();
}
