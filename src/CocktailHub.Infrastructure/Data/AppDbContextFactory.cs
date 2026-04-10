using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CocktailHub.Infrastructure.Data;

/// <summary>
/// Design-time factory so <c>dotnet ef migrations</c> always targets PostgreSQL (existing migrations are Npgsql-specific).
/// </summary>
public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseNpgsql(
            "Host=localhost;Port=5433;Database=cocktailhub;Username=cocktailhub;Password=cocktailhub");
        return new AppDbContext(optionsBuilder.Options);
    }
}
