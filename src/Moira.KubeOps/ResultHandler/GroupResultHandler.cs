using Moira.Common.Commands;
using Moira.Common.Models;

namespace Moira.KubeOps.ResultHandler;

public class GroupResultHandler : IResultHandler<IdPGroup>
{
    public Task HandleAsync(IdPCommandResult<IdPGroup> result, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}