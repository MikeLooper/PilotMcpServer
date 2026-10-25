using ModelContextProtocol.Server;
using PilotMcpServer.Models;
using PilotMcpServer.Models.Dto;
using PilotMcpServer.Services;
using PilotMcpServer.Tools.Base;
using System.ComponentModel;

namespace PilotMcpServer.Tools;

[McpServerToolType]
public sealed class OrdersTools(IPilotHttpClient client) : ToolBase
{
    [McpServerTool(Name = "get_all_orders")]
    [Description("Retrieves a page of orders from the selected Pilot API. The API returns the first page (page 0) of 20 records when page and pageSize are omitted.")]
    public Task<IReadOnlyList<OrderDto>> GetAllOrdersAsync(
        [Description("Optional. Zero-based number of the page to start with. Defaults to 0 when omitted.")] int? page = null,
        [Description("Optional. Number of records to return. Defaults to 20 when omitted.")] int? pageSize = null,
        [Description("Optional. Name of the Pilot API to call (see list_apis). Defaults to the currently selected API.")] string? apiName = null,
        CancellationToken cancellationToken = default)
        => client.GetJsonListAsync<OrderDto>(WithPaging($"/v{this.ApiVersion}/orders/get-all", page, pageSize), apiName, cancellationToken);

    [McpServerTool(Name = "get_order")]
    [Description("Retrieves a single order by its numeric ID. Returns null if no order with that ID exists.")]
    public Task<OrderDto?> GetOrderAsync(
        [Description("Numeric ID of the order to retrieve. Required.")] int orderId,
        [Description("Optional. Name of the Pilot API to call (see list_apis). Defaults to the currently selected API.")] string? apiName = null,
        CancellationToken cancellationToken = default)
        => client.GetJsonAsync<OrderDto>($"/v{this.ApiVersion}/orders/get/{orderId}", apiName, cancellationToken);

    [McpServerTool(Name = "add_order")]
    [Description("Creates a new order. Returns the identifier assigned to the new record.")]
    public Task<AddResponseInt> AddOrderAsync(
        [Description("The order to create. Required.")] OrderDto order,
        [Description("Optional. Name of the Pilot API to call (see list_apis). Defaults to the currently selected API.")] string? apiName = null,
        CancellationToken cancellationToken = default)
        => client.PostJsonAsync<OrderDto, AddResponseInt>($"/v{this.ApiVersion}/orders/add", order, apiName, cancellationToken);

    [McpServerTool(Name = "update_order")]
    [Description("Updates an existing order, identified by its orderId.")]
    public Task UpdateOrderAsync(
        [Description("The order to update, including its existing orderId. Required.")] OrderDto order,
        [Description("Optional. Name of the Pilot API to call (see list_apis). Defaults to the currently selected API.")] string? apiName = null,
        CancellationToken cancellationToken = default)
        => client.PutJsonAsync($"/v{this.ApiVersion}/orders/update", order, apiName, cancellationToken);

    [McpServerTool(Name = "delete_order")]
    [Description("Deletes an order by its numeric ID.")]
    public Task DeleteOrderAsync(
        [Description("Numeric ID of the order to delete. Required.")] int orderId,
        [Description("Optional. Name of the Pilot API to call (see list_apis). Defaults to the currently selected API.")] string? apiName = null,
        CancellationToken cancellationToken = default)
        => client.DeleteAsync($"/v{this.ApiVersion}/orders/delete/{orderId}", apiName, cancellationToken);
}
