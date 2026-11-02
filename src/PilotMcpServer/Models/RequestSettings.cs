namespace PilotMcpServer.Models;

/// <summary>
/// Data associated with the MCP request currently being handled. An instance is created when a tool call starts
/// and is available, through <see cref="Contracts.Services.IRequestSettingsAccessor"/>, until the call ends.
/// </summary>
public sealed class RequestSettings
{
	/// <summary>
	/// The correlation ID for the current request. It is sent to the Pilot API in the <c>X-Correlation-Id</c>
	/// header and recorded on OpenTelemetry spans and log records, so one tool call can be followed end to end.
	/// It is the value supplied by the caller, if any, and otherwise a new GUID.
	/// </summary>
	public string CorrelationId { get; init; } = Guid.NewGuid().ToString();

	/// <summary>
	/// The JSON-RPC <c>id</c> of the current MCP request, or <see langword="null"/> when it is not known. Recorded on
	/// spans and log records so one request can be told apart from others that share a correlation ID.
	/// </summary>
	public string? RequestId { get; init; }

	/// <summary>
	/// The name the MCP client gave in its <c>initialize</c> request (<c>clientInfo.name</c>, e.g.
	/// <c>Visual Studio Code</c>), or <see langword="null"/> when it is not known. Recorded on spans and log records.
	/// It is not sent to the Pilot API: the <c>X-Caller-Name</c> header names this server instead (see
	/// <see cref="Services.PilotApiHeadersDelegatingHandler"/>).
	/// </summary>
	public string? CallerName { get; init; }
}
