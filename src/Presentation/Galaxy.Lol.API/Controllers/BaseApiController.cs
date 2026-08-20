using Galaxy.Lol.Application.Results;
using Microsoft.AspNetCore.Mvc;

namespace Galaxy.Lol.API.Controllers
{

    [ApiController]
    public class BaseApiController : ControllerBase
    {
        protected IActionResult HandlerResult(Result result)
        {
            if (result.IsSuccess) return Ok(result);

            var estado = result.ErrorCode != 0 ? result.ErrorCode : StatusCodes.Status400BadRequest;

            if (result.Errors.Count != 0)
                return StatusCode(estado, new { IsSuccess = false, result.Errors, TimeSpan = DateTime.Now });

            return StatusCode(estado, result);
        }
    }
}
