namespace Moira.Common.Models;

public record IdPOidcApplicationSpec(
    string DisplayName,
    IEnumerable<string> RedirectUris,
    IEnumerable<string> PostLogoutRedirectUris,
    string LaunchUrl,
    IEnumerable<string> Scopes,
    IEnumerable<string> GrantTypes,
    IEnumerable<string> ResponseTypes,
    bool AutoDelete,
    int RotationDays,
    bool RotateClientSecret);
