namespace Moira.Common.Abstractions.Exceptions;

public class UnknownMoiraException(string message, Exception innerException)
    : MoiraException(message, MoiraExceptionType.Unknown, MoiraExceptionReason.UnknownError, innerException);
