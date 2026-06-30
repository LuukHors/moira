namespace Moira.Common.Abstractions.Exceptions;

public class IdPException : MoiraException
{
    public IdPException(string message, IdPExceptionReason reason = IdPExceptionReason.IdpError, Exception? innerException = null)
        : this(message, MoiraExceptionType.Idp, reason, innerException)
    {
    }

    protected IdPException(string message, MoiraExceptionType type, IdPExceptionReason reason, Exception? innerException = null)
        : base(message, type, MapReason(reason), innerException)
    {
        IdPReason = reason;
    }

    public IdPExceptionReason IdPReason { get; }

    private static MoiraExceptionReason MapReason(IdPExceptionReason reason)
    {
        return reason switch
        {
            IdPExceptionReason.IdpRequestFailed => MoiraExceptionReason.IdpRequestFailed,
            IdPExceptionReason.IdpValidationFailed => MoiraExceptionReason.IdpValidationFailed,
            _ => MoiraExceptionReason.IdpError
        };
    }
}
