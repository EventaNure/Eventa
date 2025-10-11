using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Eventa.Application.Services;
using Eventa.Infrastructure.Options;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Eventa.Infrastructure.Services
{
    public class JwtTokenService : IJwtTokenService
    {
        private readonly JwtTokenOptions _jwtTokenOptions;
        public JwtTokenService(IOptions<JwtTokenOptions> jwtBearerOptions)
        {
            _jwtTokenOptions = jwtBearerOptions.Value;
        }

        public string GenerateToken(string userId, string role)
        {
            SecurityTokenDescriptor descriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.NameIdentifier, userId),
                    new Claim(ClaimTypes.Role, role)
                }),
                Issuer = _jwtTokenOptions.Issuer,
                Audience = _jwtTokenOptions.Audience,
                Expires = DateTime.UtcNow.AddDays(_jwtTokenOptions.ExpiresDay),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtTokenOptions.Key)), 
                    SecurityAlgorithms.HmacSha256)
            };

            JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
            var token = handler.CreateToken(descriptor);
            return handler.WriteToken(token);
        }
    }
}
