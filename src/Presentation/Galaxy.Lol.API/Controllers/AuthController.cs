using Asp.Versioning;
using Galaxy.Lol.API.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galaxy.Lol.API.Controllers
{

    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [AllowAnonymous]
    public class AuthController(JwtTokenGenerator generator) : BaseApiController
    {
        public record TokenRequest(string User);

        [HttpPost("token")]
        public IActionResult GetToken([FromBody] TokenRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.User))
                return BadRequest(new { IsSuccess = false, Message = "Indique un usuario." });

            var (token, expira) = generator.Generate(request.User);

            return Ok(new { IsSuccess = true, Token = token, ExpiresAt = expira });
        }
    }
}
