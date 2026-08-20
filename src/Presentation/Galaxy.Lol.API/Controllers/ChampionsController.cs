using Asp.Versioning;
using Galaxy.Lol.Application.Features.Champions.DTO;
using Galaxy.Lol.Application.Features.Champions.Ports;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galaxy.Lol.API.Controllers
{

    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [Authorize]
    public class ChampionsController(ChampionUseCases useCases) : BaseApiController
    {

        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] SearchChampionsRequest request, CancellationToken cancellationToken) =>
            HandlerResult(await useCases.Catalog.ExecuteAsync(request, cancellationToken));

        [HttpGet("{championId}")]
        public async Task<IActionResult> GetById(string championId, [FromQuery] string platform = "la1",
                                                 CancellationToken cancellationToken = default) =>
            HandlerResult(await useCases.Detail.ExecuteAsync(championId, platform, cancellationToken));

        [HttpGet("role-distribution")]
        public async Task<IActionResult> GetRoleDistribution([FromQuery] string platform = "la1",
                                                             CancellationToken cancellationToken = default) =>
            HandlerResult(await useCases.RoleDistribution.ExecuteAsync(platform, cancellationToken));
    }
}
