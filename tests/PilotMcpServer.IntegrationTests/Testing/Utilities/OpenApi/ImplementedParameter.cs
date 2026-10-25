using Microsoft.OpenApi;

namespace PilotMcpServer.IntegrationTests.Testing.Utilities.OpenApi;

/// <summary>A path, query or header parameter the client sends with an operation.</summary>
public sealed class ImplementedParameter
{
    public required string Name { get; init; }

    public required ParameterLocation Location { get; init; }

    /// <summary>Whether the client sends the parameter on every call (it cannot be omitted by the caller).</summary>
    public required bool AlwaysSent { get; init; }

    public required ImplementedSchema Schema { get; init; }

    public string? Description { get; init; }
}
