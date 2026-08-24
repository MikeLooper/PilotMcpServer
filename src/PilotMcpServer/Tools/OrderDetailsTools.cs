using System.ComponentModel;
using ModelContextProtocol.Server;
using PilotMcpServer.Models;
using PilotMcpServer.Services;

namespace PilotMcpServer.Tools;

[McpServerToolType]
public sealed class OrderDetailsTools(IPilotHttpClient client)
{
    [McpServerTool(Name = "get_all_order_details")]
    [Description("Retrieves every order line (order detail) from the selected Pilot API.")]
    public Task<IReadOnlyList<OrderDetailDto>> GetAllOrderDetailsAsync(
        [Description("Optional. Name of the Pilot API to call (see list_apis). Defaults to the currently selected API.")] string? apiName = null,
        CancellationToken cancellationToken = default)
        => client.GetJsonListAsync<OrderDetailDto>("/order-details/get-all", apiName, cancellationToken);

    [McpServerTool(Name = "get_order_detail")]
    [Description("Retrieves a single order line, identified by its product ID and order ID. Returns null if no matching order line exists.")]
    public Task<OrderDetailDto?> GetOrderDetailAsync(
        [Description("Numeric ID of the product on the order line. Required.")] int productId,
        [Description("Numeric ID of the order the line belongs to. Required.")] int orderId,
        [Description("Optional. Name of the Pilot API to call (see list_apis). Defaults to the currently selected API.")] string? apiName = null,
        CancellationToken cancellationToken = default)
        => client.GetJsonAsync<OrderDetailDto>($"/order-details/get/product/{productId}/order/{orderId}", apiName, cancellationToken);

    [McpServerTool(Name = "add_order_detail")]
    [Description("Creates a new order line. Returns the identifier assigned to the new record.")]
    public Task<AddResponse> AddOrderDetailAsync(
        [Description("The order line to create. Required.")] OrderDetailDto orderDetail,
        [Description("Optional. Name of the Pilot API to call (see list_apis). Defaults to the currently selected API.")] string? apiName = null,
        CancellationToken cancellationToken = default)
        => client.PostJsonAsync<OrderDetailDto, AddResponse>("/order-details/add", orderDetail, apiName, cancellationToken);

    [McpServerTool(Name = "update_order_detail")]
    [Description("Updates an existing order line, identified by its productID and orderID.")]
    public Task UpdateOrderDetailAsync(
        [Description("The order line to update, including its existing productID and orderID. Required.")] OrderDetailDto orderDetail,
        [Description("Optional. Name of the Pilot API to call (see list_apis). Defaults to the currently selected API.")] string? apiName = null,
        CancellationToken cancellationToken = default)
        => client.PutJsonAsync("/order-details/update", orderDetail, apiName, cancellationToken);

    [McpServerTool(Name = "delete_order_detail")]
    [Description("Deletes an order line, identified by its product ID and order ID.")]
    public Task DeleteOrderDetailAsync(
        [Description("Numeric ID of the product on the order line. Required.")] int productId,
        [Description("Numeric ID of the order the line belongs to. Required.")] int orderId,
        [Description("Optional. Name of the Pilot API to call (see list_apis). Defaults to the currently selected API.")] string? apiName = null,
        CancellationToken cancellationToken = default)
        => client.DeleteAsync($"/order-details/delete/product/{productId}/order/{orderId}", apiName, cancellationToken);
}
