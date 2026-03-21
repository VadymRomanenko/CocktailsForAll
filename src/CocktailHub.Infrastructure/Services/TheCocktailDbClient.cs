using System.Text.Json;
using System.Text.Json.Serialization;

namespace CocktailHub.Infrastructure.Services;

/// <summary>
/// TheCocktailDB returns "drinks": "no data found" (string) when empty, but we expect List.
/// </summary>
internal sealed class DrinksSummaryOrNullConverter : JsonConverter<List<DrinkSummary>?>
{
    public override List<DrinkSummary>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null) return null;
        if (reader.TokenType == JsonTokenType.String) return null; // "no data found"
        if (reader.TokenType == JsonTokenType.StartArray)
            return JsonSerializer.Deserialize<List<DrinkSummary>>(ref reader, options);
        throw new JsonException($"Unexpected token type: {reader.TokenType}");
    }

    public override void Write(Utf8JsonWriter writer, List<DrinkSummary>? value, JsonSerializerOptions options) { }
}

internal sealed class DrinksDetailOrNullConverter : JsonConverter<List<DrinkDetail>?>
{
    public override List<DrinkDetail>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null) return null;
        if (reader.TokenType == JsonTokenType.String) return null; // "no data found"
        if (reader.TokenType == JsonTokenType.StartArray)
            return JsonSerializer.Deserialize<List<DrinkDetail>>(ref reader, options);
        throw new JsonException($"Unexpected token type: {reader.TokenType}");
    }

    public override void Write(Utf8JsonWriter writer, List<DrinkDetail>? value, JsonSerializerOptions options) { }
}

public class TheCocktailDbClient
{
    private readonly HttpClient _httpClient;
    private const string BaseUrl = "https://www.thecocktaildb.com/api/json/v1/1/";

    public TheCocktailDbClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri(BaseUrl);
    }

    public async Task<IngredientsListResponse?> GetIngredientsListAsync(CancellationToken ct = default)
    {
        var response = await _httpClient.GetAsync("list.php?i=list", ct);
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync(ct);
        return JsonSerializer.Deserialize<IngredientsListResponse>(json);
    }

    public async Task<CategoriesListResponse?> GetCategoriesListAsync(CancellationToken ct = default)
    {
        var response = await _httpClient.GetAsync("list.php?c=list", ct);
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync(ct);
        return JsonSerializer.Deserialize<CategoriesListResponse>(json);
    }

    public async Task<DrinksListResponse?> FilterByCategoryAsync(string category, CancellationToken ct = default)
    {
        var encoded = category.Replace(" ", "_").Replace("/", "_");
        var response = await _httpClient.GetAsync($"filter.php?c={encoded}", ct);
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync(ct);
        return JsonSerializer.Deserialize<DrinksListResponse>(json);
    }

    public async Task<DrinksListResponse?> FilterByIngredientAsync(string ingredient, CancellationToken ct = default)
    {
        var encoded = Uri.EscapeDataString(ingredient);
        var response = await _httpClient.GetAsync($"filter.php?i={encoded}", ct);
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync(ct);
        return JsonSerializer.Deserialize<DrinksListResponse>(json);
    }

    public async Task<DrinksListResponse?> SearchByFirstLetterAsync(char letter, CancellationToken ct = default)
    {
        var response = await _httpClient.GetAsync($"search.php?f={letter}", ct);
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync(ct);
        return JsonSerializer.Deserialize<DrinksListResponse>(json);
    }

    public async Task<DrinksDetailResponse?> LookupCocktailAsync(string drinkId, CancellationToken ct = default)
    {
        var response = await _httpClient.GetAsync($"lookup.php?i={drinkId}", ct);
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync(ct);
        return JsonSerializer.Deserialize<DrinksDetailResponse>(json);
    }

    public static JsonSerializerOptions JsonOptions => new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        Converters = { new JsonStringEnumConverter() }
    };
}

public class IngredientsListResponse
{
    [JsonPropertyName("drinks")]
    public List<IngredientItem>? Drinks { get; set; }
}

public class IngredientItem
{
    [JsonPropertyName("strIngredient1")]
    public string? StrIngredient1 { get; set; }
}

