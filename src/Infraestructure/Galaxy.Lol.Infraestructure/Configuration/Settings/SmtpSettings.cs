namespace Galaxy.Lol.Infraestructure.Configuration.Settings
{
    public class SmtpSettings
    {
        public const string SectionName = "Smtp";

        public string Host { get; set; } = "localhost";
        public int Port { get; set; } = 1025;
        public bool UseSsl { get; set; }
        public string? User { get; set; }
        public string? Password { get; set; }
        public string From { get; set; } = "no-reply@galaxy-lol.local";
        public string FromName { get; set; } = "Galaxy LoL Champions";
        public string To { get; set; } = "equipo@galaxy-lol.local";
        public bool Enabled { get; set; } = true;
    }
}
