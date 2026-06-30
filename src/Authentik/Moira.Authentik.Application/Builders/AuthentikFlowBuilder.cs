using Moira.Authentik.Application.Ports;
using Moira.Authentik.Domain.Applications;
using Moira.Common.Abstractions.Exceptions;
using Moira.Common.Abstractions.Models;

namespace Moira.Authentik.Application.Builders;

public class AuthentikFlowBuilder(
    IAuthentikRepository<AuthentikFlowV3, AuthentikFlowV3, string> flowRepository) : IAuthentikFlowBuilder
{
    public async Task<string> BuildAsync(IdPProvider provider, string flowSlug, CancellationToken cancellationToken)
    {
        var flow = await flowRepository.GetByIdAsync(flowSlug, provider, null, cancellationToken);

        if (flow is null || string.IsNullOrWhiteSpace(flow.pk))
        {
            throw new IdPException(
                $"Authentik flow \"{flowSlug}\" was not found.",
                IdPExceptionReason.IdpValidationFailed);
        }

        return flow.pk;
    }
}