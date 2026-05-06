using Microsoft.Extensions.Configuration;

namespace WebApiEcomm.InfraStructure.Configuration
{
    /// <summary>
    /// Merged JWT settings: prefers <c>Jwt:*</c>, falls back to legacy <c>Token:*</c>.
    /// Expiry: prefers <c>Jwt:AccessTokenExpiryMinutes</c>, else <c>Token:ExpiryDays</c>, else 7 days.
    /// </summary>
    public sealed class JwtMergedSettings
    {
        public string Secret { get; }
        public string? Issuer { get; }
        public string? Audience { get; }

        private readonly bool _expiryUsesMinutes;
        private readonly int _minutes;
        private readonly int _days;

        private JwtMergedSettings(
            string secret,
            string? issuer,
            string? audience,
            bool expiryUsesMinutes,
            int minutes,
            int days)
        {
            Secret = secret;
            Issuer = issuer;
            Audience = audience;
            _expiryUsesMinutes = expiryUsesMinutes;
            _minutes = minutes;
            _days = days;
        }

        public DateTime ResolveExpiresUtc()
        {
            if (_expiryUsesMinutes && _minutes > 0)
                return DateTime.UtcNow.AddMinutes(_minutes);

            var days = _days > 0 ? _days : 7;
            return DateTime.UtcNow.AddDays(days);
        }

        /// <summary>Merges issuer/audience/expiry from configuration; resolves secret via <see cref="JwtSecretResolver"/>.</summary>
        public static JwtMergedSettings FromConfiguration(IConfiguration configuration, string resolvedSecret)
        {
            var jwt = configuration.GetSection("Jwt");
            var legacy = configuration.GetSection("Token");

            var secret = string.IsNullOrWhiteSpace(resolvedSecret)
                ? string.Empty
                : resolvedSecret.Trim();
            var issuer = FirstNonEmpty(jwt["Issuer"], legacy["Issuer"]);
            var audience = FirstNonEmpty(jwt["Audience"], legacy["Audience"]);

            var jwtMinutesOk = TryParsePositiveInt(jwt["AccessTokenExpiryMinutes"], out var jwtMinutes);
            var legacyDaysOk = TryParsePositiveInt(legacy["ExpiryDays"], out var legacyDays);

            return new JwtMergedSettings(
                secret,
                issuer,
                audience,
                jwtMinutesOk,
                jwtMinutesOk ? jwtMinutes : 0,
                legacyDaysOk ? legacyDays : 0);

            static bool TryParsePositiveInt(string? raw, out int value)
            {
                value = 0;
                if (string.IsNullOrWhiteSpace(raw)) return false;
                if (!int.TryParse(raw, out var parsed) || parsed <= 0) return false;
                value = parsed;
                return true;
            }

        }

        /// <summary>
        /// Backward-compatible overload: resolves secret from configuration and environment (see <see cref="JwtSecretResolver"/>).
        /// Prefer <see cref="FromConfiguration(IConfiguration,string)"/> when using dev/prod policy in the host.
        /// </summary>
        public static JwtMergedSettings FromConfiguration(IConfiguration configuration)
        {
            var (secret, _) = JwtSecretResolver.Resolve(configuration);
            return FromConfiguration(configuration, secret ?? string.Empty);
        }

        /// <summary>Same issuer/audience/expiry, new signing secret (e.g. development ephemeral key).</summary>
        public JwtMergedSettings WithReplacedSecret(string secret)
            => new JwtMergedSettings(
                secret,
                Issuer,
                Audience,
                _expiryUsesMinutes,
                _minutes,
                _days);

        private static string? FirstNonEmpty(params string?[] values)
            => values.FirstOrDefault(v => !string.IsNullOrWhiteSpace(v));
    }
}
