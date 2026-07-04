namespace Moira.Common.Abstractions.Exceptions;

public class EntityValidationException(string message, Exception? innerException = null)
    : MoiraException(message, MoiraExceptionType.Validation, MoiraExceptionReason.ValidationFailed, innerException);
