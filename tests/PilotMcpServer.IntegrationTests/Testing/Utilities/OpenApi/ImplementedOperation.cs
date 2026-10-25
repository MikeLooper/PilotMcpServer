namespace PilotMcpServer.IntegrationTests.Testing.Utilities.OpenApi;

/// <summary>One Pilot API operation as the client implements it.</summary>
public sealed class ImplementedOperation
{
    /// <summary>The upper-case HTTP method, e.g. GET.</summary>
    public required string Method { get; init; }

    public required string Path { get; init; }

    public required string Tag { get; init; }

    public required string Summary { get; init; }

    /// <summary>The MCP tool that calls the operation, as "ClassName.MethodName".</summary>
    public required string ToolName { get; init; }

    public IReadOnlyList<ImplementedParameter> Parameters { get; init; } = [];

    public ImplementedRequestBody? RequestBody { get; init; }

    /// <summary>The media type the client reads response bodies as.</summary>
    public required string ResponseMediaType { get; init; }

    /// <summary>
    /// The schema the client deserializes a 2xx body into, or <see langword="null"/> when the client ignores the
    /// body. The client accepts every 2xx status code as success.
    /// </summary>
    public ImplementedSchema? SuccessSchema { get; init; }

    /// <summary>Whether the client treats 404 Not Found as a normal "no record" result rather than an error.</summary>
    public bool HandlesNotFound { get; init; }

    /// <summary>
    /// The schema the client deserializes 4xx/5xx bodies into (falling back to raw text when the body is not JSON),
    /// or <see langword="null"/> when the client only looks at the status code.
    /// </summary>
    public ImplementedSchema? ErrorSchema { get; init; }

    /// <summary>Whether the client attaches a bearer token to the request (when security is active).</summary>
    public required bool SendsBearerToken { get; init; }

    public string Key => $"{Method} {Path}";
}
