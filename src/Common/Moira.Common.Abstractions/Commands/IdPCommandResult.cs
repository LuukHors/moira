using Moira.Common.Abstractions.Models;

namespace Moira.Common.Abstractions.Commands;

public record IdPCommandResult<TEntity>(Guid Id, TEntity Entity) where TEntity : IdPEntityBase;
