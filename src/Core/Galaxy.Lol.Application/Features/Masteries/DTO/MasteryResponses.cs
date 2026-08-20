using Galaxy.Lol.Application.Common.DTO;

namespace Galaxy.Lol.Application.Features.Masteries.DTO
{
    public record MasteryItemResponse(
        int ChampionKey,
        string ChampionId,
        string ChampionName,
        string? ImageUrl,
        string MainRole,
        int Level,
        long Points,
        decimal DominanceIndex,
        DateTime? LastPlayTime,
        bool ChestGranted,
        int TokensEarned,
        bool InFreeRotation,
        bool Abandoned);

    public record PlayerMasteryResponse(
        string MaskedPuuid,
        string Platform,
        int TotalChampions,
        long TotalPoints,
        int MaxLevel,
        string DominanceFormula,
        DateTime? LastSyncAt,
        PagedResult<MasteryItemResponse> Masteries);

    public record TopMasteryResponse(
        string MaskedPuuid,
        string Platform,
        IReadOnlyCollection<MasteryItemResponse> Top);

    public record RecommendationResponse(
        int ChampionKey,
        string ChampionId,
        string ChampionName,
        string? ImageUrl,
        string MainRole,
        int Difficulty,
        string DifficultyCategory,
        bool InFreeRotation,
        string Reason,
        int Score);

    public record MasteryByRoleResponse(
        string Role,
        int Champions,
        long TotalPoints,
        int MaxLevel);
}
