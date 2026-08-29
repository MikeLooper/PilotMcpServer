using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using PilotMcpServer.Configuration;
using PilotMcpServer.Contracts.Services;
using PilotMcpServer.Models;

namespace PilotMcpServer.Services;

/// <summary>
/// Executes JSON calls against Pilot API deployments. Never sets <see cref="HttpClient.BaseAddress"/> on the
/// shared, dependency-injected <see cref="HttpClient"/>, since it is used concurrently against several
/// different hosts (e.g. by the list_apis tool's fan-out); every request builds an absolute URI instead.
/// </summary>
public sealed class PilotHttpClient(HttpClient httpClient, IPilotApiSelection selection) : IPilotHttpClient
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    public PilotApiEndpoint ResolveEndpoint(string? apiName)
    {
        if (string.IsNullOrWhiteSpace(apiName))
        {
            return selection.Current;
        }

        if (!PilotApiCatalog.TryGet(apiName, out var endpoint))
        {
            var validNames = string.Join(", ", PilotApiCatalog.All.Select(a => $"\"{a.Name}\""));
            throw new ArgumentException($"Unknown Pilot API \"{apiName}\". Valid options are: {validNames}.", nameof(apiName));
        }

        return endpoint;
    }

    public async Task<IReadOnlyList<T>> GetJsonListAsync<T>(string path, string? apiName, CancellationToken cancellationToken)
    {
        var endpoint = ResolveEndpoint(apiName);
        using var response = await httpClient.GetAsync(BuildUri(endpoint, path), cancellationToken).ConfigureAwait(false);
        await EnsureSuccessAsync(response, cancellationToken).ConfigureAwait(false);
        var result = await response.Content.ReadFromJsonAsync<List<T>>(SerializerOptions, cancellationToken).ConfigureAwait(false);
        return result ?? [];
    }

    public async Task<T?> GetJsonAsync<T>(string path, string? apiName, CancellationToken cancellationToken) where T : class
    {
        var endpoint = ResolveEndpoint(apiName);
        using var response = await httpClient.GetAsync(BuildUri(endpoint, path), cancellationToken).ConfigureAwait(false);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        await EnsureSuccessAsync(response, cancellationToken).ConfigureAwait(false);
        return await response.Content.ReadFromJsonAsync<T>(SerializerOptions, cancellationToken).ConfigureAwait(false);
    }

    public async Task<TResponse> PostJsonAsync<TRequest, TResponse>(string path, TRequest body, string? apiName, CancellationToken cancellationToken)
    {
        var endpoint = ResolveEndpoint(apiName);
        using var response = await httpClient.PostAsJsonAsync(BuildUri(endpoint, path), body, SerializerOptions, cancellationToken).ConfigureAwait(false);
        await EnsureSuccessAsync(response, cancellationToken).ConfigureAwait(false);
        var result = await response.Content.ReadFromJsonAsync<TResponse>(SerializerOptions, cancellationToken).ConfigureAwait(false);
        return result ?? throw new PilotApiException(response.StatusCode, null);
    }

    public async Task PutJsonAsync<TRequest>(string path, TRequest body, string? apiName, CancellationToken cancellationToken)
    {
        var endpoint = ResolveEndpoint(apiName);
        using var response = await httpClient.PutAsJsonAsync(BuildUri(endpoint, path), body, SerializerOptions, cancellationToken).ConfigureAwait(false);
        await EnsureSuccessAsync(response, cancellationToken).ConfigureAwait(false);
    }

    public async Task DeleteAsync(string path, string? apiName, CancellationToken cancellationToken)
    {
        var endpoint = ResolveEndpoint(apiName);
        using var response = await httpClient.DeleteAsync(BuildUri(endpoint, path), cancellationToken).ConfigureAwait(false);
        await EnsureSuccessAsync(response, cancellationToken).ConfigureAwait(false);
    }

    public async Task<AboutResponse> GetAboutAsync(PilotApiEndpoint endpoint, CancellationToken cancellationToken)
    {
        using var response = await httpClient.GetAsync(BuildUri(endpoint, "/about"), cancellationToken).ConfigureAwait(false);
        await EnsureSuccessAsync(response, cancellationToken).ConfigureAwait(false);
        var result = await response.Content.ReadFromJsonAsync<AboutResponse>(SerializerOptions, cancellationToken).ConfigureAwait(false);
        return result ?? throw new PilotApiException(response.StatusCode, null);
    }

    public async Task<bool> GetHealthCheckAsync(string? apiName, CancellationToken cancellationToken)
    {
        var endpoint = ResolveEndpoint(apiName);
        using var response = await httpClient.GetAsync(BuildUri(endpoint, "/healthcheck"), cancellationToken).ConfigureAwait(false);
        return response.IsSuccessStatusCode;
    }

    private static Uri BuildUri(PilotApiEndpoint endpoint, string path) => new($"{endpoint.BaseUrl}{path}");

    private static async Task EnsureSuccessAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        ProblemDetailsResponse? problem = null;
        try
        {
            problem = await response.Content.ReadFromJsonAsync<ProblemDetailsResponse>(SerializerOptions, cancellationToken).ConfigureAwait(false);
        }
        catch (JsonException)
        {
            // Response body wasn't a ProblemDetails payload; fall through with no parsed detail.
        }

        throw new PilotApiException(response.StatusCode, problem);
    }
}
