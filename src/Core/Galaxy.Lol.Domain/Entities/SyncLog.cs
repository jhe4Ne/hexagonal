using Galaxy.Lol.Domain.Enums;

namespace Galaxy.Lol.Domain.Entities
{

    public class SyncLog : BaseEntity
    {
        public SyncOrigin Origin { get; private set; }
        public string Endpoint { get; private set; } = string.Empty;
        public string? Platform { get; private set; }
        public bool Successful { get; private set; }
        public string? Message { get; private set; }
        public int ProcessedRecords { get; private set; }
        public long ElapsedMilliseconds { get; private set; }
        public DateTime ExecutedAt { get; private set; }

        private SyncLog() { }

        private SyncLog(SyncOrigin origin, string endpoint, string? platform, bool successful,
                        string? message, int processedRecords, long elapsedMilliseconds)
        {
            Origin = origin;
            Endpoint = endpoint;
            Platform = platform;
            Successful = successful;
            Message = message;
            ProcessedRecords = processedRecords;
            ElapsedMilliseconds = elapsedMilliseconds;
            ExecutedAt = DateTime.UtcNow;
        }

        public static SyncLog Exito(SyncOrigin origin, string endpoint, string? platform,
                                    int processedRecords, long elapsedMilliseconds) =>
            new(origin, endpoint, platform, true, null, processedRecords, elapsedMilliseconds);

        public static SyncLog Fallo(SyncOrigin origin, string endpoint, string? platform,
                                    string message, long elapsedMilliseconds) =>
            new(origin, endpoint, platform, false, message, 0, elapsedMilliseconds);
    }
}
