using Galaxy.Lol.Domain.ValueObjects;

namespace Galaxy.Lol.Domain.Entities
{

    public class ChampionMastery : BaseEntity
    {
        public Guid SummonerId { get; private set; }
        public ChampionKey Key { get; private set; } = default!;
        public MasteryScore Score { get; private set; } = default!;
        public DateTime? LastPlayTime { get; private set; }
        public bool ChestGranted { get; private set; }
        public int TokensEarned { get; private set; }

        private ChampionMastery() { }

        internal ChampionMastery(Guid summonerId, ChampionKey key, MasteryScore score,
                                 DateTime? lastPlayTime, bool chestGranted, int tokensEarned)
        {
            SummonerId = summonerId;
            Key = key;
            Score = score;
            LastPlayTime = lastPlayTime;
            ChestGranted = chestGranted;
            TokensEarned = tokensEarned;
        }

        internal void Actualizar(MasteryScore score, DateTime? lastPlayTime, bool chestGranted, int tokensEarned)
        {
            Score = score;
            LastPlayTime = lastPlayTime;
            ChestGranted = chestGranted;
            TokensEarned = tokensEarned;
            Touch();
        }

        public bool EstaAbandonado(DateTime ahora) =>
            LastPlayTime is null || (ahora - LastPlayTime.Value).TotalDays > 90;
    }
}
