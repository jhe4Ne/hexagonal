namespace Galaxy.Lol.Application.Features.Rotations.DTO
{
    public record RotationChampionResponse(
        int Key,
        string ChampionId,
        string Name,
        string Title,
        string? ImageUrl,
        string MainRole,
        int Difficulty,
        string DifficultyCategory,
        bool ForNewPlayers);

    public record FreeRotationResponse(
        string Platform,
        DateTime PeriodStart,
        DateTime PeriodEnd,
        bool IsCurrent,
        int MaxNewPlayerLevel,
        int TotalChampions,
        string Hash,
        DateTime SyncedAt,
        IReadOnlyCollection<RotationChampionResponse> Champions);
}
