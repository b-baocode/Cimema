using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using MV.ApplicationLayer.RepoInterfaces;
using MV.DomainLayer.Entities;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace MV.InfrastructureLayer.Repositories
{
    public class AuthenticationRepository : IAuthenticationRepository
    {
        private readonly IConfiguration _configuration;
        private readonly string _jwtKey;
        private readonly string _jwtIssuer;

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

        public async Task<string> GenerateJwtToken(User user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            // Create claims for the token (e.g., User ID, Username, Role)
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Username), // Subject (username)
                //new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()), // JWT ID
                //new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()), // User ID
                new Claim(JwtRegisteredClaimNames.Email, user.Email), // User's name
                new Claim("Full Name", user.FullName),
                //new Claim("Join Date", user.JoinDate),
                new Claim("Role", user.RoleId) // User's role
            };

            var token = new JwtSecurityToken(
                issuer: _jwtIssuer,
                //audience: null, // If you enable ValidAudience, set this
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1), // Token valid for 1 hour
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        
    }
}
