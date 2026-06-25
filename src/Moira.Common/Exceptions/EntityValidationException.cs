namespace Moira.Common.Exceptions;

public class EntityValidationException(string message, Exception? innerException = null)
    : MoiraException(message, MoiraExceptionType.Validation, MoiraExceptionReason.ValidationFailed, innerException);
