using Asp.Versioning;
using Galaxy.Lol.Application.Features.Masteries.DTO;
using Galaxy.Lol.Application.Features.Masteries.Ports;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galaxy.Lol.API.Controllers
{

    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [Authorize]
    public class MasteriesController(MasteryUseCases useCases) : BaseApiController
    {

        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] GetPlayerMasteryRequest request,
                                             CancellationToken cancellationToken) =>
            HandlerResult(await useCases.Player.ExecuteAsync(request, cancellationToken));

        [HttpGet("top")]
        public async Task<IActionResult> GetTop([FromQuery] GetTopMasteryRequest request,
                                                CancellationToken cancellationToken) =>
            HandlerResult(await useCases.Top.ExecuteAsync(request, cancellationToken));

        [HttpGet("recommendations")]
        public async Task<IActionResult> GetRecommendations([FromQuery] RecommendChampionsRequest request,
                                                            CancellationToken cancellationToken) =>
            HandlerResult(await useCases.Recommend.ExecuteAsync(request, cancellationToken));

        [HttpGet("by-role")]
        public async Task<IActionResult> GetByRole(
            [FromQuery] string gameName, [FromQuery] string tagLine, [FromQuery] string platform = "la1",
            CancellationToken cancellationToken = default) =>
            HandlerResult(await useCases.ByRole.ExecuteAsync(gameName, tagLine, platform, cancellationToken));
    }
}
