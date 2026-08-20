namespace Galaxy.Lol.API.Security
{

    public class JwtSettings
    {
        public const string SectionName = "Jwt";

        public string Issuer { get; set; } = "galaxy-lol-hexagonal";
        public string Audience { get; set; } = "galaxy-lol-clients";
        public string SecretKey { get; set; } = string.Empty;
        public int ExpirationMinutes { get; set; } = 60;
    }
}
