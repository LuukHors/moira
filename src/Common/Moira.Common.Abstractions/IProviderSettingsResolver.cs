using Moira.Common.Abstractions.Models;

namespace Moira.Common.Abstractions;

public interface IProviderSettingsResolver
{
    bool CanResolve(ResourceRef entity);
    Task<IdpProviderSpecificSettings?> ResolveAsync(ResourceRef entity, CancellationToken cancellationToken);
}