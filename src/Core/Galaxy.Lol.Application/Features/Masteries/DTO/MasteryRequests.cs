using Galaxy.Lol.Application.Common.DTO;

namespace Galaxy.Lol.Application.Features.Masteries.DTO
{
    public class GetPlayerMasteryRequest : PagedRequest
    {
        public string GameName { get; set; } = string.Empty;
        public string TagLine { get; set; } = string.Empty;
        public string Platform { get; set; } = "la1";

        public bool Refresh { get; set; }
    }

    public class GetTopMasteryRequest
    {
        public string GameName { get; set; } = string.Empty;
        public string TagLine { get; set; } = string.Empty;
        public string Platform { get; set; } = "la1";
        public int Count { get; set; } = 5;
        public bool Refresh { get; set; }
    }

    public class RecommendChampionsRequest
    {
        public string? GameName { get; set; }
        public string? TagLine { get; set; }
        public string Platform { get; set; } = "la1";
        public int Count { get; set; } = 10;
        public bool OnlyBeginnerFriendly { get; set; }
    }
}
