using System.Text.RegularExpressions;
using Moira.Authentik.Domain.Applications;
using Moira.Common.Models;

namespace Moira.Authentik.Application.Builders;

public partial class AuthentikApplicationBuilder : IAuthentikApplicationBuilder
{
    public AuthentikApplicationV3 Build(IdPOidcApplication application, int? providerId, string? applicationPk)
    {
        return new AuthentikApplicationV3(
            application.Spec.DisplayName,
            Slug(application.Name),
            applicationPk,
            providerId,
            NormalizeLaunchUrl(application.Spec.LaunchUrl));
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