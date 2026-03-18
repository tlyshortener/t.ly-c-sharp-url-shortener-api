using System;
using System.Net;

namespace Tly;

public sealed class TlyApiException : Exception
{
    public TlyApiException(HttpStatusCode statusCode, string responseBody)
        : base($"T.LY API request failed with status code {(int)statusCode} ({statusCode}).")
    {
        StatusCode = statusCode;
        ResponseBody = responseBody;
    }

    public HttpStatusCode StatusCode { get; }

    public string ResponseBody { get; }
}
