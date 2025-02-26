using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using VideoDiningApp.Models;

namespace VideoDiningApp.Services
{
    public class JwtService : IJwtService
    {
        private const string SecretKey = "12345678910111213141516171819202";
        private readonly SymmetricSecurityKey _key;
        private readonly string _issuer = "your-app"; 
        private readonly string _audience = "your-app"; 
        private readonly int _expirationInMinutes = 1440; 

        public JwtService()
        {
            _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SecretKey));
        }

        public string GenerateToken(User user)
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Email), 
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("UserId", user.Id.ToString()), 
            };

            var credentials = new SigningCredentials(_key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                issuer: _issuer, 
                audience: _audience, 
                claims: claims,
                expires: DateTime.Now.AddMinutes(_expirationInMinutes), 
                signingCredentials: credentials 
            );

            var tokenHandler = new JwtSecurityTokenHandler();
            return tokenHandler.WriteToken(token);
        }
    }
}
