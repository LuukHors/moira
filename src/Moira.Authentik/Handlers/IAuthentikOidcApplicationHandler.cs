using Moira.Authentik.Models.V3;
using Moira.Common.Commands;
using Moira.Common.Models;

namespace Moira.Authentik.Handlers;

public interface IAuthentikOidcApplicationHandler : IAuthentikHandler<IdPOidcApplication, AuthentikOidcApplicationV3>
{
    Task<AuthentikOAuth2ProviderV3?> GetProviderAsync(
        IdPCommand<IdPOidcApplication> command,
        CancellationToken cancellationToken);

    Task<AuthentikApplicationV3> CreateApplicationAsync(
        IdPCommand<IdPOidcApplication> command,
        AuthentikOAuth2ProviderV3 provider,
        CancellationToken cancellationToken);
}
