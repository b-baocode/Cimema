using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using MV.ApplicationLayer.DTO.ResponseModel;
using MV.ApplicationLayer.RepositoryInterfaces;

namespace MV.InfrastructureLayer.Repositories
{
    public class AuthenticationRepository : IAuthenticationRepository
    {
        private readonly IConfiguration _configuration;
        private readonly string? _jwtKey;
        private readonly string? _jwtIssuer;

        public AuthenticationRepository(IConfiguration configuration)
        {
            _configuration = configuration;
            _jwtKey = _configuration["Jwt:Key"];
            _jwtIssuer = _configuration["Jwt:Issuer"];

            if (string.IsNullOrEmpty(_jwtKey) || string.IsNullOrEmpty(_jwtIssuer))
            {
                throw new InvalidOperationException("JWT configuration is missing in appsettings.json for AuthService.");
            }
        }

        public string GenerateJwtToken(LoginResponse loginResponse)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.NameId, loginResponse.Userid),
                new Claim(JwtRegisteredClaimNames.Sub, loginResponse.Username),
                new Claim(JwtRegisteredClaimNames.Email, loginResponse.Email),
                new Claim(ClaimTypes.MobilePhone, loginResponse.Phone),
                new Claim(ClaimTypes.Role, loginResponse.Role)
            };

            var token = new JwtSecurityToken(
                issuer: _jwtIssuer,
                //audience: null, 
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }


    }
}
