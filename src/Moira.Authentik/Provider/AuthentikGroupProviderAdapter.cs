using Microsoft.Extensions.Logging;
using Moira.Common.Commands;
using Moira.Common.Models;
using Moira.Common.Provider;

namespace Moira.Authentik.Provider;

public class AuthentikGroupProviderAdapter(
    ILogger<AuthentikGroupProviderAdapter> logger) : IProviderAdapter<IdPGroup>
{
    private const string ProviderName = "Authentik";
    public string Name { get; } = ProviderName;
    
    public Task<IdPCommandResult> ExecuteAsync(IdPCommand<IdPGroup> command)
    {
        logger.LogInformation("Authentik adapter");
        return Task.FromResult(new IdPCommandResult());
    }
}