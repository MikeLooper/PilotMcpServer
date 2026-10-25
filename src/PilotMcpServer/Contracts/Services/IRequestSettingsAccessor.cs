using PilotMcpServer.Models;

namespace PilotMcpServer.Contracts.Services;

/// <summary>
/// Provides access to the <see cref="RequestSettings"/> of the MCP request currently being handled. The value
/// flows with the async execution context, so concurrent tool calls each see their own settings.
/// </summary>
public interface IRequestSettingsAccessor
{
	/// <summary>The current request's settings, or <see langword="null"/> outside of a tool call.</summary>
	RequestSettings? Current { get; set; }
}
