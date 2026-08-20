namespace Galaxy.Lol.Application.Features.Champions.DTO
{
    public record ChampionListItemResponse(
        string ChampionId,
        int Key,
        string Name,
        string Title,
        string? ImageUrl,
        string MainRole,
        IReadOnlyCollection<string> Roles,
        int Difficulty,
        string DifficultyCategory,
        bool SuitableForBeginners,
        bool InFreeRotation);

    public record ChampionAbilityResponse(
        string Slot,
        string Name,
        string? Description,
        string? ImageUrl,
        int? Cooldown);

    public record ChampionStatsResponse(
        double Hp,
        double Mp,
        double Armor,
        double SpellBlock,
        double AttackDamage,
        double AttackSpeed,
        double MoveSpeed,
        double Tankiness);

    public record ChampionDetailResponse(
        string ChampionId,
        int Key,
        string Name,
        string Title,
        string? Blurb,
        string? ImageUrl,
        string Version,
        int Difficulty,
        string DifficultyCategory,
        bool SuitableForBeginners,
        bool InFreeRotation,
        IReadOnlyCollection<string> Roles,
        ChampionStatsResponse Stats,
        IReadOnlyCollection<ChampionAbilityResponse> Abilities);

    public record RoleDistributionResponse(
        string Role,
        int Total,
        double AverageDifficulty,
        int InFreeRotation);
}
