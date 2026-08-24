using System.Text.Json.Serialization;

namespace PilotMcpServer.Models;

/// <summary>Payload returned by a Pilot API's "/about" endpoint.</summary>
public sealed class AboutResponse
{
    [JsonPropertyName("name")]
    public string? Name { get; init; }

    [JsonPropertyName("apiVersion")]
    public string? ApiVersion { get; init; }

    [JsonPropertyName("buildVersion")]
    public string? BuildVersion { get; init; }

    [JsonPropertyName("deployDate")]
    public string? DeployDate { get; init; }
}
