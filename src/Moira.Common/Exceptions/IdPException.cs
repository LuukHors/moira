namespace Moira.Common.Exceptions;

public class IdPException(string message, IdpExceptionType type) : Exception(message)
{
    public IdpExceptionType Type { get; } = type;
}
