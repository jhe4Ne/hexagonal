using Asp.Versioning;
using Galaxy.Lol.Application.Features.Rotations.Ports;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galaxy.Lol.API.Controllers
{

    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [Authorize]
    public class RotationsController(RotationUseCases useCases) : BaseApiController
    {
        [HttpGet("current")]
        public async Task<IActionResult> GetCurrent([FromQuery] string platform = "la1",
                                                    CancellationToken cancellationToken = default) =>
            HandlerResult(await useCases.Current.ExecuteAsync(platform, cancellationToken));

        [HttpGet("history")]
        public async Task<IActionResult> GetHistory([FromQuery] string platform = "la1", [FromQuery] int take = 10,
                                                    CancellationToken cancellationToken = default) =>
            HandlerResult(await useCases.History.ExecuteAsync(platform, take, cancellationToken));
    }
}
