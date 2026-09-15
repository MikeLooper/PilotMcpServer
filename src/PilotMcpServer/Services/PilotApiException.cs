using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Encodings.Web;
using ModelContextProtocol;
using PilotMcpServer.Models;

namespace PilotMcpServer.Services;

/// <summary>
/// Thrown when a Pilot API call returns a non-success status code. Carries every piece of diagnostic
/// information available from the failed response so callers (and, ultimately, the MCP tool consumer) can
/// determine the failure point without re-running the call: the status code, any RFC 7807 problem details,
/// the raw response body (when it wasn't parseable as problem details), and the request that failed.
/// </summary>
public sealed class PilotApiException : McpException
{
    private const int MaxRawBodyLength = 2000;
    private static readonly JsonSerializerOptions DiagnosticJsonOptions = new()
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };

    public HttpStatusCode StatusCode { get; }
    public ProblemDetailsResponse? Problem { get; }

    /// <summary>The raw response body, present only when it could not be parsed as <see cref="ProblemDetailsResponse"/>.</summary>
    public string? RawBody { get; }

    public HttpMethod? RequestMethod { get; }
    public Uri? RequestUri { get; }
    public MediaTypeHeaderValue? ContentType { get; }

    public PilotApiException()
    {
    }

    public PilotApiException(string message)
        : base(message)
    {
    }

    public PilotApiException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    public PilotApiException(
        HttpStatusCode statusCode,
        ProblemDetailsResponse? problem,
        string? rawBody = null,
        HttpMethod? requestMethod = null,
        Uri? requestUri = null,
        MediaTypeHeaderValue? contentType = null)
        : base(BuildMessage(statusCode, problem, rawBody, requestMethod, requestUri, contentType))
    {
        StatusCode = statusCode;
        Problem = problem;
        RawBody = problem is null ? rawBody : null;
        RequestMethod = requestMethod;
        RequestUri = requestUri;
        ContentType = contentType;
    }

    private static string BuildMessage(
        HttpStatusCode statusCode,
        ProblemDetailsResponse? problem,
        string? rawBody,
        HttpMethod? requestMethod,
        Uri? requestUri,
        MediaTypeHeaderValue? contentType)
    {
        var detail = problem?.Detail ?? problem?.Title;
        var boundedRawBody = rawBody is null || rawBody.Length <= MaxRawBodyLength
            ? rawBody
            : string.Concat(rawBody.AsSpan(0, MaxRawBodyLength), "... (truncated)");

        return JsonSerializer.Serialize(new
        {
            statusCode = (int)statusCode,
            status = statusCode.ToString(),
            description = detail ?? $"Pilot API request failed with status {(int)statusCode} ({statusCode}).",
            problem,
            rawBody = boundedRawBody,
            request = requestMethod is null && requestUri is null
                ? null
                : new
                {
                    method = requestMethod?.Method,
                    uri = requestUri?.ToString(),
                },
            contentType = contentType?.ToString(),
        }, DiagnosticJsonOptions);
    }
}
