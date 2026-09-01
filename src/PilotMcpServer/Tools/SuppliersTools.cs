using ModelContextProtocol.Server;
using PilotMcpServer.Models;
using PilotMcpServer.Models.Dto;
using PilotMcpServer.Services;
using PilotMcpServer.Tools.Base;
using System.ComponentModel;

namespace PilotMcpServer.Tools;

[McpServerToolType]
public sealed class SuppliersTools(IPilotHttpClient client) : ToolBase
{
    [McpServerTool(Name = "get_all_suppliers")]
    [Description("Retrieves every supplier from the selected Pilot API.")]
    public Task<IReadOnlyList<SupplierDto>> GetAllSuppliersAsync(
        [Description("Optional. Name of the Pilot API to call (see list_apis). Defaults to the currently selected API.")] string? apiName = null,
        CancellationToken cancellationToken = default)
        => client.GetJsonListAsync<SupplierDto>($"/v{this.ApiVersion}/suppliers/get-all", apiName, cancellationToken);

    [McpServerTool(Name = "get_supplier")]
    [Description("Retrieves a single supplier by its numeric ID. Returns null if no supplier with that ID exists.")]
    public Task<SupplierDto?> GetSupplierAsync(
        [Description("Numeric ID of the supplier to retrieve. Required.")] int supplierId,
        [Description("Optional. Name of the Pilot API to call (see list_apis). Defaults to the currently selected API.")] string? apiName = null,
        CancellationToken cancellationToken = default)
        => client.GetJsonAsync<SupplierDto>($"/v{this.ApiVersion}/suppliers/get/{supplierId}", apiName, cancellationToken);

    [McpServerTool(Name = "add_supplier")]
    [Description("Creates a new supplier. Returns the identifier assigned to the new record.")]
    public Task<AddResponse> AddSupplierAsync(
        [Description("The supplier to create. Required.")] SupplierDto supplier,
        [Description("Optional. Name of the Pilot API to call (see list_apis). Defaults to the currently selected API.")] string? apiName = null,
        CancellationToken cancellationToken = default)
        => client.PostJsonAsync<SupplierDto, AddResponse>($"/v{this.ApiVersion}/suppliers/add", supplier, apiName, cancellationToken);

    [McpServerTool(Name = "update_supplier")]
    [Description("Updates an existing supplier, identified by its supplierID.")]
    public Task UpdateSupplierAsync(
        [Description("The supplier to update, including its existing supplierID. Required.")] SupplierDto supplier,
        [Description("Optional. Name of the Pilot API to call (see list_apis). Defaults to the currently selected API.")] string? apiName = null,
        CancellationToken cancellationToken = default)
        => client.PutJsonAsync($"/v{this.ApiVersion}/suppliers/update", supplier, apiName, cancellationToken);

    [McpServerTool(Name = "delete_supplier")]
    [Description("Deletes a supplier by its numeric ID.")]
    public Task DeleteSupplierAsync(
        [Description("Numeric ID of the supplier to delete. Required.")] int supplierId,
        [Description("Optional. Name of the Pilot API to call (see list_apis). Defaults to the currently selected API.")] string? apiName = null,
        CancellationToken cancellationToken = default)
        => client.DeleteAsync($"/v{this.ApiVersion}/suppliers/delete/{supplierId}", apiName, cancellationToken);
}
