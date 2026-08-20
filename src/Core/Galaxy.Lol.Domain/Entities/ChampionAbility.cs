using Galaxy.Lol.Domain.Enums;

namespace Galaxy.Lol.Domain.Entities
{

    public class ChampionAbility : BaseEntity
    {
        public Guid ChampionProfileId { get; private set; }
        public AbilitySlot Slot { get; private set; }
        public string Name { get; private set; } = string.Empty;
        public string? Description { get; private set; }
        public string? ImageUrl { get; private set; }
        public int? Cooldown { get; private set; }

        private ChampionAbility() { }

        internal ChampionAbility(Guid championProfileId, AbilitySlot slot, string name,
                                 string? description, string? imageUrl, int? cooldown)
        {
            ChampionProfileId = championProfileId;
            Slot = slot;
            Name = name;
            Description = description;
            ImageUrl = imageUrl;
            Cooldown = cooldown;
        }
    }
}
