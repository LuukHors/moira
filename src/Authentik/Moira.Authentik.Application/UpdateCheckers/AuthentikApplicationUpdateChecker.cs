using Moira.Authentik.Domain.Applications;
using Moira.Common.Abstractions;

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
                   StringComparison.Ordinal)
               || !string.Equals(NormalizeOptionalText(desired.meta_description), NormalizeOptionalText(current.meta_description), StringComparison.Ordinal)
               || !string.Equals(NormalizeOptionalText(desired.meta_icon_url), NormalizeOptionalText(current.meta_icon_url), StringComparison.Ordinal)
               || !string.Equals(NormalizeOptionalText(desired.meta_publisher), NormalizeOptionalText(current.meta_publisher), StringComparison.Ordinal)
               || desired.open_in_new_tab != current.open_in_new_tab
               || !string.Equals(NormalizeOptionalText(desired.group), NormalizeOptionalText(current.group), StringComparison.Ordinal);
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
