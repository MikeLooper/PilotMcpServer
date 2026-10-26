using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace PilotMcpServer.Models.Dto;

/// <summary>
/// A Northwind employee record.
/// </summary>
public sealed class EmployeeDto
{
	/// <summary>
	/// Unique identifier of the employee. Omit when adding a new employee; the API assigns it.
	/// </summary>
	[JsonPropertyName("employeeId")]
	[Description("Unique identifier of the employee. Omit when adding a new employee; the API assigns it.")]
	[Range(1, int.MaxValue)]
	public int? EmployeeId { get; init; }

	/// <summary>
	/// Employee's first name.
	/// </summary>
	[JsonPropertyName("firstName")]
	[Description("Employee's first name.")]
	[StringLength(10, MinimumLength = 1)]
	public required string FirstName { get; init; }

	/// <summary>
	/// Employee's last name.
	/// </summary>
	[JsonPropertyName("lastName")]
	[Description("Employee's last name.")]
	[StringLength(20, MinimumLength = 1)]
	public required string LastName { get; init; }

	/// <summary>
	/// Employee's job title.
	/// </summary>
	[JsonPropertyName("title")]
	[Description("Employee's job title.")]
	[StringLength(30)]
	public string? Title { get; init; }
	
	/// <summary>
	/// Courtesy title (e.g. Mr., Ms., Dr.).
	/// </summary>
	[JsonPropertyName("titleOfCourtesy")]
	[Description("Courtesy title (e.g. Mr., Ms., Dr.).")]
	[StringLength(25)]
	public string? TitleOfCourtesy { get; init; }
	
	/// <summary>
	/// Employee's date of birth, ISO 8601. Must be earlier than the current date. Valid range is 1753-01-01 through 9999-12-31.
	/// </summary>
	[JsonPropertyName("birthDate")]
	[Description("Employee's date of birth, ISO 8601. Must be earlier than the current date. Valid range is 1753-01-01 through 9999-12-31.")]
	public DateTime? BirthDate { get; init; }

	/// <summary>
	/// Date the employee was hired, ISO 8601. Valid range is 1753-01-01 through 9999-12-31.
	/// </summary>
	[JsonPropertyName("hireDate")]
	[Description("Date the employee was hired, ISO 8601. Valid range is 1753-01-01 through 9999-12-31.")]
	public DateTime? HireDate { get; init; }

	/// <summary>
	/// Street address of the employee.
	/// </summary>
	[JsonPropertyName("address")]
	[Description("Street address of the employee.")]
	[StringLength(60)]
	public string? Address { get; init; }
	
	/// <summary>
	/// City of the employee's address.
	/// </summary>
	[JsonPropertyName("city")]
	[Description("City of the employee's address.")]
	[StringLength(15)]
	public string? City { get; init; }
	
	/// <summary>
	/// State or region of the employee's address.
	/// </summary>
	[JsonPropertyName("region")]
	[Description("State or region of the employee's address.")]
	[StringLength(15)]
	public string? Region { get; init; }
	
	/// <summary>
	/// Postal or ZIP code of the employee's address.
	/// </summary>
	[JsonPropertyName("postalCode")]
	[Description("Postal or ZIP code of the employee's address.")]
	[StringLength(10)]
	public string? PostalCode { get; init; }

	/// <summary>
	/// Country of the employee's address.
	/// </summary>
	[JsonPropertyName("country")]
	[Description("Country of the employee's address.")]
	[StringLength(15)]
	public string? Country { get; init; }

	/// <summary>
	/// Employee's home phone number.
	/// </summary>
	[JsonPropertyName("homePhone")]
	[Description("Employee's home phone number.")]
	[StringLength(24)]
	public string? HomePhone { get; init; }

	/// <summary>
	/// Employee's internal phone extension.
	/// </summary>
	[JsonPropertyName("extension")]
	[Description("Employee's internal phone extension.")]
	[StringLength(4)]
	public string? Extension { get; init; }

	/// <summary>
	/// Optional photo of the employee, base64-encoded.
	/// </summary>
	[JsonPropertyName("photo")]
	[Description("Optional photo of the employee, base64-encoded.")]
	public byte[]? Photo { get; init; }

	/// <summary>
	/// URL or path to the employee's photo.
	/// </summary>
	[JsonPropertyName("photoPath")]
	[Description("URL or path to the employee's photo.")]
	[StringLength(255)]
	public string? PhotoPath { get; init; }

	/// <summary>
	/// Free-text notes about the employee.
	/// </summary>
	[JsonPropertyName("notes")]
	[Description("Free-text notes about the employee.")]
	public string? Notes { get; init; }
	
	/// <summary>
	/// Employee ID (employeeId) of this employee's manager, if any.
	/// </summary>
	[JsonPropertyName("reportsTo")]
	[Description("Employee ID (employeeId) of this employee's manager, if any.")]
	[Range(1, int.MaxValue)]
	public int? ReportsTo { get; init; }
}
