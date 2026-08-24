using PilotMcpServer.Models;

namespace PilotMcpServer.Configuration;

/// <summary>
/// The fixed set of Pilot API deployments this server can talk to. Compiled into the assembly (rather than
/// loaded from an external appsettings.json) so it cannot be altered post-deployment by editing a config file
/// on disk; a developer updates this file directly and rebuilds when the deployment topology changes.
/// </summary>
public static class PilotApiCatalog
{
    public static readonly IReadOnlyList<PilotApiEndpoint> All =
    [
        new(".NET Core with SQL Server", "localhost", 55101),
        new(".NET Core with PostgreSQL", "localhost", 55201),
        new("Java Spring Boot with SQL Server", "localhost", 55301),
        new("Java Spring Boot with PostgreSQL", "localhost", 55401),
        new("Python with SQL Server", "localhost", 55701),
        new("Python with PostgreSQL", "localhost", 55801),
    ];

    /// <summary>The API used when no explicit selection has been made.</summary>
    public static PilotApiEndpoint Default => All[0];

    /// <summary>Looks up a catalog entry by its exact (case-insensitive) name.</summary>
    public static bool TryGet(string name, out PilotApiEndpoint endpoint)
    {
        foreach (var candidate in All)
        {
            if (string.Equals(candidate.Name, name, StringComparison.OrdinalIgnoreCase))
            {
                endpoint = candidate;
                return true;
            }
        }

        endpoint = null!;
        return false;
    }
}
