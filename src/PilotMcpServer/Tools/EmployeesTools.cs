using ModelContextProtocol.Server;
using PilotMcpServer.Models;
using PilotMcpServer.Models.Dto;
using PilotMcpServer.Services;
using PilotMcpServer.Tools.Base;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace PilotMcpServer.Tools;

[McpServerToolType]
public sealed class EmployeesTools(IPilotHttpClient client) : ToolBase
{
    [McpServerTool(Name = "get_all_employees")]
    [Description("Retrieves a page of employees from the selected Pilot API. The API returns the first page (page 0) of 20 records when page and pageSize are omitted.")]
    public Task<IReadOnlyList<EmployeeDto>> GetAllEmployeesAsync(
        [Description("Optional. Zero-based number of the page to start with. Defaults to 0 when omitted.")] int? page = null,
        [Description("Optional. Number of records to return. Defaults to 20 when omitted.")] int? pageSize = null,
        [Description("Optional. Name of the Pilot API to call (see list_apis). Defaults to the currently selected API.")] string? apiName = null,
        CancellationToken cancellationToken = default)
        => client.GetJsonListAsync<EmployeeDto>(WithPaging($"/v{this.ApiVersion}/employees/get-all", page, pageSize), apiName, cancellationToken);

    [McpServerTool(Name = "get_employee")]
    [Description("Retrieves a single employee by its numeric ID. Returns null if no employee with that ID exists.")]
    public Task<EmployeeDto?> GetEmployeeAsync(
        [Description("Numeric ID of the employee to retrieve. Required.")] [Range(1, int.MaxValue)] int employeeId,
        [Description("Optional. Name of the Pilot API to call (see list_apis). Defaults to the currently selected API.")] string? apiName = null,
        CancellationToken cancellationToken = default)
        => client.GetJsonAsync<EmployeeDto>($"/v{this.ApiVersion}/employees/get/{employeeId}", apiName, cancellationToken);

    [McpServerTool(Name = "add_employee")]
    [Description("Creates a new employee. Returns the identifier assigned to the new record.")]
    public Task<AddResponseInt> AddEmployeeAsync(
        [Description("The employee to create. Required.")] EmployeeDto employee,
        [Description("Optional. Name of the Pilot API to call (see list_apis). Defaults to the currently selected API.")] string? apiName = null,
        CancellationToken cancellationToken = default)
        => client.PostJsonAsync<EmployeeDto, AddResponseInt>($"/v{this.ApiVersion}/employees/add", employee, apiName, cancellationToken);

    [McpServerTool(Name = "update_employee")]
    [Description("Updates an existing employee, identified by its employeeId.")]
    public Task UpdateEmployeeAsync(
        [Description("The employee to update, including its existing employeeId. Required.")] EmployeeDto employee,
        [Description("Optional. Name of the Pilot API to call (see list_apis). Defaults to the currently selected API.")] string? apiName = null,
        CancellationToken cancellationToken = default)
        => client.PutJsonAsync($"/v{this.ApiVersion}/employees/update", employee, apiName, cancellationToken);

    [McpServerTool(Name = "delete_employee")]
    [Description("Deletes an employee by its numeric ID.")]
    public Task DeleteEmployeeAsync(
        [Description("Numeric ID of the employee to delete. Required.")] [Range(1, int.MaxValue)] int employeeId,
        [Description("Optional. Name of the Pilot API to call (see list_apis). Defaults to the currently selected API.")] string? apiName = null,
        CancellationToken cancellationToken = default)
        => client.DeleteAsync($"/v{this.ApiVersion}/employees/delete/{employeeId}", apiName, cancellationToken);
}
