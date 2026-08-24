namespace PilotMcpServer.Models;

/// <summary>One logical endpoint offered by every configured Pilot API (they all share the same contract).</summary>
public sealed class EndpointSummary
{
    public required string Resource { get; init; }
    public required string Operation { get; init; }
    public required string HttpMethod { get; init; }
    public required string PathTemplate { get; init; }
    public required string Summary { get; init; }
}
