using System.Net;

namespace Moira.Common.Exceptions;

public class HttpException(string message, HttpStatusCode? statusCode, string method, string url, int? statusCodeNumber = 0) : Exception(message)
{
    public HttpStatusCode? StatusCode => statusCode;
    public int? StatusCodeNumber => statusCodeNumber;
    public string Method => method;
    public string Url => url;
}