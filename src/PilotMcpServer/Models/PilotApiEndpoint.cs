namespace PilotMcpServer.Models;

/// <summary>Identifies one deployment of the Pilot API that this server can call.</summary>
public sealed record PilotApiEndpoint(string Name, string Host, int Port)
{
    /// <summary>The "http://host:port" prefix used to build absolute request URIs for this API.</summary>
    public string BaseUrl => $"http://{Host}:{Port}";
}
