using System.ComponentModel;
using System.Text.Json.Serialization;

namespace PilotMcpServer.Models.Dto;

/// <summary>
/// A Northwind customer record.
/// </summary>
public sealed class CustomerDto
{
	/// <summary>
	/// Unique identifier of the customer (a 5-character code).
	/// </summary>
	[JsonPropertyName("customerID")]
	[Description("Unique identifier of the customer (a 5-character code).")]
	public required string CustomerId { get; init; }
	
	/// <summary>
	/// Name of the customer's company.
	/// </summary>
	[JsonPropertyName("companyName")]
	[Description("Name of the customer's company.")]
	public required string CompanyName { get; init; }
	
	/// <summary>
	/// Name of the primary contact at the company.
	/// </summary>
	[JsonPropertyName("contactName")]
	[Description("Name of the primary contact at the company.")]
	public string? ContactName { get; init; }
	
	/// <summary>
	/// Job title of the primary contact.
	/// </summary>
	[JsonPropertyName("contactTitle")]
	[Description("Job title of the primary contact.")]
	public string? ContactTitle { get; init; }
	
	/// <summary>
	/// Street address of the customer.
	/// </summary>
	[JsonPropertyName("address")]
	[Description("Street address of the customer.")]
	public string? Address { get; init; }
	
	/// <summary>
	/// City of the customer's address.
	/// </summary>
	[JsonPropertyName("city")]
	[Description("City of the customer's address.")]
	public string? City { get; init; }
	
	/// <summary>
	/// State or region of the customer's address.
	/// </summary>
	[JsonPropertyName("region")]
	[Description("State or region of the customer's address.")]
	public string? Region { get; init; }
	
	/// <summary>
	/// Postal or ZIP code of the customer's address.
	/// </summary>
	[JsonPropertyName("postalCode")]
	[Description("Postal or ZIP code of the customer's address.")]
	public string? PostalCode { get; init; }
	
	/// <summary>
	/// Country of the customer's address.
	/// </summary>
	[JsonPropertyName("country")]
	[Description("Country of the customer's address.")]
	public string? Country { get; init; }
	
	/// <summary>
	/// Primary phone number for the customer.
	/// </summary>
	[JsonPropertyName("phone")]
	[Description("Primary phone number for the customer.")]
	public string? Phone { get; init; }
	
	/// <summary>
	/// Fax number for the customer.
	/// </summary>
	[JsonPropertyName("fax")]
	[Description("Fax number for the customer.")]
	public string? Fax { get; init; }
}
