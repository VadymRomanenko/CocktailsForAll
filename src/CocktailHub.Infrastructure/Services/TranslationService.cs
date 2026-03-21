using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace CocktailHub.Infrastructure.Services;

public class TranslationService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<TranslationService> _logger;

    public TranslationService(HttpClient httpClient, ILogger<TranslationService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<string?> TranslateAsync(string text, string fromLang, string toLang, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(text) || fromLang == toLang) return text;

        try
        {
            var pair = $"{fromLang}|{toLang}";
            var url = $"https://api.mymemory.translated.net/get?q={Uri.EscapeDataString(text)}&langpair={pair}";
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
            _logger.LogWarning(ex, "Translation failed for {Text}", text[..Math.Min(50, text.Length)]);
            return text;
        }
    }
}
