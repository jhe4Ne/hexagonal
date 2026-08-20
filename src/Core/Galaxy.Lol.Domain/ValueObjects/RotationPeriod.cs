using Galaxy.Lol.Domain.Exceptions;

namespace Galaxy.Lol.Domain.ValueObjects
{

    public class RotationPeriod : ValueObject
    {
        public DateTime Start { get; init; }
        public DateTime End { get; init; }

        private RotationPeriod() { }

        private RotationPeriod(DateTime start, DateTime end)
        {
            if (end <= start) throw new InvalidRotationPeriodException(start, end);
            Start = start;
            End = end;
        }

        public static RotationPeriod Create(DateTime start, DateTime end) => new(start, end);

        public static RotationPeriod SemanaDe(DateTime momento)
        {
            var dias = ((int)momento.DayOfWeek - (int)DayOfWeek.Tuesday + 7) % 7;
            var inicio = momento.Date.AddDays(-dias);
            return new RotationPeriod(inicio, inicio.AddDays(7));
        }

        public bool Contiene(DateTime momento) => momento >= Start && momento < End;

        public bool EstaVigente(DateTime ahora) => Contiene(ahora);

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return Start;
            yield return End;
        }

        public override string ToString() => $"{Start:yyyy-MM-dd} a {End:yyyy-MM-dd}";
    }
}
