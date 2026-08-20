using Galaxy.Lol.Domain.Events.Domain;
using Galaxy.Lol.Domain.ValueObjects;

namespace Galaxy.Lol.Domain.Entities
{

    public class Summoner : BaseEntity
    {
        public Puuid Puuid { get; private set; } = default!;
        public string? GameName { get; private set; }
        public string? TagLine { get; private set; }
        public string Platform { get; private set; } = string.Empty;
        public DateTime? LastSyncAt { get; private set; }

        private readonly List<ChampionMastery> _masteries = [];
        public IReadOnlyCollection<ChampionMastery> Masteries => _masteries.AsReadOnly();

        private Summoner() { }

        private Summoner(Puuid puuid, string platform, string? gameName, string? tagLine)
        {
            Puuid = puuid;
            Platform = platform;
            GameName = gameName;
            TagLine = tagLine;
        }

        public static Summoner Create(Puuid puuid, string platform, string? gameName = null, string? tagLine = null)
        {
            if (string.IsNullOrWhiteSpace(platform))
                throw new ArgumentException("La plataforma es obligatoria.", nameof(platform));

            return new Summoner(puuid, platform, gameName, tagLine);
        }

        public ChampionMastery RegistrarMaestria(ChampionKey key, MasteryScore score,
                                                 DateTime? lastPlayTime, bool chestGranted, int tokensEarned)
        {
            AddDomainEvent(new MasteryRecordedDomainEvent(Id, key.Value, score.Points));

            var existente = _masteries.FirstOrDefault(m => m.Key == key);
            if (existente is not null)
            {
                existente.Actualizar(score, lastPlayTime, chestGranted, tokensEarned);
                return existente;
            }

            var nueva = new ChampionMastery(Id, key, score, lastPlayTime, chestGranted, tokensEarned);
            _masteries.Add(nueva);
            return nueva;
        }

        public void MarcarSincronizado()
        {
            LastSyncAt = DateTime.UtcNow;
            Touch();
        }

        public void ActualizarIdentidad(string? gameName, string? tagLine)
        {
            GameName = gameName;
            TagLine = tagLine;
            Touch();
        }

        public MasteryScore PuntajeMaximo =>
            _masteries.Count == 0 ? MasteryScore.Zero : _masteries.MaxBy(m => m.Score.Points)!.Score;

        public bool HaJugado(ChampionKey key) => _masteries.Any(m => m.Key == key);

        public string NombreCompleto =>
            string.IsNullOrWhiteSpace(GameName) ? Puuid.Masked : $"{GameName}#{TagLine}";
    }
}
