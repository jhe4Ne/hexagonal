namespace Galaxy.Lol.Application.Features.Synchronization.DTO
{
    public class SyncCatalogRequest
    {

        public string? Version { get; set; }
        public string? Locale { get; set; }

        public bool IncludeDetails { get; set; }
    }

    public class SyncRotationRequest
    {
        public string Platform { get; set; } = "la1";
    }

    public record SyncResultResponse(
        string Endpoint,
        string? Version,
        string? Platform,
        int ProcessedRecords,
        int CreatedRecords,
        int UpdatedRecords,
        bool ChangeDetected,
        long ElapsedMilliseconds,
        DateTime ExecutedAt);

    public record SyncLogResponse(
        string Origin,
        string Endpoint,
        string? Platform,
        bool Successful,
        string? Message,
        int ProcessedRecords,
        long ElapsedMilliseconds,
        DateTime ExecutedAt);
}
