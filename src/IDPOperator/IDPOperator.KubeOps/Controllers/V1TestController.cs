using KubeOps.Abstractions.Controller;

namespace IDPOperator.KubeOps.Controllers;

public class V1TestController : IEntityController<V1TestEntity>
{
    public Task ReconcileAsync(V1TestEntity entity, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task DeletedAsync(V1TestEntity entity, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}