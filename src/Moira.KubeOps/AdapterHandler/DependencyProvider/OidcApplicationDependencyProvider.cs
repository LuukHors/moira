using Moira.Common.Models;
using Moira.KubeOps.Entities;

namespace Moira.KubeOps.DependencyProvider;

public class OidcApplicationDependencyProvider : IDependencyProvider<OidcApplication, IdPOidcApplication>
{
    public Task<IdPOidcApplication> ResolveAsync(OidcApplication entity, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}