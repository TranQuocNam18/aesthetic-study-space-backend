namespace AestheticStudySpace.Infrastructure.Identity;

public class JwtSettings
{
    public const string SectionName = "Jwt";

    public string Secret { get; set; } = string.Empty;
    public string Issuer { get; set; } = "AestheticStudySpace";
    public string Audience { get; set; } = "AestheticStudySpace.Client";
    public int AccessTokenMinutes { get; set; } = 60;
}
