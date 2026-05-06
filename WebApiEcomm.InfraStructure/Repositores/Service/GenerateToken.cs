using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using WebApiEcomm.Core.Entites.Identity;
using WebApiEcomm.Core.Services;
using WebApiEcomm.InfraStructure.Configuration;

namespace WebApiEcomm.InfraStructure.Repositores.Service
{
    public class GenrateToken : IGenrateToken
    {
        private readonly JwtMergedSettings _jwt;
        private readonly UserManager<AppUser> _userManager;

        public GenrateToken(JwtMergedSettings jwt, UserManager<AppUser> userManager)
        {
            _jwt = jwt;
            _userManager = userManager;
        }

        public async Task<string> CreateTokenAsync(AppUser appUser)
        {
            var roles = await _userManager.GetRolesAsync(appUser).ConfigureAwait(false);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, appUser.Id),
                new Claim(ClaimTypes.Name, appUser.UserName ?? string.Empty),
                new Claim(ClaimTypes.Email, appUser.Email ?? string.Empty)
            };

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            if (string.IsNullOrEmpty(_jwt.Secret))
                throw new InvalidOperationException("JWT Secret not configured.");

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Secret));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = _jwt.ResolveExpiresUtc(),
                Issuer = string.IsNullOrWhiteSpace(_jwt.Issuer) ? null : _jwt.Issuer,
                Audience = string.IsNullOrWhiteSpace(_jwt.Audience) ? null : _jwt.Audience,
                SigningCredentials = credentials,
                NotBefore = DateTime.UtcNow
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
