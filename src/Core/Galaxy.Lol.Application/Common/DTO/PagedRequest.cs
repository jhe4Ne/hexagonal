namespace Galaxy.Lol.Application.Common.DTO
{

    public class PagedRequest
    {
        private const int FilasMaximas = 100;

        public int Page { get; set; } = 1;

        private int _rows = 10;
        public int Rows
        {
            get => _rows;

            set => _rows = value switch { <= 0 => 10, > FilasMaximas => FilasMaximas, _ => value };
        }
    }
}
