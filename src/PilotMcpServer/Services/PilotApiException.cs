using System.Net;
using PilotMcpServer.Models;

namespace PilotMcpServer.Services;

/// <summary>Thrown when a Pilot API call returns a non-success status code.</summary>
public sealed class PilotApiException : Exception
{
    public HttpStatusCode StatusCode { get; }
    public ProblemDetailsResponse? Problem { get; }

    public PilotApiException(HttpStatusCode statusCode, ProblemDetailsResponse? problem)
        : base(BuildMessage(statusCode, problem))
    {
        StatusCode = statusCode;
        Problem = problem;
    }

    private static string BuildMessage(HttpStatusCode statusCode, ProblemDetailsResponse? problem)
    {
        var detail = problem?.Detail ?? problem?.Title;
        return detail is null
            ? $"Pilot API request failed with status {(int)statusCode} ({statusCode})."
            : $"Pilot API request failed with status {(int)statusCode} ({statusCode}): {detail}";
    }
}
