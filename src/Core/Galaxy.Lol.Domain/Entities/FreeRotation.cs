using System.Security.Cryptography;
using System.Text;
using Galaxy.Lol.Domain.Events.Domain;
using Galaxy.Lol.Domain.ValueObjects;

namespace Galaxy.Lol.Domain.Entities
{

    public class FreeRotation : BaseEntity
    {
        public string Platform { get; private set; } = string.Empty;
        public RotationPeriod Period { get; private set; } = default!;
        public int MaxNewPlayerLevel { get; private set; }
        public string Hash { get; private set; } = string.Empty;

        private readonly List<FreeRotationEntry> _entries = [];
        public IReadOnlyCollection<FreeRotationEntry> Entries => _entries.AsReadOnly();

        private FreeRotation() { }

        private FreeRotation(string platform, RotationPeriod period, int maxNewPlayerLevel)
        {
            Platform = platform;
            Period = period;
            MaxNewPlayerLevel = maxNewPlayerLevel;
        }

        public static FreeRotation Create(string platform, RotationPeriod period, int maxNewPlayerLevel,
                                          IEnumerable<int> championKeys, IEnumerable<int> newPlayerChampionKeys)
        {
            if (string.IsNullOrWhiteSpace(platform))
                throw new ArgumentException("La plataforma es obligatoria.", nameof(platform));

            var rotacion = new FreeRotation(platform, period, maxNewPlayerLevel);

            foreach (var key in championKeys.Distinct().OrderBy(k => k))
                rotacion._entries.Add(new FreeRotationEntry(rotacion.Id, ChampionKey.Create(key), false));

            foreach (var key in newPlayerChampionKeys.Distinct().OrderBy(k => k))
                rotacion._entries.Add(new FreeRotationEntry(rotacion.Id, ChampionKey.Create(key), true));

            rotacion.Hash = CalcularHash(rotacion._entries);
            rotacion.AddDomainEvent(new FreeRotationChangedDomainEvent(
                platform,
                rotacion._entries.Where(e => !e.ForNewPlayers).Select(e => e.Key.Value).ToList()));

            return rotacion;
        }

        private static string CalcularHash(IEnumerable<FreeRotationEntry> entries)
        {
            var texto = string.Join(",", entries
                .OrderBy(e => e.ForNewPlayers).ThenBy(e => e.Key.Value)
                .Select(e => $"{(e.ForNewPlayers ? "N" : "G")}{e.Key.Value}"));

            return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(texto)));
        }

        public bool TieneMismoContenidoQue(FreeRotation? otra) => otra is not null && otra.Hash == Hash;

        public bool Contiene(ChampionKey key) => _entries.Any(e => e.Key == key);

        public IReadOnlyCollection<int> ClavesGenerales =>
            _entries.Where(e => !e.ForNewPlayers).Select(e => e.Key.Value).ToList().AsReadOnly();

        public IReadOnlyCollection<int> ClavesParaNovatos =>
            _entries.Where(e => e.ForNewPlayers).Select(e => e.Key.Value).ToList().AsReadOnly();

        public bool EstaVigente(DateTime ahora) => Period.EstaVigente(ahora);
    }
}
