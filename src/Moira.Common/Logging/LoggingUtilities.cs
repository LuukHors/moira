using Microsoft.Extensions.Logging;
using Moira.Common.Commands;
using Moira.Common.Models;

namespace Moira.Common.Logging;

public class LoggingUtilities<TEntity>(
    ILogger<LoggingUtilities<TEntity>> logger) : ILoggingUtilities<TEntity> where TEntity : IdPEntity
{
    public void LogInformation(IdPCommand<TEntity> command, string message, params object[] args)
    {
        logger.LogInformation(message, args);
    }
}