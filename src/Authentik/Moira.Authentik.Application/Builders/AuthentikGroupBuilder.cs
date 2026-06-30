using Microsoft.Extensions.Logging;
using Moira.Authentik.Application.Mappers;
using Moira.Authentik.Application.Ports;
using Moira.Authentik.Domain.Groups;
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

    public async Task<AuthentikGroupV3> BuildAsync(IdPCommand<IdPGroup> command, CancellationToken cancellationToken)
    {
        var parentIds = await ResolveParentIdsAsync(command, cancellationToken);
        var attributes = MergeAttributes(command.Entity.Spec.ProviderSettings);
        return command.Entity.ToAuthentikGroup(parentIds, attributes);
    }

    private static IReadOnlyDictionary<string, object> MergeAttributes(IdpProviderSpecificSettings? providerSettings)
    {
        if (providerSettings is null)
            return DefaultAttributes;

        return DefaultAttributes;
        // var settings = providerSettings.ToAuthentikOidcApplicationSettings();
        // return DefaultAttributes
        //     .Concat(settings.Attributes.Values.Select(kv => new KeyValuePair<string, object>(kv.Key, kv.Value)))
        //     .ToDictionary(kv => kv.Key, kv => kv.Value);
    }

    private async Task<IEnumerable<string>> ResolveParentIdsAsync(IdPCommand<IdPGroup> command, CancellationToken cancellationToken)
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