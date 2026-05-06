using Microsoft.Extensions.Configuration;

namespace WebApiEcomm.InfraStructure.Configuration;

/// <summary>
/// Resolves JWT signing secret with an explicit precedence so we can log the source.
/// Also checks common environment variable spellings beyond what may be bound into <see cref="IConfiguration"/>.
/// </summary>
public static class JwtSecretResolver
{
    /// <summary>
    /// Returns the first non-empty secret and a human-readable source label.
    /// </summary>
    public static (string? Secret, string SourceDescription) Resolve(IConfiguration configuration)
    {
        // 1) Explicit environment variables (production / containers / IIS)
        foreach (var (envKey, label) in EnvCandidates)
        {
            var v = Environment.GetEnvironmentVariable(envKey);
            if (!string.IsNullOrWhiteSpace(v))
                return (v.Trim(), label);
        }

        // 2) Standard configuration (appsettings.*, user secrets, env vars bound as Jwt__Secret → Jwt:Secret, etc.)
        var jwtFromConfig = FirstNonEmpty(configuration["Jwt:Secret"], configuration["Token:Secret"]);
        if (!string.IsNullOrWhiteSpace(jwtFromConfig))
        {
            var detail = ClassifyConfigurationSecret(configuration, jwtFromConfig.Trim());
            return (jwtFromConfig.Trim(), detail);
        }

        return (null, "(not set)");
    }

    private static readonly (string EnvKey, string Label)[] EnvCandidates =
    [
        ("JWT__SECRET", "environment variable JWT__SECRET"),
        ("Jwt__Secret", "environment variable Jwt__Secret"),
        ("TOKEN__SECRET", "environment variable TOKEN__SECRET (legacy)"),
        ("Token__Secret", "environment variable Token__Secret (legacy)")
    ];

    private static string ClassifyConfigurationSecret(IConfiguration configuration, string value)
    {
        // If the same value is present on a well-known env key, attribute to environment (typical on shared hosting).
        foreach (var (envKey, label) in EnvCandidates)
        {
            var envVal = Environment.GetEnvironmentVariable(envKey);
            if (!string.IsNullOrWhiteSpace(envVal) && string.Equals(envVal.Trim(), value, StringComparison.Ordinal))
                return $"{label} (same value as configuration Jwt:Secret / Token:Secret)";
        }

        // Bound env vars without going through our explicit list (e.g. custom host prefix) still land in configuration.
        if (configuration["Jwt:Secret"] is { Length: > 0 } j && string.Equals(j.Trim(), value, StringComparison.Ordinal))
            return "configuration key Jwt:Secret (appsettings, user secrets, environment variables, command line, etc.)";

        if (configuration["Token:Secret"] is { Length: > 0 } t && string.Equals(t.Trim(), value, StringComparison.Ordinal))
            return "configuration key Token:Secret (legacy; appsettings, user secrets, environment variables, etc.)";

        return "configuration (Jwt:Secret or Token:Secret)";
    }

    private static string? FirstNonEmpty(params string?[] values)
        => values.FirstOrDefault(v => !string.IsNullOrWhiteSpace(v));
}
