using System.Threading;
using PilotMcpServer.Configuration;
using PilotMcpServer.Models;

namespace PilotMcpServer.Services;

/// <summary>Thread-safe singleton implementation of <see cref="IPilotApiSelection"/>, seeded to the catalog default.</summary>
public sealed class PilotApiSelectionState : IPilotApiSelection
{
    private readonly Lock _lock = new();
    private PilotApiEndpoint _current = PilotApiCatalog.Default;

    public PilotApiEndpoint Current
    {
        get
        {
            lock (_lock)
            {
                return _current;
            }
        }
    }

    public void SetCurrent(string name)
    {
        if (!PilotApiCatalog.TryGet(name, out var endpoint))
        {
            var validNames = string.Join(", ", PilotApiCatalog.All.Select(a => $"\"{a.Name}\""));
            throw new ArgumentException($"Unknown Pilot API \"{name}\". Valid options are: {validNames}.", nameof(name));
        }

        lock (_lock)
        {
            _current = endpoint;
        }
    }
}
