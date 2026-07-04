namespace Moira.Common.Abstractions.Exceptions;

public abstract class DependencyException(string message, MoiraExceptionReason reason, Exception? innerException = null)
    : MoiraException(message, MoiraExceptionType.Dependency, reason, innerException);
