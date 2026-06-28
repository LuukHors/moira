using Moira.Common.Commands;
using Moira.Common.Models;

namespace Moira.Authentik.Application.Handlers;

public interface IAuthentikProviderHandler
{
    Task CheckAsync(IdPCommand<IdPProvider> command, CancellationToken cancellationToken);
    Task<bool> DeleteAsync(IdPCommand<IdPProvider> command, CancellationToken cancellationToken);
}