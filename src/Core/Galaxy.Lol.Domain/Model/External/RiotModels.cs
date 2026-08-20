namespace Galaxy.Lol.Domain.Model.External
{

    public record RiotFreeRotation(
        IReadOnlyCollection<int> FreeChampionIds,
        IReadOnlyCollection<int> FreeChampionIdsForNewPlayers,
        int MaxNewPlayerLevel);

    public record RiotChampionMastery(
        int ChampionId,
        int ChampionLevel,
        long ChampionPoints,
        long LastPlayTime,
        bool ChestGranted,
        int TokensEarned);

    public record RiotAccount(string Puuid, string GameName, string TagLine);
}
