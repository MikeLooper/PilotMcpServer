using PilotMcpServer.Models;

namespace PilotMcpServer.Services;

/// <summary>
/// Executes JSON HTTP calls against a Pilot API. Every method resolves which API to call from an optional
/// <paramref name="apiName"/> override, falling back to the currently selected API (see <see cref="IPilotApiSelection"/>).
/// </summary>
public interface IPilotHttpClient
{
    Task<IReadOnlyList<T>> GetJsonListAsync<T>(string path, string? apiName, CancellationToken cancellationToken);

    /// <summary>Returns <see langword="null"/> when the server responds 404 Not Found.</summary>
    Task<T?> GetJsonAsync<T>(string path, string? apiName, CancellationToken cancellationToken) where T : class;

    Task<TResponse> PostJsonAsync<TRequest, TResponse>(string path, TRequest body, string? apiName, CancellationToken cancellationToken);

    Task PutJsonAsync<TRequest>(string path, TRequest body, string? apiName, CancellationToken cancellationToken);

    Task DeleteAsync(string path, string? apiName, CancellationToken cancellationToken);

    Task<AboutResponse> GetAboutAsync(PilotApiEndpoint endpoint, CancellationToken cancellationToken);

    /// <summary>Resolves an optional per-call API name override to a concrete endpoint, defaulting to the current selection.</summary>
    PilotApiEndpoint ResolveEndpoint(string? apiName);
}
