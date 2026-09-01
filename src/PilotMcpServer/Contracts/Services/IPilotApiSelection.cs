using PilotMcpServer.Models;

namespace PilotMcpServer.Contracts.Services;

/// <summary>Tracks which configured Pilot API is used by default when a tool call doesn't override it.</summary>
public interface IPilotApiSelection
{
    /// <summary>The currently selected Pilot API.</summary>
    PilotApiEndpoint Current { get; }

    /// <summary>
    /// Selects the Pilot API to use by default for subsequent calls.
    /// </summary>
    /// <param name="name">Exact (case-insensitive) name of a Pilot API from <see cref="Configuration.PilotApiCatalog"/>.</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="name"/> does not match a known Pilot API.</exception>
    void SetCurrent(string name);
}
