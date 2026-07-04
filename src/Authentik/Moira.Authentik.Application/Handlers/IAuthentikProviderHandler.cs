using Moira.Common.Abstractions.Commands;
using Moira.Common.Abstractions.Models;

namespace Moira.Authentik.Application.Handlers;

public interface IAuthentikProviderHandler
{
    Task CheckAsync(IdPCommand<IdPProvider> command, CancellationToken cancellationToken);
    Task<bool> DeleteAsync(IdPCommand<IdPProvider> command, CancellationToken cancellationToken);
}