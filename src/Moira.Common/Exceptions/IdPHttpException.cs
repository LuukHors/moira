using System.Net;

namespace Moira.Common.Exceptions;

public class IdPHttpException(
    string message,
    HttpStatusCode? statusCode,
    string method,
    string url,
    int? statusCodeNumber = 0,
    Exception? innerException = null)
    : IdPException(message, MoiraExceptionType.IdpHttp, IdPExceptionReason.IdpRequestFailed, innerException)
{
    public HttpStatusCode? StatusCode { get; } = statusCode;
    public int? StatusCodeNumber { get; } = statusCodeNumber;
    public string Method { get; } = method;
    public string Url { get; } = url;
}
