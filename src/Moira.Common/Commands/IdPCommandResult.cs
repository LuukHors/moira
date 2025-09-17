using Moira.Common.Models;

namespace Moira.Common.Commands;

public record IdPCommandResult<TEntity>(Guid Id, TEntity Entity, IdPCommandResultStatus Status) where TEntity : IdPEntity;