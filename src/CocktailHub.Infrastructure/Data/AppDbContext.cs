using CocktailHub.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace CocktailHub.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Country> Countries => Set<Country>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Ingredient> Ingredients => Set<Ingredient>();
    public DbSet<Cocktail> Cocktails => Set<Cocktail>();
    public DbSet<CocktailIngredient> CocktailIngredients => Set<CocktailIngredient>();
    public DbSet<Favorite> Favorites => Set<Favorite>();
    public DbSet<CocktailTranslation> CocktailTranslations => Set<CocktailTranslation>();
    public DbSet<IngredientTranslation> IngredientTranslations => Set<IngredientTranslation>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Country>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Name).IsUnique();
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Email).IsUnique();
        });

        modelBuilder.Entity<Ingredient>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Name).IsUnique();
        });

        modelBuilder.Entity<Cocktail>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Country)
                .WithMany(c => c.Cocktails)
                .HasForeignKey(e => e.CountryId);
            entity.HasOne(e => e.CreatedByUser)
                .WithMany(u => u.CreatedCocktails)
                .HasForeignKey(e => e.CreatedByUserId)
                .IsRequired(false);
        });

        modelBuilder.Entity<CocktailIngredient>(entity =>
        {
            entity.HasKey(e => new { e.CocktailId, e.IngredientId });
            entity.HasOne(e => e.Cocktail)
                .WithMany(c => c.CocktailIngredients)
                .HasForeignKey(e => e.CocktailId);
            entity.HasOne(e => e.Ingredient)
                .WithMany(i => i.CocktailIngredients)
                .HasForeignKey(e => e.IngredientId);
        });

        modelBuilder.Entity<Favorite>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.CocktailId });
            entity.HasOne(e => e.User)
                .WithMany(u => u.Favorites)
                .HasForeignKey(e => e.UserId);
            entity.HasOne(e => e.Cocktail)
                .WithMany(c => c.Favorites)
                .HasForeignKey(e => e.CocktailId);
        });

        modelBuilder.Entity<CocktailTranslation>(entity =>
        {
            entity.HasKey(e => new { e.CocktailId, e.LangCode });
            entity.HasOne(e => e.Cocktail)
                .WithMany(c => c.Translations)
                .HasForeignKey(e => e.CocktailId);
        });

        modelBuilder.Entity<IngredientTranslation>(entity =>
        {
            entity.HasKey(e => new { e.IngredientId, e.LangCode });
            entity.HasOne(e => e.Ingredient)
                .WithMany(i => i.Translations)
                .HasForeignKey(e => e.IngredientId);
        });

        SeedData(modelBuilder);
    }

    private static void SeedData(ModelBuilder modelBuilder)
    {
        var adminPasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!");

        modelBuilder.Entity<User>().HasData(new User
        {
            Id = 1,
            Email = "admin@cocktailhub.com",
            PasswordHash = adminPasswordHash,
            Role = UserRole.Admin
        });

        var countries = new[]
        {
            new Country { Id = 1, Name = "USA", IsoCode = "US" },
            new Country { Id = 2, Name = "United Kingdom", IsoCode = "GB" },
            new Country { Id = 3, Name = "France", IsoCode = "FR" },
            new Country { Id = 4, Name = "Mexico", IsoCode = "MX" },
            new Country { Id = 5, Name = "Cuba", IsoCode = "CU" },
            new Country { Id = 6, Name = "Italy", IsoCode = "IT" },
            new Country { Id = 7, Name = "Japan", IsoCode = "JP" },
            new Country { Id = 8, Name = "ruZZia", IsoCode = "RU" },
            new Country { Id = 9, Name = "Spain", IsoCode = "ES" },
            new Country { Id = 10, Name = "Ireland", IsoCode = "IE" },
            new Country { Id = 11, Name = "Brazil", IsoCode = "BR" },
            new Country { Id = 12, Name = "Germany", IsoCode = "DE" },
            new Country { Id = 13, Name = "Poland", IsoCode = "PL" },
            new Country { Id = 14, Name = "Ukraine", IsoCode = "UA" },
            new Country { Id = 15, Name = "Greece", IsoCode = "GR" },
            new Country { Id = 16, Name = "Portugal", IsoCode = "PT" },
            new Country { Id = 17, Name = "Argentina", IsoCode = "AR" },
            new Country { Id = 18, Name = "Colombia", IsoCode = "CO" },
            new Country { Id = 19, Name = "Jamaica", IsoCode = "JM" },
            new Country { Id = 20, Name = "Thailand", IsoCode = "TH" },
            new Country { Id = 21, Name = "China", IsoCode = "CN" },
            new Country { Id = 22, Name = "India", IsoCode = "IN" },
            new Country { Id = 23, Name = "Australia", IsoCode = "AU" },
            new Country { Id = 24, Name = "Canada", IsoCode = "CA" },
            new Country { Id = 25, Name = "Netherlands", IsoCode = "NL" },
            new Country { Id = 26, Name = "Belgium", IsoCode = "BE" },
            new Country { Id = 27, Name = "Austria", IsoCode = "AT" },
            new Country { Id = 28, Name = "Sweden", IsoCode = "SE" },
            new Country { Id = 29, Name = "Norway", IsoCode = "NO" },
            new Country { Id = 30, Name = "Denmark", IsoCode = "DK" },
            new Country { Id = 31, Name = "Finland", IsoCode = "FI" },
            new Country { Id = 32, Name = "Switzerland", IsoCode = "CH" },
            new Country { Id = 33, Name = "Czech Republic", IsoCode = "CZ" },
        };

        modelBuilder.Entity<Country>().HasData(countries);
    }
}
