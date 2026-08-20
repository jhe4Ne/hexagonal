using Galaxy.Lol.Domain.Exceptions;

namespace Galaxy.Lol.Domain.ValueObjects
{

    public class DifficultyLevel : ValueObject
    {
        public int Value { get; init; }

        private DifficultyLevel() { }

        private DifficultyLevel(int value)
        {
            if (value is < 0 or > 10) throw new InvalidDifficultyLevelException(value);
            Value = value;
        }

        public static DifficultyLevel Create(int value) => new(value);

        public string Category => Value switch
        {
            <= 3 => "Baja",
            <= 6 => "Media",
            _ => "Alta"
        };

        public bool SuitableForBeginners => Value <= 4;

        protected override IEnumerable<object?> GetEqualityComponents() { yield return Value; }

        public override string ToString() => $"{Value}/10 ({Category})";
    }
}
