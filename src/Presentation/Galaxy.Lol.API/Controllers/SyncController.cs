using Asp.Versioning;
using Galaxy.Lol.Application.Features.Synchronization.DTO;
using Galaxy.Lol.Application.Features.Synchronization.Ports;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galaxy.Lol.API.Controllers
{

    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [Authorize]
    public class SyncController(SyncUseCases useCases) : BaseApiController
    {

        [HttpPost("catalog")]
        public async Task<IActionResult> SyncCatalog([FromBody] SyncCatalogRequest request,
                                                     CancellationToken cancellationToken) =>
            HandlerResult(await useCases.Catalog.ExecuteAsync(request, cancellationToken));

        [HttpPost("rotation")]
        public async Task<IActionResult> SyncRotation([FromBody] SyncRotationRequest request,
                                                      CancellationToken cancellationToken) =>
            HandlerResult(await useCases.Rotation.ExecuteAsync(request, cancellationToken));

        [HttpGet("history")]
        public async Task<IActionResult> GetHistory([FromQuery] int take = 50, CancellationToken cancellationToken = default) =>
            HandlerResult(await useCases.History.ExecuteAsync(take, cancellationToken));
    }
}
