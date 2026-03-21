# Seed Data Generator

Fetches cocktail data from [TheCocktailDB](https://www.thecocktaildb.com/) and generates a static JSON file for seeding the database.

## Usage

```bash
cd d:\Work\CocktailsForAll
dotnet run --project tools/SeedDataGenerator/SeedDataGenerator.csproj
```

Optional limit: `dotnet run --project tools/SeedDataGenerator/SeedDataGenerator.csproj -- 100`

Output: `src/CocktailHub.Infrastructure/SeedData/cocktails.json`

**Note:** Default 500 cocktails, ~200ms per drink. Use a smaller limit (e.g. 50) for quick runs.

## When to run

- Before first deploy to populate initial seed data
- When you want to refresh the cocktail catalog from TheCocktailDB

## Seeding flow

1. **JSON exists and has cocktails** → Import from JSON (deterministic, no API calls)
2. **JSON missing or empty** → Fetch from TheCocktailDB API at runtime
