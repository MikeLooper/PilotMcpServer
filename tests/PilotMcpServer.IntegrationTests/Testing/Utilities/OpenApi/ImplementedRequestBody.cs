namespace PilotMcpServer.IntegrationTests.Testing.Utilities.OpenApi;

/// <summary>The request body the client sends with an operation.</summary>
public sealed class ImplementedRequestBody
{
    public required string MediaType { get; init; }

    /// <summary>Whether the client sends a body on every call.</summary>
    public required bool Required { get; init; }

    public required ImplementedSchema Schema { get; init; }
}
