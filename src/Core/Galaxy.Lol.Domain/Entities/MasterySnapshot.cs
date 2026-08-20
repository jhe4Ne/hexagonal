namespace Galaxy.Lol.Domain.Entities
{

    public class MasterySnapshot : BaseEntity
    {
        public string MaskedPuuid { get; private set; } = string.Empty;
        public int ChampionKey { get; private set; }
        public string ChampionName { get; private set; } = string.Empty;
        public long Points { get; private set; }
        public int Level { get; private set; }
        public decimal DominanceIndex { get; private set; }
        public DateTime TakenAt { get; private set; }

        private MasterySnapshot() { }

        private MasterySnapshot(string maskedPuuid, int championKey, string championName,
                                long points, int level, decimal dominanceIndex)
        {
            MaskedPuuid = maskedPuuid;
            ChampionKey = championKey;
            ChampionName = championName;
            Points = points;
            Level = level;
            DominanceIndex = dominanceIndex;
            TakenAt = DateTime.UtcNow;
        }

        public static MasterySnapshot Create(string maskedPuuid, int championKey, string championName,
                                             long points, int level, decimal dominanceIndex) =>
            new(maskedPuuid, championKey, championName, points, level, dominanceIndex);
    }
}
