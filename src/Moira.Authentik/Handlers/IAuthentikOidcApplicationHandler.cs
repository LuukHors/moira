using Moira.Authentik.Models.V3;
using Moira.Common.Commands;
using Moira.Common.Models;

namespace Moira.Authentik.Handlers;

public interface IAuthentikOidcApplicationHandler : IAuthentikHandler<IdPOidcApplication, AuthentikOidcApplicationV3>
{
    Task<AuthentikOAuth2ProviderV3> CreateProviderAsync(
        IdPCommand<IdPOidcApplication> command,
        CancellationToken cancellationToken);
}
