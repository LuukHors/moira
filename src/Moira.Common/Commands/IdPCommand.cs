using Moira.Common.Models;

namespace Moira.Common.Commands;

public record IdPCommand<TEntity>(Guid CommandId, TEntity entity) where TEntity : IdPEntity;