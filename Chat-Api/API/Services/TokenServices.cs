using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace API.Services
{
    public class TokenServices(IConfiguration configuration)
    {
        public string GenerateToken(string userId, string userName)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var secret = configuration["Token:Key"];

            if (string.IsNullOrEmpty(secret))
            {
                throw new InvalidOperationException("Token key is not configured.");
            }

            var key = Encoding.UTF8.GetBytes(secret);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Name, userName)
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

    }
}
