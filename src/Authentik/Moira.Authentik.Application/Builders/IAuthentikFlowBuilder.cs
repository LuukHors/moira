using Moira.Common.Models;

namespace Moira.Authentik.Application.Builders;

public interface IAuthentikFlowBuilder
{
    Task<string> BuildAsync(IdPProvider provider, string flowSlug, CancellationToken cancellationToken);
}