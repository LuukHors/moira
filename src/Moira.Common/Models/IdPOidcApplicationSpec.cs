namespace Moira.Common.Models;

public record IdPOidcApplicationSpec(
    string DisplayName,
    string ApplicationType,
    IEnumerable<string> RedirectUris,
    IEnumerable<string> PostLogoutRedirectUris,
    string LaunchUrl,
    IEnumerable<string> Scopes,
    IEnumerable<string> GrantTypes,
    IEnumerable<string> ResponseTypes,
    string ClientAuthenticationMethod,
    string ClientUri,
    string LogoUri,
    string PolicyUri,
    string TermsOfServiceUri,
    IEnumerable<string> Contacts,
    OidcProviderSettings? ProviderSettings,
    bool AutoDelete,
    int RotationDays,
    bool RotateClientSecret)
{
    public bool UsesClientSecret => ClientAuthenticationMethod != OidcClientAuthenticationMethod.None;
}
