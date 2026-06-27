using Moira.Authentik.Domain.Applications;
using Moira.Common.Provider;

namespace Moira.Authentik.Application.UpdateCheckers;

public class AuthentikApplicationUpdateChecker : IUpdateChecker<AuthentikApplicationV3, AuthentikApplicationV3>
{
    public bool ShouldUpdate(AuthentikApplicationV3 desired, AuthentikApplicationV3 current)
    {
        return !string.Equals(desired.name, current.name, StringComparison.Ordinal)
               || !string.Equals(desired.slug, current.slug, StringComparison.Ordinal)
               || !desired.provider.Equals(current.provider)
               || !string.Equals(
                   NormalizeLaunchUrl(desired.meta_launch_url),
                   NormalizeLaunchUrl(current.meta_launch_url),
                   StringComparison.Ordinal);
    }

    private static string NormalizeOptionalText(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? string.Empty : value;
    }

    private static string NormalizeLaunchUrl(string? value)
    {
        var normalized = NormalizeOptionalText(value);
        return normalized.Length == 0 ? normalized : normalized.TrimEnd('/');
    }
}
