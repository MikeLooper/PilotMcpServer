namespace PilotMcpServer.Models;

/// <summary>
/// Identifies one deployment of the Pilot API that this server can call. <see cref="ContainerName"/> is the
/// deployment's identifier (e.g. <c>pilot-api-dotnet-mssql</c>), whichever of it or a hostname <see cref="Host"/> holds.
/// </summary>
public sealed record PilotApiEndpoint(string Name, string Host, int Port, string ContainerName)
{
	/// <summary>
	/// The "http://host:port" prefix used to build absolute request URIs for this API.
	/// </summary>
	public string BaseUrl => $"http://{Host}:{Port}";
}
