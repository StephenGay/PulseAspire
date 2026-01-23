using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Configuration;
using Pulse.Models.Users;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Pulse.ApiService.Security
{
    public class TokenService
    {
        //private readonly IConfiguration _configuration;
        
        private readonly SymmetricSecurityKey _key;
        private readonly string _issuer;
        private readonly string _audience;

        public TokenService(IConfiguration configuration)
        {
            _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!));
            _issuer = configuration["Jwt:Issuer"] ?? "Pulse.ApiService";
            _audience = configuration["Jwt:Audience"] ?? "Pulse.Clients";
            //_configuration = configuration;
            //_userManager = userManager;
        }

        //public string GenerateJwtToken(IdentityUser user, byte[] jwtKey, IConfiguration config)
        //{
        //    var claims = new[] { new Claim(ClaimTypes.Name, user.UserName) };
        //    var key = new SymmetricSecurityKey(jwtKey);
        //    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        //    var expires = DateTime.Now.AddDays(7);

        //    var token = new JwtSecurityToken(
        //        issuer: config["Jwt:Issuer"],
        //        audience: config["Jwt:Audience"],
        //        claims: claims,
        //        expires: expires,
        //        signingCredentials: creds
        //    );

        //    return new JwtSecurityTokenHandler().WriteToken(token);
        //}

        public async Task<string> GenerateJwtToken(ApplicationUser user, IList<string> roles)
        {
            var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id), // Standard "sub" claim
            new Claim(JwtRegisteredClaimNames.NameId, user.Id), // Explicit NameIdentifier
            new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName ?? string.Empty),
            new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()), // Unique token ID
            new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
        };
            //    var claims = new List<Claim>
            //{
            //    new Claim(JwtRegisteredClaimNames.Sub, user.Email),
            //    new Claim(ClaimTypes.Name, user.UserName),
            //    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            //    new Claim(ClaimTypes.NameIdentifier, user.Id)
            //};

            // Add roles as claims
            
            //List<IdentityRole> roles = UserManager.GetRolesAsync(user);
            //foreach (var role in roles)
            //{
            //    claims.Add(new Claim(ClaimTypes.Role, role));
            //}

            // Optionally add custom claims from ApplicationUser properties
            // claims.Add(new Claim("FullName", user.FullName ?? string.Empty));

            // Add roles as separate claims (SignalR/IHubContext can use them)
            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));
            var credentials = new SigningCredentials(_key, SecurityAlgorithms.HmacSha256);

            //var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            //var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            //var expires = DateTime.Now.AddDays(7);  // Adjust expiration as needed

            var token = new JwtSecurityToken(
                issuer: _issuer,
                audience: _audience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(8), // Adjust expiry as needed
                signingCredentials: credentials
            );
            //var token = new JwtSecurityToken(
            //    issuer: _configuration["Jwt:Issuer"],
            //    audience: _configuration["Jwt:Audience"],
            //    claims: claims,
            //    expires: expires,
            //    signingCredentials: creds
            //);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
