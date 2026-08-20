using Galaxy.Lol.Domain.Exceptions;

namespace Galaxy.Lol.Domain.ValueObjects
{

    public class ChampionKey : ValueObject
    {
        public int Value { get; init; }

        private ChampionKey() { }

        private ChampionKey(int value)
        {
            if (value <= 0) throw new InvalidChampionKeyException(value);
            Value = value;
        }

        public static ChampionKey Create(int value) => new(value);

        public static ChampionKey Create(string value) =>
            int.TryParse(value, out var numero) ? new ChampionKey(numero) : throw new InvalidChampionKeyException(0);

        protected override IEnumerable<object?> GetEqualityComponents() { yield return Value; }

        public override string ToString() => Value.ToString();
    }
}
