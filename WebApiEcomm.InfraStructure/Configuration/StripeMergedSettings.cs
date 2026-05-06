using Microsoft.Extensions.Configuration;

namespace WebApiEcomm.InfraStructure.Configuration
{
    /// <summary>Stripe keys merged from supported configuration spellings.</summary>
    public sealed class StripeMergedSettings
    {
        public string SecretKey { get; private init; } = string.Empty;

        public static StripeMergedSettings FromConfiguration(IConfiguration configuration)
        {
            // Common patterns (including typo used previously in PaymentService).
            var key =
                configuration["Stripe:SecretKey"]
                ?? configuration["StripeSettings:SecretKey"]
                ?? configuration["StripeSetting:SecretKey"]
                ?? configuration["StripeSetting : SecretKey"]
                ?? string.Empty;

            return new StripeMergedSettings { SecretKey = key };
        }
    }
}
