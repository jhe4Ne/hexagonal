using Galaxy.Lol.Application.Common.DTO;

namespace Galaxy.Lol.Application.Features.Champions.DTO
{
    public class SearchChampionsRequest : PagedRequest
    {
        public string? Filter { get; set; }
        public string? Role { get; set; }
        public int? MinDifficulty { get; set; }
        public int? MaxDifficulty { get; set; }

        public bool OnlyFreeRotation { get; set; }
        public string Platform { get; set; } = "la1";
    }
}
