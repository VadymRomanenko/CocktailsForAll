using System.Text.Json;
using CocktailHub.Infrastructure.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CocktailHub.Infrastructure.Services;

public class TranslationService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<TranslationService>? _logger;
    private readonly string _email;
    public const string ERROR_STRING = "error";

    /// <summary>Used by the DI container (API / Infrastructure).</summary>
    [ActivatorUtilitiesConstructor]
    public TranslationService(HttpClient httpClient, ILogger<TranslationService> logger, IOptions<CocktailHubOptions> options)
        : this(httpClient, options.Value.TranslationEmail)
    {
        _logger = logger;
    }

    /// <summary>Used directly by CLI tools that don't have a DI container.</summary>
    public TranslationService(HttpClient httpClient, string email = "")
    {
        _httpClient = httpClient;
        _logger = null!;
        _email = email;
    }

    public async Task<string?> TranslateAsync(string text, string fromLang, string toLang, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(text) || fromLang == toLang) return text;

        try
        {
            var pair = $"{fromLang}|{toLang}";
            var emailParam = string.IsNullOrWhiteSpace(_email) ? "" : $"&de={Uri.EscapeDataString(_email)}";
            var url = $"https://api.mymemory.translated.net/get?q={Uri.EscapeDataString(text)}&langpair={pair}{emailParam}";
            var response = await _httpClient.GetAsync(url, ct);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync(ct);
            var doc = JsonDocument.Parse(json);
            var result = doc.RootElement
                .GetProperty("responseData")
                .GetProperty("translatedText")
                .GetString();
            return result ?? text;
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "Translation failed for {Text}", text[..Math.Min(50, text.Length)]);
            Console.Error.WriteLine($"Translation failed: {ex.Message}");
            return ERROR_STRING;
        }
    }
}
