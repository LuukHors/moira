namespace Moira.Authentik.Domain.Applications;

public record AuthentikOAuth2ProviderV3
{
    public string name { get; set; } = string.Empty;
    public int? pk { get; set; }
    public string client_type { get; set; } = "confidential";
    public string client_id { get; set; } = string.Empty;
    public string client_secret { get; set; } = string.Empty;
    public string authorization_flow { get; set; } = string.Empty;
    public string invalidation_flow { get; set; } = string.Empty;
    public IReadOnlyDictionary<string, object> attributes { get; set; } = new Dictionary<string, object>();
    public IEnumerable<object> property_mappings { get; set; } = [];
    public IEnumerable<AuthentikRedirectUriV3> redirect_uris { get; set; } = [];
    public string logout_uri { get; set; } = string.Empty;
    public string? access_code_validity { get; set; }
    public string? access_token_validity { get; set; }
    public string? refresh_token_validity { get; set; }
}
