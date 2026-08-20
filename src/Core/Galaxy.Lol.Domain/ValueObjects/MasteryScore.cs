using Galaxy.Lol.Domain.Exceptions;

namespace Galaxy.Lol.Domain.ValueObjects
{

    public class MasteryScore : ValueObject
    {
        public long Points { get; init; }
        public int Level { get; init; }

        private MasteryScore() { }

        private MasteryScore(long points, int level)
        {
            if (points < 0) throw new InvalidMasteryScoreException(points);
            Points = points;
            Level = level < 0 ? 0 : level;
        }

        public static MasteryScore Create(long points, int level) => new(points, level);

        public static MasteryScore Zero => new(0, 0);

        public bool EsMaestriaAlta => Level >= 5;

        public static MasteryScore operator +(MasteryScore left, MasteryScore right) =>
            new(left.Points + right.Points, Math.Max(left.Level, right.Level));

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return Points;
            yield return Level;
        }

        public override string ToString() => $"Nivel {Level} - {Points:N0} pts";
    }
}
