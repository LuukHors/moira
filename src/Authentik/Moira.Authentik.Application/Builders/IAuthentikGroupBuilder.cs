using Moira.Authentik.Application.Models.Group;
using Moira.Authentik.Domain.Groups;
using Moira.Common.Abstractions.Commands;

namespace Moira.Authentik.Application.Builders;

public interface IAuthentikGroupBuilder
{
    Task<AuthentikGroupV3> BuildAsync(IdPCommand<AuthentikGroupModel> command, CancellationToken cancellationToken);
}