using System.ComponentModel;
using ModelContextProtocol.Server;
using PilotMcpServer.Models;
using PilotMcpServer.Services;

namespace PilotMcpServer.Tools;

[McpServerToolType]
public sealed class OrdersTools(IPilotHttpClient client)
{
    [McpServerTool(Name = "get_all_orders")]
    [Description("Retrieves every order from the selected Pilot API.")]
    public Task<IReadOnlyList<OrderDto>> GetAllOrdersAsync(
        [Description("Optional. Name of the Pilot API to call (see list_apis). Defaults to the currently selected API.")] string? apiName = null,
        CancellationToken cancellationToken = default)
        => client.GetJsonListAsync<OrderDto>("/orders/get-all", apiName, cancellationToken);

    [McpServerTool(Name = "get_order")]
    [Description("Retrieves a single order by its numeric ID. Returns null if no order with that ID exists.")]
    public Task<OrderDto?> GetOrderAsync(
        [Description("Numeric ID of the order to retrieve. Required.")] int orderId,
        [Description("Optional. Name of the Pilot API to call (see list_apis). Defaults to the currently selected API.")] string? apiName = null,
        CancellationToken cancellationToken = default)
        => client.GetJsonAsync<OrderDto>($"/orders/get/{orderId}", apiName, cancellationToken);

    [McpServerTool(Name = "add_order")]
    [Description("Creates a new order. Returns the identifier assigned to the new record.")]
    public Task<AddResponse> AddOrderAsync(
        [Description("The order to create. Required.")] OrderDto order,
        [Description("Optional. Name of the Pilot API to call (see list_apis). Defaults to the currently selected API.")] string? apiName = null,
        CancellationToken cancellationToken = default)
        => client.PostJsonAsync<OrderDto, AddResponse>("/orders/add", order, apiName, cancellationToken);

    [McpServerTool(Name = "update_order")]
    [Description("Updates an existing order, identified by its orderID.")]
    public Task UpdateOrderAsync(
        [Description("The order to update, including its existing orderID. Required.")] OrderDto order,
        [Description("Optional. Name of the Pilot API to call (see list_apis). Defaults to the currently selected API.")] string? apiName = null,
        CancellationToken cancellationToken = default)
        => client.PutJsonAsync("/orders/update", order, apiName, cancellationToken);

    [McpServerTool(Name = "delete_order")]
    [Description("Deletes an order by its numeric ID.")]
    public Task DeleteOrderAsync(
        [Description("Numeric ID of the order to delete. Required.")] int orderId,
        [Description("Optional. Name of the Pilot API to call (see list_apis). Defaults to the currently selected API.")] string? apiName = null,
        CancellationToken cancellationToken = default)
        => client.DeleteAsync($"/orders/delete/{orderId}", apiName, cancellationToken);
}
