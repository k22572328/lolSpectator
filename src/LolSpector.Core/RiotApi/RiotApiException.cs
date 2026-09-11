using System.Net;

namespace LolSpector.Core.RiotApi;

public class RiotApiException(string message, HttpStatusCode statusCode) : Exception(message)
{
    public HttpStatusCode StatusCode { get; } = statusCode;
}
