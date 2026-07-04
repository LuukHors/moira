namespace Moira.Common.Abstractions.Exceptions;

public abstract class MoiraException : Exception
{
    protected MoiraException(string message, MoiraExceptionType type, MoiraExceptionReason reason, Exception? innerException = null)
        : base(message, innerException)
    {
        Type = type;
        Reason = reason;
    }

    public MoiraExceptionType Type { get; }
    public MoiraExceptionReason Reason { get; }
}