public class CategoriesListResponse
{
    [JsonPropertyName("drinks")]
    public List<CategoryItem>? Drinks { get; set; }
}

public class CategoryItem
{
    [JsonPropertyName("strCategory")]
    public string? StrCategory { get; set; }
}

public class DrinksListResponse
{
    [JsonPropertyName("drinks")]
    [JsonConverter(typeof(DrinksSummaryOrNullConverter))]
    public List<DrinkSummary>? Drinks { get; set; }
}

public class DrinkSummary
{
    [JsonPropertyName("idDrink")]
    public string? IdDrink { get; set; }
}

public class DrinksDetailResponse
{
    [JsonPropertyName("drinks")]
    [JsonConverter(typeof(DrinksDetailOrNullConverter))]
    public List<DrinkDetail>? Drinks { get; set; }
}

public class DrinkDetail
{
    [JsonPropertyName("idDrink")]
    public string? IdDrink { get; set; }
    [JsonPropertyName("strDrink")]
    public string? StrDrink { get; set; }
    [JsonPropertyName("strInstructions")]
    public string? StrInstructions { get; set; }
    [JsonPropertyName("strDrinkThumb")]
    public string? StrDrinkThumb { get; set; }
    [JsonPropertyName("strCategory")]
    public string? StrCategory { get; set; }
    [JsonPropertyName("strIBA")]
    public string? StrIBA { get; set; }
    [JsonPropertyName("strIngredient1")] public string? StrIngredient1 { get; set; }
    [JsonPropertyName("strIngredient2")] public string? StrIngredient2 { get; set; }
    [JsonPropertyName("strIngredient3")] public string? StrIngredient3 { get; set; }
    [JsonPropertyName("strIngredient4")] public string? StrIngredient4 { get; set; }
    [JsonPropertyName("strIngredient5")] public string? StrIngredient5 { get; set; }
    [JsonPropertyName("strIngredient6")] public string? StrIngredient6 { get; set; }
    [JsonPropertyName("strIngredient7")] public string? StrIngredient7 { get; set; }
    [JsonPropertyName("strIngredient8")] public string? StrIngredient8 { get; set; }
    [JsonPropertyName("strIngredient9")] public string? StrIngredient9 { get; set; }
    [JsonPropertyName("strIngredient10")] public string? StrIngredient10 { get; set; }
    [JsonPropertyName("strIngredient11")] public string? StrIngredient11 { get; set; }
    [JsonPropertyName("strIngredient12")] public string? StrIngredient12 { get; set; }
    [JsonPropertyName("strIngredient13")] public string? StrIngredient13 { get; set; }
    [JsonPropertyName("strIngredient14")] public string? StrIngredient14 { get; set; }
    [JsonPropertyName("strIngredient15")] public string? StrIngredient15 { get; set; }
    [JsonPropertyName("strMeasure1")] public string? StrMeasure1 { get; set; }
    [JsonPropertyName("strMeasure2")] public string? StrMeasure2 { get; set; }
    [JsonPropertyName("strMeasure3")] public string? StrMeasure3 { get; set; }
    [JsonPropertyName("strMeasure4")] public string? StrMeasure4 { get; set; }
    [JsonPropertyName("strMeasure5")] public string? StrMeasure5 { get; set; }
    [JsonPropertyName("strMeasure6")] public string? StrMeasure6 { get; set; }
    [JsonPropertyName("strMeasure7")] public string? StrMeasure7 { get; set; }
    [JsonPropertyName("strMeasure8")] public string? StrMeasure8 { get; set; }
    [JsonPropertyName("strMeasure9")] public string? StrMeasure9 { get; set; }
    [JsonPropertyName("strMeasure10")] public string? StrMeasure10 { get; set; }
    [JsonPropertyName("strMeasure11")] public string? StrMeasure11 { get; set; }
    [JsonPropertyName("strMeasure12")] public string? StrMeasure12 { get; set; }
    [JsonPropertyName("strMeasure13")] public string? StrMeasure13 { get; set; }
    [JsonPropertyName("strMeasure14")] public string? StrMeasure14 { get; set; }
    [JsonPropertyName("strMeasure15")] public string? StrMeasure15 { get; set; }
}
