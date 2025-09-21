namespace Moira.Common.Exceptions;

public class IdPException(string message, IdpExceptionType type) : Exception(message);