namespace CocktailHub.Infrastructure.Options;

public class CocktailHubOptions
{
    public bool SeedTranslations { get; set; }

    /// <summary>
    /// Email sent to MyMemory API as the 'de' parameter to raise the daily limit
    /// from 5,000 to 50,000 characters. Leave empty to use the anonymous limit.
    /// </summary>
    public string TranslationEmail { get; set; } = "";

    /// <summary>OpenAI API key for generating extended cocktail descriptions.</summary>
    public string OpenAiApiKey { get; set; } = "";
}
