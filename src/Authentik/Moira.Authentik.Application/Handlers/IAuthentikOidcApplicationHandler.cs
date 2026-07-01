using Moira.Authentik.Application.Models;
using Moira.Authentik.Domain.Applications;
using Moira.Common.Abstractions.Commands;
using Moira.Common.Abstractions.Models;

namespace Moira.Authentik.Application.Handlers;

public interface IAuthentikOidcApplicationHandler : IAuthentikHandler<AuthentikOidcApplicationModel, AuthentikOidcApplicationV3>
{
    Task<AuthentikOAuth2ProviderV3> CreateProviderAsync(
        IdPCommand<AuthentikOidcApplicationModel> command,
        CancellationToken cancellationToken);
}
