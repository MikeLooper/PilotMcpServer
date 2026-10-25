namespace PilotMcpServer.IntegrationTests.Testing.Utilities.OpenApi;

/// <summary>The Pilot API contract as implemented by this client application.</summary>
public sealed class ImplementedContract
{
    /// <summary>The API version the client targets (from the embedded catalog), e.g. "1".</summary>
    public string? ApiVersion { get; init; }

    /// <summary>The base URLs of every configured Pilot API deployment.</summary>
    public IReadOnlyList<string> ServerUrls { get; init; } = [];

    /// <summary>The OpenAPI security scheme type the client authenticates with, e.g. "http".</summary>
    public required string SecuritySchemeType { get; init; }

    /// <summary>The HTTP authorization scheme the client authenticates with, e.g. "bearer".</summary>
    public required string SecurityScheme { get; init; }

    public IReadOnlyList<ImplementedOperation> Operations { get; init; } = [];

    public IReadOnlySet<string> Tags => Operations.Select(operation => operation.Tag).ToHashSet(StringComparer.Ordinal);
}
