using ModelContextProtocol.Server;
using PilotMcpServer.Models;
using PilotMcpServer.Models.Dto;
using PilotMcpServer.Services;
using PilotMcpServer.Tools.Base;
using System.ComponentModel;

namespace PilotMcpServer.Tools;

[McpServerToolType]
public sealed class ShippersTools(IPilotHttpClient client) : ToolBase
{
    [McpServerTool(Name = "get_all_shippers")]
    [Description("Retrieves every shipping company from the selected Pilot API.")]
    public Task<IReadOnlyList<ShipperDto>> GetAllShippersAsync(
        [Description("Optional. Name of the Pilot API to call (see list_apis). Defaults to the currently selected API.")] string? apiName = null,
        CancellationToken cancellationToken = default)
        => client.GetJsonListAsync<ShipperDto>($"/v{this.ApiVersion}/shippers/get-all", apiName, cancellationToken);

    [McpServerTool(Name = "get_shipper")]
    [Description("Retrieves a single shipping company by its numeric ID. Returns null if no shipper with that ID exists.")]
    public Task<ShipperDto?> GetShipperAsync(
        [Description("Numeric ID of the shipper to retrieve. Required.")] int shipperId,
        [Description("Optional. Name of the Pilot API to call (see list_apis). Defaults to the currently selected API.")] string? apiName = null,
        CancellationToken cancellationToken = default)
        => client.GetJsonAsync<ShipperDto>($"/v{this.ApiVersion}/shippers/get/{shipperId}", apiName, cancellationToken);

    [McpServerTool(Name = "add_shipper")]
    [Description("Creates a new shipping company. Returns the identifier assigned to the new record.")]
    public Task<AddResponse> AddShipperAsync(
        [Description("The shipper to create. Required.")] ShipperDto shipper,
        [Description("Optional. Name of the Pilot API to call (see list_apis). Defaults to the currently selected API.")] string? apiName = null,
        CancellationToken cancellationToken = default)
        => client.PostJsonAsync<ShipperDto, AddResponse>($"/v{this.ApiVersion}/shippers/add", shipper, apiName, cancellationToken);

    [McpServerTool(Name = "update_shipper")]
    [Description("Updates an existing shipping company, identified by its shipperID.")]
    public Task UpdateShipperAsync(
        [Description("The shipper to update, including its existing shipperID. Required.")] ShipperDto shipper,
        [Description("Optional. Name of the Pilot API to call (see list_apis). Defaults to the currently selected API.")] string? apiName = null,
        CancellationToken cancellationToken = default)
        => client.PutJsonAsync($"/v{this.ApiVersion}/shippers/update", shipper, apiName, cancellationToken);

    [McpServerTool(Name = "delete_shipper")]
    [Description("Deletes a shipping company by its numeric ID.")]
    public Task DeleteShipperAsync(
        [Description("Numeric ID of the shipper to delete. Required.")] int shipperId,
        [Description("Optional. Name of the Pilot API to call (see list_apis). Defaults to the currently selected API.")] string? apiName = null,
        CancellationToken cancellationToken = default)
        => client.DeleteAsync($"/v{this.ApiVersion}/shippers/delete/{shipperId}", apiName, cancellationToken);
}
