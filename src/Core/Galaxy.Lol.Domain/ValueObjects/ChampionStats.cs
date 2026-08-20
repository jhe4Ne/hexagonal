namespace Galaxy.Lol.Domain.ValueObjects
{

    public class ChampionStats : ValueObject
    {
        public double Hp { get; init; }
        public double Mp { get; init; }
        public double Armor { get; init; }
        public double SpellBlock { get; init; }
        public double AttackDamage { get; init; }
        public double AttackSpeed { get; init; }
        public double MoveSpeed { get; init; }

        private ChampionStats() { }

        private ChampionStats(double hp, double mp, double armor, double spellBlock,
                              double attackDamage, double attackSpeed, double moveSpeed)
        {
            Hp = hp; Mp = mp; Armor = armor; SpellBlock = spellBlock;
            AttackDamage = attackDamage; AttackSpeed = attackSpeed; MoveSpeed = moveSpeed;
        }

        public static ChampionStats Create(double hp, double mp, double armor, double spellBlock,
                                           double attackDamage, double attackSpeed, double moveSpeed) =>
            new(hp, mp, armor, spellBlock, attackDamage, attackSpeed, moveSpeed);

        public static ChampionStats Empty => new(0, 0, 0, 0, 0, 0, 0);

        public double Tankiness => Hp / 100 + Armor + SpellBlock;

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return Hp; yield return Mp; yield return Armor; yield return SpellBlock;
            yield return AttackDamage; yield return AttackSpeed; yield return MoveSpeed;
        }
    }
}
