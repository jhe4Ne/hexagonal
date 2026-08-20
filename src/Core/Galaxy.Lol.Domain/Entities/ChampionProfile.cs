using Galaxy.Lol.Domain.Enums;
using Galaxy.Lol.Domain.Events.Domain;
using Galaxy.Lol.Domain.ValueObjects;

namespace Galaxy.Lol.Domain.Entities
{

    public class ChampionProfile : BaseEntity
    {
        public ChampionKey Key { get; private set; } = default!;

        public string ChampionId { get; private set; } = string.Empty;
        public string Name { get; private set; } = string.Empty;
        public string Title { get; private set; } = string.Empty;
        public string? Blurb { get; private set; }
        public string? ImageUrl { get; private set; }
        public string Version { get; private set; } = string.Empty;
        public DifficultyLevel Difficulty { get; private set; } = default!;
        public ChampionStats Stats { get; private set; } = default!;

        private readonly List<ChampionRole> _roles = [];
        public IReadOnlyCollection<ChampionRole> Roles => _roles.AsReadOnly();

        private readonly List<ChampionAbility> _abilities = [];
        public IReadOnlyCollection<ChampionAbility> Abilities => _abilities.AsReadOnly();

        private ChampionProfile() { }

        private ChampionProfile(ChampionKey key, string championId, string name, string title,
                                string? blurb, string? imageUrl, string version,
                                DifficultyLevel difficulty, ChampionStats stats)
        {
            Key = key;
            ChampionId = championId;
            Name = name;
            Title = title;
            Blurb = blurb;
            ImageUrl = imageUrl;
            Version = version;
            Difficulty = difficulty;
            Stats = stats;
        }

        public static ChampionProfile Create(ChampionKey key, string championId, string name, string title,
                                             string? blurb, string? imageUrl, string version,
                                             DifficultyLevel difficulty, ChampionStats stats)
        {
            if (string.IsNullOrWhiteSpace(championId))
                throw new ArgumentException("El championId de Data Dragon es obligatorio.", nameof(championId));
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("El nombre del campeon es obligatorio.", nameof(name));

            return new ChampionProfile(key, championId, name, title, blurb, imageUrl, version, difficulty, stats);
        }

        public void ActualizarFicha(string name, string title, string? blurb, string? imageUrl,
                                    string version, DifficultyLevel difficulty, ChampionStats stats)
        {
            Name = name;
            Title = title;
            Blurb = blurb;
            ImageUrl = imageUrl;
            Version = version;
            Difficulty = difficulty;
            Stats = stats;
            Touch();
        }

        public void DefinirRoles(IEnumerable<ChampionRole> roles)
        {
            _roles.Clear();
            foreach (var rol in roles.Distinct())
                _roles.Add(rol);
            Touch();
        }

        public void ReemplazarHabilidades(IEnumerable<(AbilitySlot Slot, string Name, string? Description, string? ImageUrl, int? Cooldown)> habilidades)
        {
            _abilities.Clear();
            foreach (var h in habilidades)
                _abilities.Add(new ChampionAbility(Id, h.Slot, h.Name, h.Description, h.ImageUrl, h.Cooldown));
            Touch();
        }

        public void NotificarSincronizacion(string version, int totalCampeones) =>
            AddDomainEvent(new ChampionCatalogSyncedDomainEvent(version, totalCampeones));

        public bool EsAptoParaPrincipiante => Difficulty.SuitableForBeginners;

        public bool TieneRol(string rol) => _roles.Any(r => r.Value.Equals(rol, StringComparison.OrdinalIgnoreCase));

        public string RolPrincipal => _roles.FirstOrDefault()?.Value ?? "Sin clasificar";
    }
}
