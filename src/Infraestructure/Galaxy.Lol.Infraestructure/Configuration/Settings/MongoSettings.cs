namespace Galaxy.Lol.Infraestructure.Configuration.Settings
{
    public class MongoSettings
    {
        public const string SectionName = "Mongo";

        public string ConnectionString { get; set; } = string.Empty;
        public string Database { get; set; } = "lol_raw_cache";
        public string CatalogCollection { get; set; } = "champion_catalog";
        public string DetailCollection { get; set; } = "champion_detail";
    }
}
