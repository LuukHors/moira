using Moira.Authentik.Domain.ProviderSettings;
using Moira.Common.Abstractions.Models;

namespace Moira.Authentik.Application.Models;

public record AuthentikOidcApplicationSpec(
    string DisplayName,
    OidcApplicationType ApplicationType,
    IEnumerable<string> RedirectUris,
    string LogoutUri,
    string LaunchUrl,
    IEnumerable<string> Scopes,
    IEnumerable<string> GrantTypes,
    IEnumerable<string> ResponseTypes,
    OidcClientAuthenticationMethod ClientAuthenticationMethod,
    string ClientUri,
    string LogoUri,
    string PolicyUri,
    string TermsOfServiceUri,
    IEnumerable<string> Contacts,
    bool AutoDelete,
    int RotationDays,
    bool RotateClientSecret,
    OidcAuthentikProviderSettings Authentik)
{
    public bool UsesClientSecret => ClientAuthenticationMethod != OidcClientAuthenticationMethod.None;
}