using System.Text.RegularExpressions;
using Moira.Authentik.Application.Mappers;
using Moira.Authentik.Domain.Applications;
using Moira.Common.Models;

namespace Moira.Authentik.Application.Builders;

public partial class AuthentikApplicationBuilder : IAuthentikApplicationBuilder
{
    public AuthentikApplicationV3 Build(OidcProviderSettings providerSettings, IdPOidcApplication application, int? providerId, string? applicationPk)
    {
        var settings = providerSettings.ToAuthentikSettings();

        return new AuthentikApplicationV3(
            application.Spec.DisplayName,
            Slug(application.Name),
            applicationPk,
            providerId,
            NormalizeLaunchUrl(application.Spec.LaunchUrl),
            meta_icon_url: settings.Metadata.Icon ?? string.Empty,
            meta_description: settings.Metadata.Description ?? string.Empty,
            meta_publisher: settings.Metadata.Publisher ?? string.Empty,
            open_in_new_tab: settings.Metadata.OpenInNewTab,
            group: NormalizeOptionalText(settings.Group));
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

    private static string Slug(string value)
    {
        var slug = SlugRegex().Replace(value.ToLowerInvariant(), "-").Trim('-');
        return string.IsNullOrWhiteSpace(slug) ? "oidc-application" : slug;
    }

    [GeneratedRegex("[^a-z0-9]+")]
    private static partial Regex SlugRegex();
}