using Moira.Authentik.Domain.Applications;
using Moira.Common.Commands;
using Moira.Common.Models;

namespace Moira.Authentik.Application.Handlers;

public interface IAuthentikOidcApplicationHandler : IAuthentikHandler<IdPOidcApplication, AuthentikOidcApplicationV3>
{
    Task<AuthentikOAuth2ProviderV3> CreateProviderAsync(
        IdPCommand<IdPOidcApplication> command,
        CancellationToken cancellationToken);
}
