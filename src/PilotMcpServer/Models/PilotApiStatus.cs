namespace PilotMcpServer.Models;

/// <summary>Availability/version snapshot of one configured Pilot API, as reported by its "/about" endpoint.</summary>
public sealed class PilotApiStatus
{
    public required string Name { get; init; }
    public required string Host { get; init; }
    public required int Port { get; init; }
    public required bool IsSelected { get; init; }
    public required bool IsDefault { get; init; }
    public required bool IsAvailable { get; init; }
    public string? ApiVersion { get; init; }
    public string? BuildVersion { get; init; }
    public string? DeployDate { get; init; }
    public string? Error { get; init; }
}
