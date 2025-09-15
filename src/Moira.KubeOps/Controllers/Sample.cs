using Microsoft.Extensions.Logging;

namespace Moira.KubeOps.Controllers;

public class Sample(ILogger<Sample> logger) : ISample
{
    public Task Test()
    {
        logger.LogInformation("This is a test.");

        return Task.CompletedTask;
    }
}