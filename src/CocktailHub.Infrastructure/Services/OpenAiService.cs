using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using CocktailHub.Infrastructure.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CocktailHub.Infrastructure.Services;

public class OpenAiService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<OpenAiService> _logger;
    private readonly string _apiKey;

    private static readonly string[] SupportedLangs = ["en", "uk", "pl"];

    public OpenAiService(HttpClient httpClient, ILogger<OpenAiService> logger, IOptions<CocktailHubOptions> options)
    {
        _httpClient = httpClient;
        _logger = logger;
        _apiKey = options.Value.OpenAiApiKey;
    }

    public bool IsConfigured => !string.IsNullOrWhiteSpace(_apiKey);

    /// <summary>
    /// Calls OpenAI to generate an extended HTML description for the given cocktail name.
    /// Returns a dictionary keyed by lang code ("en", "uk", "pl"), or null on failure.
    /// </summary>
    public async Task<Dictionary<string, string>?> GenerateExtendedDescriptionAsync(
        string cocktailNameInEnglish, CancellationToken ct = default)
    {
        if (!IsConfigured)
        {
            _logger.LogWarning("OpenAI API key is not configured");
            return null;
        }

        var prompt = $$"""
            You are a content writer for "Cocktail Hub", a cocktail discovery website.

            Write an engaging extended description for the cocktail "{{cocktailNameInEnglish}}" to be displayed on the website.

            Respond ONLY with a valid JSON object using this exact schema — no markdown, no code blocks:
            {
              "en": "<p>...</p>",
              "uk": "<p>...</p>",
              "pl": "<p>...</p>"
            }

            Requirements for each language value:
            - Use only these HTML tags: <p>, <strong>, <em>, <ul>, <li>, <h3>
            - Do NOT use <section>, <div>, <html>, <body>, or any markdown
            - Include: origin/history (1–2 sentences), flavor profile (1 sentence), and 1–2 fun facts
            - Length per language: 100–160 words
            - Tone: warm, knowledgeable, accessible to both beginners and cocktail enthusiasts
            - Do NOT start with the cocktail name as a heading
            """;

        var requestBody = new
        {
            model = "gpt-4o-mini",
            response_format = new { type = "json_object" },
            messages = new[]
            {
                new { role = "system", content = "You are a professional cocktail content writer. Always respond with valid JSON only." },
                new { role = "user", content = prompt }
            },
            max_tokens = 1200,
            temperature = 0.7
        };

        try
        {
            var json = JsonSerializer.Serialize(requestBody);
            using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/chat/completions");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request, ct);
            response.EnsureSuccessStatusCode();

            var responseJson = await response.Content.ReadAsStringAsync(ct);
            var doc = JsonDocument.Parse(responseJson);
            var content = doc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();

            if (string.IsNullOrWhiteSpace(content)) return null;

            var result = JsonSerializer.Deserialize<Dictionary<string, string>>(content);
            if (result == null) return null;

            // Keep only expected lang keys, trim whitespace
            return SupportedLangs
                .Where(result.ContainsKey)
                .ToDictionary(l => l, l => result[l].Trim());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "OpenAI request failed for cocktail '{Name}'", cocktailNameInEnglish);
            return null;
        }
    }
}
