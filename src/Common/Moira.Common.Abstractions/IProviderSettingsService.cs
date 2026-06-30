using Moira.Common.Abstractions.Models;

namespace Moira.Common.Abstractions;

public interface IProviderSettingsService
{
    Task<IdpProviderSpecificSettings?> ResolveAsync(ResourceRef? entity, CancellationToken cancellationToken);
}