using System.Net;

namespace Moira.Common.Exceptions;

public class HttpException(string message, HttpStatusCode statusCode, string method, string url) : Exception(message)
{
    public HttpStatusCode StatusCode => statusCode;
    public string Method => method;
    public string Url => url;
}