using Moira.Common.Models;

namespace Moira.Authentik.Application.Builders;

public interface IAuthentikScopeMappingBuilder
{
    Task<IEnumerable<string>> BuildAsync(IEnumerable<string> scopeNames, IdPProvider provider, CancellationToken cancellationToken);
}