namespace Galaxy.Lol.Application.Common.DTO
{
    public class PagedResult<T>
    {
        public IReadOnlyCollection<T> Items { get; init; } = [];
        public int Page { get; init; }
        public int Rows { get; init; }
        public int TotalRows { get; init; }
        public int TotalPages => Rows <= 0 ? 0 : (int)Math.Ceiling((double)TotalRows / Rows);

        public static PagedResult<T> Create(IReadOnlyCollection<T> items, int page, int rows, int totalRows) =>
            new() { Items = items, Page = page, Rows = rows, TotalRows = totalRows };
    }
}
