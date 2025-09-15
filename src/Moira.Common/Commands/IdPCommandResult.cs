using Moira.Common.Models;

namespace Moira.Common.Commands;

public record IdPCommandResult<TEntity>(Guid Id, TEntity Entity) where TEntity : IdPEntity;