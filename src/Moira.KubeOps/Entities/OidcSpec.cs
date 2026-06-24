namespace Moira.KubeOps.Entities;

public class OidcSpec
{
    public string ApplicationType { get; set; } = "web";
    public IEnumerable<string> RedirectUris { get; set; } = [];
    public IEnumerable<string> PostLogoutRedirectUris { get; set; } = [];
    public string LaunchUrl { get; set; } = string.Empty;
    public IEnumerable<string> Scopes { get; set; } = ["openid"];
    public IEnumerable<string> GrantTypes { get; set; } = ["authorization_code"];
    public IEnumerable<string> ResponseTypes { get; set; } = ["code"];
    public string ClientAuthenticationMethod { get; set; } = "client_secret_basic";
    public string ClientUri { get; set; } = string.Empty;
    public string LogoUri { get; set; } = string.Empty;
    public string PolicyUri { get; set; } = string.Empty;
    public string TermsOfServiceUri { get; set; } = string.Empty;
    public IEnumerable<string> Contacts { get; set; } = [];
}
