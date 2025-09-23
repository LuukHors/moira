using Moira.Common.Exceptions;

namespace Moira.KubeOps.Mappers;

public static class ExceptionMappers
{
    public static IdPException ToIdPException(this InvalidOperationException ex)
    {
        return new IdPException(ex.Message, IdpExceptionType.Logical);
    }

    public static IdPException ToIdPException(this Exception ex)
    {
        return new IdPException(ex.Message, IdpExceptionType.Unknown);
    }

    public static IdPException ToIdPException(this HttpException ex)
    {
        return new IdPException($"Http error {ex.Method} {ex.Url}({ex.StatusCode.ToString()}): {ex.Message}", IdpExceptionType.Http);
    }
}