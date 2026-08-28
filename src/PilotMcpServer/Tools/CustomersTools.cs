using ModelContextProtocol.Server;
using PilotMcpServer.Models;
using PilotMcpServer.Models.Dto;
using PilotMcpServer.Services;
using PilotMcpServer.Tools.Base;
using System.ComponentModel;

namespace PilotMcpServer.Tools;

[McpServerToolType]
public sealed class CustomersTools(IPilotHttpClient client) : ToolBase
{
    [McpServerTool(Name = "get_all_customers")]
    [Description("Retrieves every customer from the selected Pilot API.")]
    public Task<IReadOnlyList<CustomerDto>> GetAllCustomersAsync(
        [Description("Optional. Name of the Pilot API to call (see list_apis). Defaults to the currently selected API.")] string? apiName = null,
        CancellationToken cancellationToken = default)
        => client.GetJsonListAsync<CustomerDto>($"/v{this.ApiVersion}/customers/get-all", apiName, cancellationToken);

    [McpServerTool(Name = "get_customer")]
    [Description("Retrieves a single customer by its 5-character customer code. Returns null if no customer with that code exists.")]
    public Task<CustomerDto?> GetCustomerAsync(
        [Description("5-character customer code of the customer to retrieve. Required.")] string customerId,
        [Description("Optional. Name of the Pilot API to call (see list_apis). Defaults to the currently selected API.")] string? apiName = null,
        CancellationToken cancellationToken = default)
        => client.GetJsonAsync<CustomerDto>($"/v{this.ApiVersion}/customers/get/{Uri.EscapeDataString(customerId)}", apiName, cancellationToken);

    [McpServerTool(Name = "add_customer")]
    [Description("Creates a new customer. Returns the identifier assigned to the new record.")]
    public Task<AddResponse> AddCustomerAsync(
        [Description("The customer to create. Required.")] CustomerDto customer,
        [Description("Optional. Name of the Pilot API to call (see list_apis). Defaults to the currently selected API.")] string? apiName = null,
        CancellationToken cancellationToken = default)
        => client.PostJsonAsync<CustomerDto, AddResponse>($"/v{this.ApiVersion}/customers/add", customer, apiName, cancellationToken);

    [McpServerTool(Name = "update_customer")]
    [Description("Updates an existing customer, identified by its customerID.")]
    public Task UpdateCustomerAsync(
        [Description("The customer to update, including its existing customerID. Required.")] CustomerDto customer,
        [Description("Optional. Name of the Pilot API to call (see list_apis). Defaults to the currently selected API.")] string? apiName = null,
        CancellationToken cancellationToken = default)
        => client.PutJsonAsync($"/v{this.ApiVersion}/customers/update", customer, apiName, cancellationToken);

    [McpServerTool(Name = "delete_customer")]
    [Description("Deletes a customer by its 5-character customer code.")]
    public Task DeleteCustomerAsync(
        [Description("5-character customer code of the customer to delete. Required.")] string customerId,
        [Description("Optional. Name of the Pilot API to call (see list_apis). Defaults to the currently selected API.")] string? apiName = null,
        CancellationToken cancellationToken = default)
        => client.DeleteAsync($"/v{this.ApiVersion}/customers/delete/{Uri.EscapeDataString(customerId)}", apiName, cancellationToken);
}
