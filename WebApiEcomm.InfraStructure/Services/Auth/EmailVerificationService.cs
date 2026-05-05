using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;
using WebApiEcomm.Core.Services.Auth;
using WebApiEcomm.InfraStructure.Data;
using WebApiEcomm.InfraStructure.Entities.Auth;
using WebApiEcomm.InfraStructure.Options;

namespace WebApiEcomm.InfraStructure.Services.Auth
{
    public class EmailVerificationService : IEmailVerificationService
    {
        private readonly AppDbContext _context;
        private readonly RefreshTokenOptions _options;

        public EmailVerificationService(AppDbContext context, IOptions<RefreshTokenOptions> options)
        {
            _context = context;
            _options = options.Value;
        }

        public async Task<string> GenerateCodeAsync(string userId, CancellationToken cancellationToken = default)
        {
            var rawCode = RandomNumberGenerator.GetInt32(100000, 1000000).ToString();
            var hash = Hash(rawCode);
            await _context.EmailVerificationCodes.AddAsync(new EmailVerificationEntity
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                CodeHash = hash,
                CreatedAtUtc = DateTime.UtcNow,
                ExpiresAtUtc = DateTime.UtcNow.AddMinutes(15),
                AttemptCount = 0,
                IsUsed = false
            }, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            return rawCode;
        }

        public async Task<bool> VerifyCodeAsync(string userId, string code, CancellationToken cancellationToken = default)
        {
            var hash = Hash(code);
            var entity = await _context.EmailVerificationCodes
                .Where(x => x.UserId == userId && !x.IsUsed)
                .OrderByDescending(x => x.CreatedAtUtc)
                .FirstOrDefaultAsync(cancellationToken);

            if (entity is null || entity.ExpiresAtUtc < DateTime.UtcNow)
            {
                return false;
            }

            entity.AttemptCount++;
            var valid = entity.CodeHash == hash;
            if (valid)
            {
                entity.IsUsed = true;
            }
            await _context.SaveChangesAsync(cancellationToken);
            return valid;
        }

        private string Hash(string input)
        {
            var pepper = _options.SecretPepper;
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes($"{pepper}:{input}"));
            return Convert.ToHexString(bytes);
        }
    }
}
