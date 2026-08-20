namespace Galaxy.Lol.Infraestructure.Configuration.Settings
{

    public class RiotApiSettings
    {
        public const string SectionName = "RiotApi";

        public string BaseUrlTemplate { get; set; } = "https://{0}.api.riotgames.com";

        public string DefaultPlatform { get; set; } = "la1";

        public string ApiKey { get; set; } = string.Empty;

        public string ApiKeyHeader { get; set; } = "X-Riot-Token";

        public int TimeoutSeconds { get; set; } = 30;

        public int RequestsPerSecond { get; set; } = 20;

        public int MaxRetries { get; set; } = 3;

        public string BuildBaseUrl(string? platform) =>
            string.Format(BaseUrlTemplate, string.IsNullOrWhiteSpace(platform) ? DefaultPlatform : platform.ToLowerInvariant());

        public string ResolveContinent(string? platform) =>
            (string.IsNullOrWhiteSpace(platform) ? DefaultPlatform : platform.ToLowerInvariant()) switch
            {
                "na1" or "br1" or "la1" or "la2" or "oc1" => "americas",
                "kr" or "jp1" => "asia",
                "euw1" or "eun1" or "tr1" or "ru" => "europe",
                _ => "americas"
            };

        public string BuildAccountBaseUrl(string? platform) =>
            string.Format(BaseUrlTemplate, ResolveContinent(platform));
    }
}
