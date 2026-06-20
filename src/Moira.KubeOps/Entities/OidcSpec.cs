namespace Moira.KubeOps.Entities;

public class OidcSpec
{
    public IEnumerable<string> RedirectUris { get; set; } = [];
    public IEnumerable<string> PostLogoutRedirectUris { get; set; } = [];
    public string LaunchUrl { get; set; } = string.Empty;
    public IEnumerable<string> Scopes { get; set; } = [];
    public IEnumerable<string> GrantTypes { get; set; } = [];
    public IEnumerable<string> ResponseTypes { get; set; } = [];
}
