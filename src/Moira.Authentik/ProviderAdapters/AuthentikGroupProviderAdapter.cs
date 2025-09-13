using Microsoft.Extensions.Logging;
using Moira.Common.Commands;
using Moira.Common.Models;
using Moira.Common.Provider;

namespace Moira.Authentik.Provider;

public class AuthentikGroupProviderAdapter(
    ILogger<AuthentikGroupProviderAdapter> logger) : AbstractAuthentikProviderAdapter, IProviderAdapter<IdPGroup>
{    
    public Task<IdPCommandResult> ExecuteAsync(IdPCommand<IdPGroup> command)
    {
        logger.LogInformation("Authentik adapter");
        return Task.FromResult(new IdPCommandResult());
    }
}