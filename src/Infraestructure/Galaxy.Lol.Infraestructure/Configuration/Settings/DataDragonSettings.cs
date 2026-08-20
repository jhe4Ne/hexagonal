namespace Galaxy.Lol.Infraestructure.Configuration.Settings
{

    public class DataDragonSettings
    {
        public const string SectionName = "DataDragon";

        public string BaseUrl { get; set; } = "https://ddragon.leagueoflegends.com";

        public string DefaultLocale { get; set; } = "es_MX";

        public string? PinnedVersion { get; set; }

        public int TimeoutSeconds { get; set; } = 30;
    }
}
