using k8s.Models;
using Moira.Common.Models;
using Moira.KubeOps.Entities;

namespace Moira.KubeOps.DependencyProvider;

public class ProviderDependencyProvider : IDependencyProvider<Provider, IdPProvider>
{
    public Task<IdPProvider> ResolveAsync(Provider entity, CancellationToken cancellationToken)
    {
        return Task.FromResult(new IdPProvider(
            entity.Namespace(),
            entity.Name(),
            entity.Spec.Type.ToString()
        ));
    }
}