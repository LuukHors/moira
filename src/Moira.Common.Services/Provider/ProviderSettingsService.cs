using Moira.Common.Abstractions;
using Moira.Common.Abstractions.Exceptions;
using Moira.Common.Abstractions.Models;

namespace Moira.Common.Services.Provider;

public class ProviderSettingsService(IEnumerable<IProviderSettingsResolver> resolvers) : IProviderSettingsService
{
    public async Task<IdpProviderSpecificSettings?> ResolveAsync(ResourceRef? entity, CancellationToken cancellationToken)
    {
        if (entity == null) return null;

        var resolver = resolvers.FirstOrDefault(resolver => resolver.CanResolve(entity));

        if (resolver is null)
        {
            throw new ProviderSettingsException($"No provider settings resolver registered for provider settings '{entity.Kind}'.");
        }

        return await resolver.ResolveAsync(entity, cancellationToken);
    }
}