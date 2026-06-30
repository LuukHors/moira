using Moira.Common.Abstractions.Exceptions;
using Moira.Common.Abstractions.Models;

namespace Moira.Common.Abstractions.Commands;

public record IdPCommandResult<TEntity>(Guid Id, TEntity Entity, MoiraException? Exception = null) where TEntity : IdPEntityBase;
