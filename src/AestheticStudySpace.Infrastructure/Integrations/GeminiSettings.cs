namespace AestheticStudySpace.Infrastructure.Integrations;

public class GeminiSettings
{
    public const string SectionName = "Gemini";
    public string ApiKey { get; set; } = string.Empty;
    public string Model { get; set; } = "gemini-1.5-flash";
}
