namespace Moira.Common.Models;

public record IdPOidcApplicationSpec(
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
    OidcProviderSettings ProviderSettings,
    bool AutoDelete,
    int RotationDays,
    bool RotateClientSecret)
{
    public bool UsesClientSecret => ClientAuthenticationMethod != OidcClientAuthenticationMethod.None;
}
