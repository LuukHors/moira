using Moira.Common.Commands;
using Moira.Common.Models;

namespace Moira.Common.Logging;

public interface ILoggingUtilities<TEntity> where TEntity : IdPEntity
{
    void LogInformation(IdPCommand<TEntity> command, string message, params object[] args);
}