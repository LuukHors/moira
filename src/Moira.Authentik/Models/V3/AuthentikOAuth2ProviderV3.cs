namespace Moira.Authentik.Models.V3;

public class AuthentikOAuth2ProviderV3
{
    public string name { get; set; } = string.Empty;
    public string? pk { get; set; }
    public string client_id { get; set; } = string.Empty;
    public string client_secret { get; set; } = string.Empty;
    public IEnumerable<string> redirect_uris { get; set; } = [];
}
