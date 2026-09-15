using ModelContextProtocol.Protocol;

namespace PilotMcpServer.Extensions;

/// <summary>Builds the static <see cref="Implementation"/> metadata describing this MCP server.</summary>
public static class McpServerInfoFactory
{
	public const string ServerDescription = "A custom MCP Server that will read data from any of 6 flavors of Pilot APIs (reading Northwind data from 2 databases)";
	public const string ServerName = "Pilot MCP Server";
	public const string ServerTitle = "Pilot MCP Server";
	public const string ServerVersion = "1.0.0";
	public const string ServerWebsiteUrl = "https://github.com/MikeLooper";

	public static Implementation Create() => new()
	{
		Description = ServerDescription,
		Name = ServerName,
		Title = ServerTitle,
		Version = ServerVersion,
		WebsiteUrl = ServerWebsiteUrl
	};
}
