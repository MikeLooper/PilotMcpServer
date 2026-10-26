using ModelContextProtocol.Server;
using PilotMcpServer.Models;
using PilotMcpServer.Models.Dto;
using PilotMcpServer.Services;
using PilotMcpServer.Tools.Base;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace PilotMcpServer.Tools;

[McpServerToolType]
public sealed class ShippersTools(IPilotHttpClient client) : ToolBase
{
    [McpServerTool(Name = "get_all_shippers")]
    [Description("Retrieves a page of shipping companies from the selected Pilot API. The API returns the first page (page 0) of 20 records when page and pageSize are omitted.")]
    public Task<IReadOnlyList<ShipperDto>> GetAllShippersAsync(
        [Description("Optional. Zero-based number of the page to start with. Defaults to 0 when omitted.")] int? page = null,
        [Description("Optional. Number of records to return. Defaults to 20 when omitted.")] int? pageSize = null,
        [Description("Optional. Name of the Pilot API to call (see list_apis). Defaults to the currently selected API.")] string? apiName = null,
        CancellationToken cancellationToken = default)
        => client.GetJsonListAsync<ShipperDto>(WithPaging($"/v{this.ApiVersion}/shippers/get-all", page, pageSize), apiName, cancellationToken);

    [McpServerTool(Name = "get_shipper")]
    [Description("Retrieves a single shipping company by its numeric ID. Returns null if no shipper with that ID exists.")]
    public Task<ShipperDto?> GetShipperAsync(
        [Description("Numeric ID of the shipper to retrieve. Required.")] [Range(1, int.MaxValue)] int shipperId,
        [Description("Optional. Name of the Pilot API to call (see list_apis). Defaults to the currently selected API.")] string? apiName = null,
        CancellationToken cancellationToken = default)
        => client.GetJsonAsync<ShipperDto>($"/v{this.ApiVersion}/shippers/get/{shipperId}", apiName, cancellationToken);

    [McpServerTool(Name = "add_shipper")]
    [Description("Creates a new shipping company. Returns the identifier assigned to the new record.")]
    public Task<AddResponseInt> AddShipperAsync(
        [Description("The shipper to create. Required.")] ShipperDto shipper,
        [Description("Optional. Name of the Pilot API to call (see list_apis). Defaults to the currently selected API.")] string? apiName = null,
        CancellationToken cancellationToken = default)
        => client.PostJsonAsync<ShipperDto, AddResponseInt>($"/v{this.ApiVersion}/shippers/add", shipper, apiName, cancellationToken);

    [McpServerTool(Name = "update_shipper")]
    [Description("Updates an existing shipping company, identified by its shipperId.")]
    public Task UpdateShipperAsync(
        [Description("The shipper to update, including its existing shipperId. Required.")] ShipperDto shipper,
        [Description("Optional. Name of the Pilot API to call (see list_apis). Defaults to the currently selected API.")] string? apiName = null,
        CancellationToken cancellationToken = default)
        => client.PutJsonAsync($"/v{this.ApiVersion}/shippers/update", shipper, apiName, cancellationToken);

    [McpServerTool(Name = "delete_shipper")]
    [Description("Deletes a shipping company by its numeric ID.")]
    public Task DeleteShipperAsync(
        [Description("Numeric ID of the shipper to delete. Required.")] [Range(1, int.MaxValue)] int shipperId,
        [Description("Optional. Name of the Pilot API to call (see list_apis). Defaults to the currently selected API.")] string? apiName = null,
        CancellationToken cancellationToken = default)
        => client.DeleteAsync($"/v{this.ApiVersion}/shippers/delete/{shipperId}", apiName, cancellationToken);
}
