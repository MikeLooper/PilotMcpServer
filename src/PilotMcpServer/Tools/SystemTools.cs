using System.ComponentModel;
using ModelContextProtocol;
using ModelContextProtocol.Server;
using PilotMcpServer.Configuration;
using PilotMcpServer.Contracts.Services;
using PilotMcpServer.Models;
using PilotMcpServer.Services;

namespace PilotMcpServer.Tools;

[McpServerToolType]
public sealed class SystemTools(IPilotHttpClient client, IPilotApiSelection selection)
{
    private static readonly TimeSpan AboutCallTimeout = TimeSpan.FromSeconds(5);

    [McpServerTool(Name = "select_api")]
    [Description("Selects which Pilot API deployment subsequent tool calls should use by default, when they don't specify an explicit apiName override. See list_apis for the set of valid names.")]
    public Task SelectApiAsync(
        [Description("Exact name of the Pilot API to select, e.g. '.NET Core with SQL Server'. Required.")] string apiName,
        CancellationToken cancellationToken)
    {
        try
        {
            selection.SetCurrent(apiName);
        }
        catch (ArgumentException ex)
        {
            throw new McpException(ex.Message);
        }

        return Task.CompletedTask;
    }

    [McpServerTool(Name = "list_apis")]
    [Description("Lists every configured Pilot API deployment along with its live availability, reported version, and deploy date (read from each API's /about endpoint), and flags which one is currently selected.")]
    public async Task<IReadOnlyList<PilotApiStatus>> ListApisAsync(CancellationToken cancellationToken)
    {
        var current = selection.Current;
        var tasks = PilotApiCatalog.All.Select(endpoint => GetStatusAsync(endpoint, current, cancellationToken));
        return await Task.WhenAll(tasks).ConfigureAwait(false);
    }

    [McpServerTool(Name = "list_endpoints")]
    [Description("Lists a summary of the logical endpoints exposed by the Pilot API contract (grouped by resource, not repeated per deployment, since every configured API implements the identical contract).")]
    public Task<IReadOnlyList<EndpointSummary>> ListEndpointsAsync(CancellationToken cancellationToken)
        => Task.FromResult(PilotEndpointCatalog.All);

    [McpServerTool(Name = "get_healthcheck")]
    [Description("Calls the /healthcheck endpoint on a Pilot API and returns whether it is healthy (true) or not (false).")]
    public Task<bool> GetHealthCheckAsync(
        [Description("Optional. Name of the Pilot API to call (see list_apis). Defaults to the currently selected API.")] string? apiName = null,
        CancellationToken cancellationToken = default)
        => client.GetHealthCheckAsync(apiName, cancellationToken);

    [McpServerTool(Name = "get_about")]
    [Description("Calls the /about endpoint on a Pilot API and returns application metadata: name, API version, build version, and deploy date.")]
    public Task<AboutResponse> GetAboutAsync(
        [Description("Optional. Name of the Pilot API to call (see list_apis). Defaults to the currently selected API.")] string? apiName = null,
        CancellationToken cancellationToken = default)
    {
        var endpoint = client.ResolveEndpoint(apiName);
        return client.GetAboutAsync(endpoint, cancellationToken);
    }

    private async Task<PilotApiStatus> GetStatusAsync(PilotApiEndpoint endpoint, PilotApiEndpoint current, CancellationToken cancellationToken)
    {
        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCts.CancelAfter(AboutCallTimeout);

        var isSelected = string.Equals(endpoint.Name, current.Name, StringComparison.OrdinalIgnoreCase);
        var isDefault = string.Equals(endpoint.Name, PilotApiCatalog.Default.Name, StringComparison.OrdinalIgnoreCase);

        try
        {
            var about = await client.GetAboutAsync(endpoint, timeoutCts.Token).ConfigureAwait(false);
            return new PilotApiStatus
            {
                Name = endpoint.Name,
                Host = endpoint.Host,
                Port = endpoint.Port,
                IsSelected = isSelected,
                IsDefault = isDefault,
                IsAvailable = true,
                ApiVersion = about.ApiVersion,
                BuildVersion = about.BuildVersion,
                DeployDate = about.DeployDate,
            };
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or PilotApiException)
        {
            return new PilotApiStatus
            {
                Name = endpoint.Name,
                Host = endpoint.Host,
                Port = endpoint.Port,
                IsSelected = isSelected,
                IsDefault = isDefault,
                IsAvailable = false,
                Error = ex.Message,
            };
        }
    }
}
