using Moira.Authentik.Domain.Groups;
using Moira.Common.Commands;
using Moira.Common.Models;

namespace Moira.Authentik.Application.Builders;

public interface IAuthentikGroupBuilder
{
    Task<AuthentikGroupV3> BuildAsync(IdPCommand<IdPGroup> command, CancellationToken cancellationToken);
}