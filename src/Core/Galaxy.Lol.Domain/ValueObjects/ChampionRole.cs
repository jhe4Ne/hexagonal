using Galaxy.Lol.Domain.Exceptions;

namespace Galaxy.Lol.Domain.ValueObjects
{

    public class ChampionRole : ValueObject
    {
        private static readonly string[] Catalogo =
            ["Assassin", "Fighter", "Mage", "Marksman", "Support", "Tank"];

        public string Value { get; init; } = string.Empty;

        private ChampionRole() { }

        private ChampionRole(string value)
        {
            var normalizado = Catalogo.FirstOrDefault(r => r.Equals(value?.Trim(), StringComparison.OrdinalIgnoreCase))
                              ?? throw new InvalidChampionRoleException(value ?? string.Empty);
            Value = normalizado;
        }

        public static ChampionRole Create(string value) => new(value);

        public static bool EsValido(string value) =>
            Catalogo.Any(r => r.Equals(value?.Trim(), StringComparison.OrdinalIgnoreCase));

        public static IReadOnlyCollection<string> Disponibles => Catalogo;

        protected override IEnumerable<object?> GetEqualityComponents() { yield return Value; }

        public override string ToString() => Value;
    }
}
