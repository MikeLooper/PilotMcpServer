using System.ComponentModel;
using ModelContextProtocol.Server;
using PilotMcpServer.Models;
using PilotMcpServer.Services;

namespace PilotMcpServer.Tools;

[McpServerToolType]
public sealed class ProductsTools(IPilotHttpClient client)
{
    [McpServerTool(Name = "get_all_products")]
    [Description("Retrieves every product from the selected Pilot API.")]
    public Task<IReadOnlyList<ProductDto>> GetAllProductsAsync(
        [Description("Optional. Name of the Pilot API to call (see list_apis). Defaults to the currently selected API.")] string? apiName = null,
        CancellationToken cancellationToken = default)
        => client.GetJsonListAsync<ProductDto>("/products/get-all", apiName, cancellationToken);

    [McpServerTool(Name = "get_product")]
    [Description("Retrieves a single product by its numeric ID. Returns null if no product with that ID exists.")]
    public Task<ProductDto?> GetProductAsync(
        [Description("Numeric ID of the product to retrieve. Required.")] int productId,
        [Description("Optional. Name of the Pilot API to call (see list_apis). Defaults to the currently selected API.")] string? apiName = null,
        CancellationToken cancellationToken = default)
        => client.GetJsonAsync<ProductDto>($"/products/get/{productId}", apiName, cancellationToken);

    [McpServerTool(Name = "add_product")]
    [Description("Creates a new product. Returns the identifier assigned to the new record.")]
    public Task<AddResponse> AddProductAsync(
        [Description("The product to create. Required.")] ProductDto product,
        [Description("Optional. Name of the Pilot API to call (see list_apis). Defaults to the currently selected API.")] string? apiName = null,
        CancellationToken cancellationToken = default)
        => client.PostJsonAsync<ProductDto, AddResponse>("/products/add", product, apiName, cancellationToken);

    [McpServerTool(Name = "update_product")]
    [Description("Updates an existing product, identified by its productID.")]
    public Task UpdateProductAsync(
        [Description("The product to update, including its existing productID. Required.")] ProductDto product,
        [Description("Optional. Name of the Pilot API to call (see list_apis). Defaults to the currently selected API.")] string? apiName = null,
        CancellationToken cancellationToken = default)
        => client.PutJsonAsync("/products/update", product, apiName, cancellationToken);

    [McpServerTool(Name = "delete_product")]
    [Description("Deletes a product by its numeric ID.")]
    public Task DeleteProductAsync(
        [Description("Numeric ID of the product to delete. Required.")] int productId,
        [Description("Optional. Name of the Pilot API to call (see list_apis). Defaults to the currently selected API.")] string? apiName = null,
        CancellationToken cancellationToken = default)
        => client.DeleteAsync($"/products/delete/{productId}", apiName, cancellationToken);
}
