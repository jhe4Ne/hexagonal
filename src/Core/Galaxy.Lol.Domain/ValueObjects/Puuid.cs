using Galaxy.Lol.Domain.Exceptions;

namespace Galaxy.Lol.Domain.ValueObjects
{

    public class Puuid : ValueObject
    {
        public string Value { get; init; } = string.Empty;

        private Puuid() { }

        private Puuid(string value)
        {
            var limpio = value?.Trim() ?? string.Empty;
            if (limpio.Length is < 70 or > 80) throw new InvalidPuuidException(value);
            Value = limpio;
        }

        public static Puuid Create(string value) => new(value);

        public string Masked => $"{Value[..4]}...{Value[^4..]}";

        protected override IEnumerable<object?> GetEqualityComponents() { yield return Value; }

        public override string ToString() => Masked;
    }
}
