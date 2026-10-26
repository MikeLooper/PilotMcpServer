using ModelContextProtocol.Server;
using PilotMcpServer.Models;
using PilotMcpServer.Models.Dto;
using PilotMcpServer.Services;
using PilotMcpServer.Tools.Base;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace PilotMcpServer.Tools;

[McpServerToolType]
public sealed class OrderDetailsTools(IPilotHttpClient client) : ToolBase
{
    [McpServerTool(Name = "get_all_order_details")]
    [Description("Retrieves a page of order lines (order details) from the selected Pilot API. The API returns the first page (page 0) of 20 records when page and pageSize are omitted.")]
    public Task<IReadOnlyList<OrderDetailDto>> GetAllOrderDetailsAsync(
        [Description("Optional. Zero-based number of the page to start with. Defaults to 0 when omitted.")] int? page = null,
        [Description("Optional. Number of records to return. Defaults to 20 when omitted.")] int? pageSize = null,
        [Description("Optional. Name of the Pilot API to call (see list_apis). Defaults to the currently selected API.")] string? apiName = null,
        CancellationToken cancellationToken = default)
        => client.GetJsonListAsync<OrderDetailDto>(WithPaging($"/v{this.ApiVersion}/order-details/get-all", page, pageSize), apiName, cancellationToken);

    [McpServerTool(Name = "get_order_detail")]
    [Description("Retrieves a single order line, identified by its product ID and order ID. Returns null if no matching order line exists.")]
    public Task<OrderDetailDto?> GetOrderDetailAsync(
        [Description("Numeric ID of the product on the order line. Required.")] [Range(1, int.MaxValue)] int productId,
        [Description("Numeric ID of the order the line belongs to. Required.")] [Range(1, int.MaxValue)] int orderId,
        [Description("Optional. Name of the Pilot API to call (see list_apis). Defaults to the currently selected API.")] string? apiName = null,
        CancellationToken cancellationToken = default)
        => client.GetJsonAsync<OrderDetailDto>($"/v{this.ApiVersion}/order-details/get/product/{productId}/order/{orderId}", apiName, cancellationToken);

    [McpServerTool(Name = "add_order_detail")]
    [Description("Creates a new order line. Returns the composite orderId and productId key of the new record.")]
    public Task<AddResponseOrderDetails> AddOrderDetailAsync(
        [Description("The order line to create. Required.")] OrderDetailDto orderDetail,
        [Description("Optional. Name of the Pilot API to call (see list_apis). Defaults to the currently selected API.")] string? apiName = null,
        CancellationToken cancellationToken = default)
        => client.PostJsonAsync<OrderDetailDto, AddResponseOrderDetails>($"/v{this.ApiVersion}/order-details/add", orderDetail, apiName, cancellationToken);

    [McpServerTool(Name = "update_order_detail")]
    [Description("Updates an existing order line, identified by its productId and orderId.")]
    public Task UpdateOrderDetailAsync(
        [Description("The order line to update, including its existing productId and orderId. Required.")] OrderDetailDto orderDetail,
        [Description("Optional. Name of the Pilot API to call (see list_apis). Defaults to the currently selected API.")] string? apiName = null,
        CancellationToken cancellationToken = default)
        => client.PutJsonAsync($"/v{this.ApiVersion}/order-details/update", orderDetail, apiName, cancellationToken);

    [McpServerTool(Name = "delete_order_detail")]
    [Description("Deletes an order line, identified by its product ID and order ID.")]
    public Task DeleteOrderDetailAsync(
        [Description("Numeric ID of the product on the order line. Required.")] [Range(1, int.MaxValue)] int productId,
        [Description("Numeric ID of the order the line belongs to. Required.")] [Range(1, int.MaxValue)] int orderId,
        [Description("Optional. Name of the Pilot API to call (see list_apis). Defaults to the currently selected API.")] string? apiName = null,
        CancellationToken cancellationToken = default)
        => client.DeleteAsync($"/v{this.ApiVersion}/order-details/delete/product/{productId}/order/{orderId}", apiName, cancellationToken);
}
