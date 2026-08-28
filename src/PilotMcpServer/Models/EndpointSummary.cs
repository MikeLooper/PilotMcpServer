namespace PilotMcpServer.Models;

/// <summary>
/// One logical endpoint offered by every configured Pilot API (they all share the same contract).
/// </summary>
public sealed class EndpointSummary
{
	/// <summary>
	/// The resource name for the endpoint, e.g. "users" or "projects".
	/// </summary>
	public required string Resource { get; init; }

	/// <summary>
	/// The operation name for the endpoint, e.g. "list", "get", "create", "update", or "delete".
	/// </summary>
	public required string Operation { get; init; }

	/// <summary>
	/// The HTTP method for the endpoint, e.g. "GET", "POST", "PUT", or "DELETE".
	/// </summary>
	public required string HttpMethod { get; init; }

	/// <summary>
	/// The path template for the endpoint, e.g. "/users" or "/projects/{projectId}".
	/// </summary>
	public required string PathTemplate { get; init; }

	/// <summary>
	/// A short description of the endpoint's purpose and behavior.
	/// </summary>
	public required string Summary { get; init; }
}
