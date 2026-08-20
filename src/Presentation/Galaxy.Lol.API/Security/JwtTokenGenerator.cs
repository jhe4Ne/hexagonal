using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Galaxy.Lol.API.Security
{

    public class JwtTokenGenerator(IOptions<JwtSettings> options)
    {
        private readonly JwtSettings _settings = options.Value;

        public (string Token, DateTime ExpiresAt) Generate(string user)
        {
            if (string.IsNullOrWhiteSpace(_settings.SecretKey))
                throw new InvalidOperationException(
                    "No hay clave de firma configurada. Defina la variable de entorno JWT_SECRET.");

            var expiracion = DateTime.UtcNow.AddMinutes(_settings.ExpirationMinutes);

            var credenciales = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.SecretKey)),
                SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _settings.Issuer,
                audience: _settings.Audience,
                claims:
                [
                    new Claim(JwtRegisteredClaimNames.Sub, user),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                ],
                expires: expiracion,
                signingCredentials: credenciales);

            return (new JwtSecurityTokenHandler().WriteToken(token), expiracion);
        }
    }
}
