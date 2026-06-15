using Moira.Common.Exceptions;
using Moira.Common.Models;
using Moira.KubeOps.Entities;

namespace Moira.KubeOps.ResultHandler;

public class OidcApplicationResultHandler : IResultHandler<OidcApplication, IdPOidcApplication>
{
    public Task HandleAsync(OidcApplication entity, IdPOidcApplication idpEntity, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task HandleExceptionAsync(OidcApplication entity, IdPException exception, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task HandleDeleteAsync(OidcApplication entity, IdPOidcApplication idpEntity, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}