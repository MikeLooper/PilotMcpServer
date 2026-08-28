using ModelContextProtocol.Server;
using PilotMcpServer.Models;
using PilotMcpServer.Models.Dto;
using PilotMcpServer.Services;
using PilotMcpServer.Tools.Base;
using System.ComponentModel;

namespace PilotMcpServer.Tools;

[McpServerToolType]
public sealed class EmployeesTools(IPilotHttpClient client) : ToolBase
{
    [McpServerTool(Name = "get_all_employees")]
    [Description("Retrieves every employee from the selected Pilot API.")]
    public Task<IReadOnlyList<EmployeeDto>> GetAllEmployeesAsync(
        [Description("Optional. Name of the Pilot API to call (see list_apis). Defaults to the currently selected API.")] string? apiName = null,
        CancellationToken cancellationToken = default)
        => client.GetJsonListAsync<EmployeeDto>($"/v{this.ApiVersion}/employees/get-all", apiName, cancellationToken);

    [McpServerTool(Name = "get_employee")]
    [Description("Retrieves a single employee by its numeric ID. Returns null if no employee with that ID exists.")]
    public Task<EmployeeDto?> GetEmployeeAsync(
        [Description("Numeric ID of the employee to retrieve. Required.")] int employeeId,
        [Description("Optional. Name of the Pilot API to call (see list_apis). Defaults to the currently selected API.")] string? apiName = null,
        CancellationToken cancellationToken = default)
        => client.GetJsonAsync<EmployeeDto>($"/v{this.ApiVersion}/employees/get/{employeeId}", apiName, cancellationToken);

    [McpServerTool(Name = "add_employee")]
    [Description("Creates a new employee. Returns the identifier assigned to the new record.")]
    public Task<AddResponse> AddEmployeeAsync(
        [Description("The employee to create. Required.")] EmployeeDto employee,
        [Description("Optional. Name of the Pilot API to call (see list_apis). Defaults to the currently selected API.")] string? apiName = null,
        CancellationToken cancellationToken = default)
        => client.PostJsonAsync<EmployeeDto, AddResponse>($"/v{this.ApiVersion}/employees/add", employee, apiName, cancellationToken);

    [McpServerTool(Name = "update_employee")]
    [Description("Updates an existing employee, identified by its employeeID.")]
    public Task UpdateEmployeeAsync(
        [Description("The employee to update, including its existing employeeID. Required.")] EmployeeDto employee,
        [Description("Optional. Name of the Pilot API to call (see list_apis). Defaults to the currently selected API.")] string? apiName = null,
        CancellationToken cancellationToken = default)
        => client.PutJsonAsync($"/v{this.ApiVersion}/employees/update", employee, apiName, cancellationToken);

    [McpServerTool(Name = "delete_employee")]
    [Description("Deletes an employee by its numeric ID.")]
    public Task DeleteEmployeeAsync(
        [Description("Numeric ID of the employee to delete. Required.")] int employeeId,
        [Description("Optional. Name of the Pilot API to call (see list_apis). Defaults to the currently selected API.")] string? apiName = null,
        CancellationToken cancellationToken = default)
        => client.DeleteAsync($"/v{this.ApiVersion}/employees/delete/{employeeId}", apiName, cancellationToken);
}
