using Moira.Common.Exceptions;
using Moira.Common.Models;

namespace Moira.Common.Commands;

public record IdPCommandResult<TEntity>(Guid Id, TEntity Entity, MoiraException? Exception = null) where TEntity : IdPEntityBase;
