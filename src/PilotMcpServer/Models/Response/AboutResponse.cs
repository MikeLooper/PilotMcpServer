using System.Text.Json.Serialization;

namespace PilotMcpServer.Models;

/// <summary>
/// Payload returned by a Pilot API's "/about" endpoint.
/// </summary>
public sealed class AboutResponse
{
	/// <summary>
	/// The name of the Pilot API.
	/// </summary>
	[JsonPropertyName("name")]
	public string? Name { get; init; }

	/// <summary>
	/// The version of the Pilot API.
	/// </summary>
	[JsonPropertyName("apiVersion")]
	public string? ApiVersion { get; init; }

	/// <summary>
	/// The build version of the Pilot API.
	/// </summary>
	[JsonPropertyName("buildVersion")]
	public string? BuildVersion { get; init; }

	/// <summary>
	/// The date the Pilot API was deployed.
	/// </summary>
	[JsonPropertyName("deployDate")]
	public string? DeployDate { get; init; }
}
