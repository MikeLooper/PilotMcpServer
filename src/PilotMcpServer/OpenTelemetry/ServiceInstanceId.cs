using System.Text.RegularExpressions;

namespace PilotMcpServer.OpenTelemetry;

/// <summary>
/// Builds this server's instance ID: its service name in lowercase kebab case followed by the target Pilot API's
/// suffix (the text after the second-to-last dash of its container name), e.g. <c>PilotMcpServer</c> and
/// <c>pilot-api-dotnet-mssql</c> give <c>pilot-mcp-server-dotnet-mssql</c>.
/// </summary>
public static partial class ServiceInstanceId
{
	/// <summary>Combines <paramref name="serviceName"/> and the suffix of <paramref name="targetApiContainerName"/>.</summary>
	public static string Create(string serviceName, string targetApiContainerName)
	{
		var serviceId = ToKebabCase(serviceName);
		var suffix = GetSuffix(targetApiContainerName);
		return suffix.Length == 0 ? serviceId : $"{serviceId}-{suffix}";
	}

	/// <summary>
	/// Returns the text after the second-to-last dash of <paramref name="targetApiContainerName"/>, or all of it when it
	/// has fewer than two dashes.
	/// </summary>
	public static string GetSuffix(string targetApiContainerName)
	{
		var trimmed = targetApiContainerName.Trim();
		var lastDash = trimmed.LastIndexOf('-');
		var secondToLastDash = lastDash > 0 ? trimmed.LastIndexOf('-', lastDash - 1) : -1;
		return trimmed[(secondToLastDash + 1)..].ToLowerInvariant();
	}

	/// <summary>Converts e.g. <c>PilotMcpServer</c>, <c>Pilot MCP Server</c> or <c>pilot_mcp_server</c> to <c>pilot-mcp-server</c>.</summary>
	public static string ToKebabCase(string value)
	{
		var separated = WordBoundary().Replace(value.Trim(), "-");
		return Separators().Replace(separated, "-").Trim('-').ToLowerInvariant();
	}

	// A lower case letter or digit followed by an upper case letter, or the last capital of an acronym ("MCPServer").
	[GeneratedRegex("(?<=[a-z0-9])(?=[A-Z])|(?<=[A-Z])(?=[A-Z][a-z])")]
	private static partial Regex WordBoundary();

	[GeneratedRegex(@"[\s_.\-]+")]
	private static partial Regex Separators();
}
