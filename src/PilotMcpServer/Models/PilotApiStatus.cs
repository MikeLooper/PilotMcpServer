namespace PilotMcpServer.Models;

/// <summary>
/// Availability/version snapshot of one configured Pilot API, as reported by its "/about" endpoint.
/// </summary>
public sealed class PilotApiStatus
{
	/// <summary>
	/// The name of the configured Pilot API, as specified in the configuration file.
	/// </summary>
	public required string Name { get; init; }

	/// <summary>
	/// The host of the configured Pilot API, as specified in the configuration file.
	/// </summary>
	public required string Host { get; init; }

	/// <summary>
	/// The port of the configured Pilot API, as specified in the configuration file.
	/// </summary>
	public required int Port { get; init; }

	/// <summary>
	/// Indicates whether this Pilot API is the one currently selected for use by the MCP server.
	/// </summary>
	public required bool IsSelected { get; init; }

	/// <summary>
	/// Indicates whether this Pilot API is the default one, as specified in the configuration file.
	/// </summary>
	public required bool IsDefault { get; init; }

	/// <summary>
	/// Indicates whether this Pilot API is available (i.e., reachable and responding to requests).
	/// </summary>
	public required bool IsAvailable { get; init; }

	/// <summary>
	/// The API version of the configured Pilot API, as reported by its "/about" endpoint.
	/// </summary>
	public string? ApiVersion { get; init; }

	/// <summary>
	/// The build version of the configured Pilot API, as reported by its "/about" endpoint.
	/// </summary>
	public string? BuildVersion { get; init; }

	/// <summary>
	/// The deploy date of the configured Pilot API, as reported by its "/about" endpoint.
	/// </summary>
	public string? DeployDate { get; init; }

	/// <summary>
	/// The error message encountered when trying to reach the configured Pilot API, if any.
	/// </summary>
	public string? Error { get; init; }
}
