using Galaxy.Lol.Domain.ValueObjects;

namespace Galaxy.Lol.Domain.Entities
{

    public class FreeRotationEntry : BaseEntity
    {
        public Guid FreeRotationId { get; private set; }
        public ChampionKey Key { get; private set; } = default!;

        public bool ForNewPlayers { get; private set; }

        private FreeRotationEntry() { }

        internal FreeRotationEntry(Guid freeRotationId, ChampionKey key, bool forNewPlayers)
        {
            FreeRotationId = freeRotationId;
            Key = key;
            ForNewPlayers = forNewPlayers;
        }
    }
}
