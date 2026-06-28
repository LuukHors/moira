using Moira.Authentik.Application.Ports;
using Moira.Authentik.Domain.Applications;
using Moira.Common.Exceptions;
using Moira.Common.Models;

namespace Moira.Authentik.Application.Builders;

public class AuthentikScopeMappingBuilder(
    IAuthentikRepository<AuthentikScopeMappingV3, AuthentikScopeMappingV3, string> scopeMappingRepository) : IAuthentikScopeMappingBuilder
{
    public async Task<IEnumerable<string>> BuildAsync(IEnumerable<string> scopeNames, IdPProvider provider, CancellationToken cancellationToken)
    {
        var distinctScopeNames = scopeNames
            .Where(scope => !string.IsNullOrWhiteSpace(scope))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        var scopeMappingTasks = distinctScopeNames.Select(scope => ResolveScopeMappingIdAsync(scope, provider, cancellationToken));

        var scopeMappingIds = await Task.WhenAll(scopeMappingTasks);
        return scopeMappingIds.Order(StringComparer.OrdinalIgnoreCase);
    }

    private async Task<string> ResolveScopeMappingIdAsync(string scope, IdPProvider provider, CancellationToken cancellationToken)
    {
        var page = await scopeMappingRepository.ListByQueryAsync(
            new Dictionary<string, string> { ["scope_name"] = scope },
            provider,
            cancellationToken: cancellationToken);
        var matches = page.Results
            .Where(mapping => mapping.scope_name.Equals(scope, StringComparison.OrdinalIgnoreCase))
            .ToArray();

        if (matches.Length == 0)
        {
            throw new IdPException(
                $"Authentik scope mapping for scope \"{scope}\" was not found.",
                IdPExceptionReason.IdpValidationFailed);
        }

        if (matches.Length > 1)
        {
            throw new IdPException(
                $"Found multiple Authentik scope mappings for scope \"{scope}\". Use unique scope names before reconciling this OIDC application.",
                IdPExceptionReason.IdpValidationFailed);
        }

        return matches[0].pk;
    }
}