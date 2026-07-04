using Moira.Authentik.Application.Models;
using Microsoft.Extensions.Logging;
using Moira.Authentik.Application.Mappers;
using Moira.Authentik.Application.Ports;
using Moira.Authentik.Domain.Groups;
using Moira.Authentik.Domain.ProviderSettings;
using Moira.Common.Abstractions.Commands;
using Moira.Common.Abstractions.Exceptions;
using Moira.Common.Abstractions.Models;

namespace Moira.Authentik.Application.Builders;

public class AuthentikGroupBuilder(
    IAuthentikRepository<AuthentikGroupV3, AuthentikGroupV3, string> groupRepository,
    ILogger<AuthentikGroupBuilder> logger) : IAuthentikGroupBuilder
{
    private static readonly IReadOnlyDictionary<string, object> DefaultAttributes =
        new Dictionary<string, object> { ["managed-by"] = "moira" };

    public async Task<AuthentikGroupV3> BuildAsync(IdPCommand<AuthentikGroupModel> command, CancellationToken cancellationToken)
    {
        var parentIds = await ResolveParentIdsAsync(command, cancellationToken);
        var attributes = MergeAttributes(command.Entity.Spec.Authentik);
        return command.Entity.ToAuthentikGroup(parentIds, attributes);
    }

    private static IReadOnlyDictionary<string, object> MergeAttributes(AuthentikGroupProviderSettings settings)
    {
        return DefaultAttributes
            .Concat(settings.Attributes.Values.Select(kv => new KeyValuePair<string, object>(kv.Key, kv.Value)))
            .ToDictionary(kv => kv.Key, kv => kv.Value);
    }

    private async Task<IEnumerable<string>> ResolveParentIdsAsync(IdPCommand<AuthentikGroupModel> command, CancellationToken cancellationToken)
    {
        var memberOfNames = command.Entity.Spec.MemberOf
            .Where(memberOf => !string.IsNullOrEmpty(memberOf))
            .Distinct()
            .ToList();

        logger.LogDebug("Resolving {ParentGroupCount} parent groups", memberOfNames.Count);

        var parentTasks = memberOfNames.Select(async memberOf =>
        {
            var parent = await groupRepository.GetByNameAsync(memberOf, command.Entity.IdPProvider, null, cancellationToken)
                ?? throw new IdPException($"Could not find parent group '{memberOf}'", IdPExceptionReason.IdpValidationFailed);

            return parent.pk!;
        });

        return await Task.WhenAll(parentTasks);
    }
}