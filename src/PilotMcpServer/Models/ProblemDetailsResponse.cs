using System.Text.Json.Serialization;

namespace PilotMcpServer.Models;

/// <summary>RFC 7807 problem-details payload returned by a Pilot API on a 400 (or similar) response.</summary>
public sealed class ProblemDetailsResponse
{
    [JsonPropertyName("type")]
    public string? Type { get; init; }

    [JsonPropertyName("title")]
    public string? Title { get; init; }

    [JsonPropertyName("status")]
    public int? Status { get; init; }

    [JsonPropertyName("detail")]
    public string? Detail { get; init; }

    [JsonPropertyName("instance")]
    public string? Instance { get; init; }
}
