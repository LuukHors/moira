using Moira.Common.Models;

namespace Moira.Common.Commands;

public record IdPCommand<TEntity>(Guid Id, TEntity Entity) where TEntity : IdPEntity;