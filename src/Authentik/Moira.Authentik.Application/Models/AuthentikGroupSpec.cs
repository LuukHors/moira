using Moira.Authentik.Domain.ProviderSettings;

namespace Moira.Authentik.Application.Models;

public record AuthentikGroupSpec(
    string DisplayName,
    IEnumerable<string> MemberOf,
    bool AutoDelete,
    AuthentikGroupProviderSettings Authentik);