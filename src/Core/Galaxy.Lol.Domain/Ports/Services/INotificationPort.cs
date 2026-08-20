namespace Galaxy.Lol.Domain.Ports.Services
{

    public interface INotificationPort
    {
        Task NotifyRotationChangedAsync(string platform, IReadOnlyCollection<string> championNames,
                                        CancellationToken cancellationToken = default);

        Task NotifyCatalogSyncedAsync(string version, int totalChampions,
                                      CancellationToken cancellationToken = default);
    }
}
