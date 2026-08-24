using System.ComponentModel;
using System.Text.Json.Serialization;

namespace PilotMcpServer.Models;

/// <summary>A Northwind employee record.</summary>
public sealed class EmployeeDto
{
    [JsonPropertyName("employeeID")]
    [Description("Unique identifier of the employee.")]
    public required int EmployeeId { get; init; }

    [JsonPropertyName("firstName")]
    [Description("Employee's first name.")]
    public required string FirstName { get; init; }

    [JsonPropertyName("lastName")]
    [Description("Employee's last name.")]
    public required string LastName { get; init; }

    [JsonPropertyName("title")]
    [Description("Employee's job title.")]
    public string? Title { get; init; }

    [JsonPropertyName("titleOfCourtesy")]
    [Description("Courtesy title (e.g. Mr., Ms., Dr.).")]
    public string? TitleOfCourtesy { get; init; }

    [JsonPropertyName("birthDate")]
    [Description("Employee's date of birth, ISO 8601.")]
    public DateTime? BirthDate { get; init; }

    [JsonPropertyName("hireDate")]
    [Description("Date the employee was hired, ISO 8601.")]
    public DateTime? HireDate { get; init; }

    [JsonPropertyName("address")]
    [Description("Street address of the employee.")]
    public string? Address { get; init; }

    [JsonPropertyName("city")]
    [Description("City of the employee's address.")]
    public string? City { get; init; }

    [JsonPropertyName("region")]
    [Description("State or region of the employee's address.")]
    public string? Region { get; init; }

    [JsonPropertyName("postalCode")]
    [Description("Postal or ZIP code of the employee's address.")]
    public string? PostalCode { get; init; }

    [JsonPropertyName("country")]
    [Description("Country of the employee's address.")]
    public string? Country { get; init; }

    [JsonPropertyName("homePhone")]
    [Description("Employee's home phone number.")]
    public string? HomePhone { get; init; }

    [JsonPropertyName("extension")]
    [Description("Employee's internal phone extension.")]
    public string? Extension { get; init; }

    [JsonPropertyName("photo")]
    [Description("Optional photo of the employee, base64-encoded.")]
    public byte[]? Photo { get; init; }

    [JsonPropertyName("photoPath")]
    [Description("URL or path to the employee's photo.")]
    public string? PhotoPath { get; init; }

    [JsonPropertyName("notes")]
    [Description("Free-text notes about the employee.")]
    public string? Notes { get; init; }

    [JsonPropertyName("reportsTo")]
    [Description("Employee ID of this employee's manager, if any.")]
    public int? ReportsTo { get; init; }
}
