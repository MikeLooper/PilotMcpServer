using ModelContextProtocol.Server;
using PilotMcpServer.Models;
using PilotMcpServer.Models.Dto;
using PilotMcpServer.Services;
using PilotMcpServer.Tools.Base;
using System.ComponentModel;

namespace PilotMcpServer.Tools;

[McpServerToolType]
public sealed class CategoriesTools(IPilotHttpClient client) : ToolBase
{
    [McpServerTool(Name = "get_all_categories")]
    [Description("Retrieves every product category from the selected Pilot API.")]
    public Task<IReadOnlyList<CategoryDto>> GetAllCategoriesAsync(
        [Description("Optional. Name of the Pilot API to call (see list_apis). Defaults to the currently selected API.")] string? apiName = null,
        CancellationToken cancellationToken = default)
        => client.GetJsonListAsync<CategoryDto>($"/v{this.ApiVersion}/categories/get-all", apiName, cancellationToken);

    [McpServerTool(Name = "get_category")]
    [Description("Retrieves a single product category by its numeric ID. Returns null if no category with that ID exists.")]
    public Task<CategoryDto?> GetCategoryAsync(
        [Description("Numeric ID of the category to retrieve. Required.")] int categoryId,
        [Description("Optional. Name of the Pilot API to call (see list_apis). Defaults to the currently selected API.")] string? apiName = null,
        CancellationToken cancellationToken = default)
        => client.GetJsonAsync<CategoryDto>($"/v{this.ApiVersion}/categories/get/{categoryId}", apiName, cancellationToken);

    [McpServerTool(Name = "add_category")]
    [Description("Creates a new product category. Returns the identifier assigned to the new record.")]
    public Task<AddResponse> AddCategoryAsync(
        [Description("The category to create. Required.")] CategoryDto category,
        [Description("Optional. Name of the Pilot API to call (see list_apis). Defaults to the currently selected API.")] string? apiName = null,
        CancellationToken cancellationToken = default)
        => client.PostJsonAsync<CategoryDto, AddResponse>($"/v{this.ApiVersion}/categories/add", category, apiName, cancellationToken);

    [McpServerTool(Name = "update_category")]
    [Description("Updates an existing product category, identified by its categoryID.")]
    public Task UpdateCategoryAsync(
        [Description("The category to update, including its existing categoryID. Required.")] CategoryDto category,
        [Description("Optional. Name of the Pilot API to call (see list_apis). Defaults to the currently selected API.")] string? apiName = null,
        CancellationToken cancellationToken = default)
        => client.PutJsonAsync($"/v{this.ApiVersion}/categories/update", category, apiName, cancellationToken);

    [McpServerTool(Name = "delete_category")]
    [Description("Deletes a product category by its numeric ID.")]
    public Task DeleteCategoryAsync(
        [Description("Numeric ID of the category to delete. Required.")] int categoryId,
        [Description("Optional. Name of the Pilot API to call (see list_apis). Defaults to the currently selected API.")] string? apiName = null,
        CancellationToken cancellationToken = default)
        => client.DeleteAsync($"/v{this.ApiVersion}/categories/delete/{categoryId}", apiName, cancellationToken);
}
