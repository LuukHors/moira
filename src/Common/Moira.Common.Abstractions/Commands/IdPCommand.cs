using Moira.Common.Abstractions.Models;

namespace Moira.Common.Abstractions.Commands;

public record IdPCommand<TEntity>(Guid Id, TEntity Entity) where TEntity : IdPEntityBase;
