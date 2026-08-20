namespace Galaxy.Lol.Domain.Model.External
{

    public record DataDragonChampionSummary(
        string ChampionId,
        int Key,
        string Name,
        string Title,
        string? Blurb,
        string? ImageFile,
        IReadOnlyCollection<string> Tags,
        int Difficulty,
        DataDragonStats Stats);

    public record DataDragonStats(
        double Hp,
        double Mp,
        double Armor,
        double SpellBlock,
        double AttackDamage,
        double AttackSpeed,
        double MoveSpeed);

    public record DataDragonChampionDetail(
        DataDragonChampionSummary Summary,
        string? Lore,
        IReadOnlyCollection<DataDragonAbility> Abilities);

    public record DataDragonAbility(
        int SlotIndex,
        string Name,
        string? Description,
        string? ImageFile,
        int? Cooldown);

    public record DataDragonCatalog(
        string Version,
        string Locale,
        IReadOnlyCollection<DataDragonChampionSummary> Champions);
}
