using Microsoft.Extensions.Logging;
using Moira.Authentik.Application.Mappers;
using Moira.Authentik.Application.Models.Group;
using Moira.Authentik.Application.Ports;
using Moira.Authentik.Domain.Groups;
using Moira.Authentik.Domain.Roles;
using Moira.Common.Abstractions.Commands;
using Moira.Common.Abstractions.Exceptions;

namespace Moira.Authentik.Application.Builders;

public class AuthentikGroupBuilder(
    IAuthentikRepository<AuthentikGroupV3, AuthentikGroupV3, string> groupRepository,
    IAuthentikRepository<AuthentikRoleV3, AuthentikRoleV3, string> roleRepository,
    ILogger<AuthentikGroupBuilder> logger) : IAuthentikGroupBuilder
{
    private static readonly IReadOnlyDictionary<string, object> DefaultAttributes =
        new Dictionary<string, object> { ["managed-by"] = "moira" };

    public async Task<AuthentikGroupV3> BuildAsync(IdPCommand<AuthentikGroupModel> command, CancellationToken cancellationToken)
    {
        var parentIds = await ResolveParentIdsAsync(command, cancellationToken);
        var roleIds = await ResolveRoleIdsAsync(command, cancellationToken);
        return command.Entity.ToAuthentikGroup(parentIds, roleIds, DefaultAttributes);
    }
    
    private async Task<IEnumerable<string>> ResolveParentIdsAsync(IdPCommand<AuthentikGroupModel> command, CancellationToken cancellationToken)
    {
        var memberOfNames = command.Entity.Spec.MemberOf
            .Where(memberOf => !string.IsNullOrWhiteSpace(memberOf))
            .Distinct()
            .ToList();

        var page = await groupRepository.ListAsync(
            name: null,
            attributes: null,
            command.Entity.IdPProvider,
            id: null,
            cancellationToken);

        var groupsByName = page.Results.ToDictionary(g => g.name);

        var missingGroup = memberOfNames
            .FirstOrDefault(memberOfName => !groupsByName.ContainsKey(memberOfName));

        if (missingGroup is not null)
        {
            throw new IdPException(
                $"Could not find group '{missingGroup}'",
                IdPExceptionReason.IdpValidationFailed);
        }

        return memberOfNames
            .Select(name => groupsByName[name].pk!);
    }
    
    private async Task<IEnumerable<string>> ResolveRoleIdsAsync(IdPCommand<AuthentikGroupModel> command, CancellationToken cancellationToken)
    {
        var roleNames = command.Entity.Spec.Roles
            .Where(role => !string.IsNullOrEmpty(role))
            .Distinct()
            .ToList();

        var page = await roleRepository.ListAsync(name: null,
            attributes: null,
            command.Entity.IdPProvider,
            id: null,
            cancellationToken);

        var rolesByName = page.Results.ToDictionary(g => g.name);

        var missingRole = roleNames
            .FirstOrDefault(roleName => !rolesByName.ContainsKey(roleName));

        if (missingRole is not null)
        {
            throw new IdPException(
                $"Could not find group '{missingRole}'",
                IdPExceptionReason.IdpValidationFailed);
        }

        return roleNames
            .Select(name => rolesByName[name].pk!);
    }
}