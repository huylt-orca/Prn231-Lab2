using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace GrpcServiceDemo.Utils
{
    public class JwtService
    {
        private readonly IConfiguration _configuration;
        public JwtService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GenerateJwtToken(string email, string role,string id)
        {
            var authClaim = new List<Claim>
            {
                new Claim(ClaimTypes.Email, email),
                new Claim(ClaimTypes.Role, role),
                new Claim("Id",id),
            };

            var authenKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]));
            var token = new JwtSecurityToken(
                issuer: _configuration["JWT:ValidIssuer"],
                audience: _configuration["JWT:ValidAudience"],
                expires: DateTime.Now.AddMinutes(20),
                claims: authClaim,
                signingCredentials: new SigningCredentials(authenKey, SecurityAlgorithms.HmacSha512Signature)
                );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        public ClaimsPrincipal DecodeJwtToken(string jwtToken)
        {
            var secretKey = _configuration["JwtConfig:Secret"];
            var secretKeyBytes = Encoding.UTF8.GetBytes(secretKey);
            var tokenHandler = new JwtSecurityTokenHandler();
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secretKey)),
                ValidateIssuer = false,
                ValidateAudience = false
            };

            try
            {
                var claimsPrincipal = tokenHandler.ValidateToken(jwtToken, validationParameters, out var validatedToken);
                return claimsPrincipal;
            }
            catch (Exception e)
            {
                // Handle token validation errors here
                return null;
            }
        }

    }
}
