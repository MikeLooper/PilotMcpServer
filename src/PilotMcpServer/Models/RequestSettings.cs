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
	/// </summary>
	public string CorrelationId { get; init; } = Guid.NewGuid().ToString();
}
