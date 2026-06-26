using Moira.Common.Models;

namespace Moira.KubeOps.Entities;

public class OidcSpec
{
    public OidcApplicationType ApplicationType { get; set; } = OidcApplicationType.Native;
    public IEnumerable<string> RedirectUris { get; set; } = [];
    public string LogoutUri { get; set; } = string.Empty;
    public string LaunchUrl { get; set; } = string.Empty;
    public IEnumerable<string> Scopes { get; set; } = ["openid"];
    public IEnumerable<string> GrantTypes { get; set; } = ["authorization_code"];
    public IEnumerable<string> ResponseTypes { get; set; } = ["code"];
    public OidcClientAuthenticationMethod ClientAuthenticationMethod { get; set; } = OidcClientAuthenticationMethod.ClientSecretBasic;
    public string ClientUri { get; set; } = string.Empty;
    public string LogoUri { get; set; } = string.Empty;
    public string PolicyUri { get; set; } = string.Empty;
    public string TermsOfServiceUri { get; set; } = string.Empty;
    public IEnumerable<string> Contacts { get; set; } = [];
}
