using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using WebApiEcomm.Core.Entites.Auth;
using WebApiEcomm.Core.Entites.Dtos;
using WebApiEcomm.Core.Entites.Identity;
using WebApiEcomm.Core.Services.Auth;
using WebApiEcomm.InfraStructure.Options;

namespace WebApiEcomm.InfraStructure.Services.Auth
{
    public class TokenService : ITokenService
    {
        private readonly JwtOptions _jwtOptions;
        private readonly RefreshTokenOptions _refreshOptions;
        private readonly IRefreshTokenStore _refreshTokenStore;
        private readonly UserManager<AppUser> _userManager;

        public TokenService(
            IOptions<JwtOptions> jwtOptions,
            IOptions<RefreshTokenOptions> refreshOptions,
            IRefreshTokenStore refreshTokenStore,
            UserManager<AppUser> userManager)
        {
            _jwtOptions = jwtOptions.Value;
            _refreshOptions = refreshOptions.Value;
            _refreshTokenStore = refreshTokenStore;
            _userManager = userManager;
        }

        public async Task<TokenPairResponse> CreateTokenPairAsync(AppUser user, string? ipAddress, string? userAgent, CancellationToken cancellationToken = default)
        {
            var roles = await _userManager.GetRolesAsync(user);
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id),
                new(ClaimTypes.Name, user.UserName ?? string.Empty),
                new(ClaimTypes.Email, user.Email ?? string.Empty),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N"))
            };

            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));
            var now = DateTime.UtcNow;
            var accessExpires = now.AddMinutes(_jwtOptions.AccessTokenExpiryMinutes);
            var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.Secret));

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Issuer = _jwtOptions.Issuer,
                Audience = _jwtOptions.Audience,
                Expires = accessExpires,
                NotBefore = now,
                SigningCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256)
            };

            var handler = new JwtSecurityTokenHandler();
            var securityToken = handler.CreateToken(tokenDescriptor);
            var accessToken = handler.WriteToken(securityToken);
            var jti = claims.First(x => x.Type == JwtRegisteredClaimNames.Jti).Value;

            var rawRefresh = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
            var hashedRefresh = HashRefreshToken(rawRefresh);
            await _refreshTokenStore.AddAsync(new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                TokenHash = hashedRefresh,
                JwtId = jti,
                CreatedAtUtc = now,
                ExpiresAtUtc = now.AddDays(_refreshOptions.ExpiryDays),
                CreatedByIp = ipAddress,
                UserAgent = userAgent,
                IsRevoked = false
            }, cancellationToken);
            await _refreshTokenStore.SaveChangesAsync(cancellationToken);

            return new TokenPairResponse
            {
                AccessToken = accessToken,
                RefreshToken = rawRefresh,
                ExpiresInSeconds = (long)TimeSpan.FromMinutes(_jwtOptions.AccessTokenExpiryMinutes).TotalSeconds,
                Roles = roles,
                Email = user.Email ?? string.Empty,
                DisplayName = user.DisplayName ?? user.UserName ?? string.Empty
            };
        }

        public async Task<TokenPairResponse> RotateRefreshTokenAsync(string refreshToken, string? ipAddress, string? userAgent, CancellationToken cancellationToken = default)
        {
            var tokenHash = HashRefreshToken(refreshToken);
            var existing = await _refreshTokenStore.GetByHashAsync(tokenHash, cancellationToken);
            if (existing is null || existing.IsRevoked || existing.ExpiresAtUtc <= DateTime.UtcNow)
            {
                throw new AuthException("Invalid refresh token", 401);
            }

            var user = await _userManager.FindByIdAsync(existing.UserId);
            if (user is null)
            {
                throw new AuthException("User not found", 404);
            }

            await _refreshTokenStore.RevokeAsync(existing, "rotated", cancellationToken);
            await _refreshTokenStore.SaveChangesAsync(cancellationToken);
            return await CreateTokenPairAsync(user, ipAddress, userAgent, cancellationToken);
        }

        public async Task RevokeRefreshTokenAsync(string userId, string refreshToken, string reason, CancellationToken cancellationToken = default)
        {
            var tokenHash = HashRefreshToken(refreshToken);
            var existing = await _refreshTokenStore.GetByHashAsync(tokenHash, cancellationToken);
            if (existing is null || existing.UserId != userId)
            {
                return;
            }
            await _refreshTokenStore.RevokeAsync(existing, reason, cancellationToken);
            await _refreshTokenStore.SaveChangesAsync(cancellationToken);
        }

        public string HashRefreshToken(string token)
        {
            var pepper = _refreshOptions.SecretPepper;
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes($"{pepper}:{token}"));
            return Convert.ToHexString(bytes);
        }
    }
}
