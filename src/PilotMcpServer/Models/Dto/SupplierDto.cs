using System.ComponentModel;
using System.Text.Json.Serialization;

namespace PilotMcpServer.Models.Dto;

/// <summary>
/// A Northwind supplier record.
/// </summary>
public sealed class SupplierDto
{
	/// <summary>
	/// Gets or sets the unique identifier of the supplier.
	/// </summary>
	[JsonPropertyName("supplierID")]
	[Description("Unique identifier of the supplier.")]
	public required int SupplierId { get; init; }

	/// <summary>
	/// Gets or sets the name of the supplier's company.
	/// </summary>
	[JsonPropertyName("companyName")]
	[Description("Name of the supplier's company.")]
	public required string CompanyName { get; init; }

	/// <summary>
	/// Gets or sets the name of the primary contact at the supplier.
	/// </summary>
	[JsonPropertyName("contactName")]
	[Description("Name of the primary contact at the supplier.")]
	public string? ContactName { get; init; }

	/// <summary>
	/// Gets or sets the job title of the primary contact at the supplier.
	/// </summary>
	[JsonPropertyName("contactTitle")]
	[Description("Job title of the primary contact.")]
	public string? ContactTitle { get; init; }

	/// <summary>
	/// Gets or sets the street address of the supplier.
	/// </summary>
	[JsonPropertyName("address")]
	[Description("Street address of the supplier.")]
	public string? Address { get; init; }

	/// <summary>
	/// Gets or sets the city of the supplier's address.
	/// </summary>
	[JsonPropertyName("city")]
	[Description("City of the supplier's address.")]
	public string? City { get; init; }

	/// <summary>
	/// Gets or sets the state or region of the supplier's address.
	/// </summary>
	[JsonPropertyName("region")]
	[Description("State or region of the supplier's address.")]
	public string? Region { get; init; }

	/// <summary>
	/// Gets or sets the postal or ZIP code of the supplier's address.
	/// </summary>
	[JsonPropertyName("postalCode")]
	[Description("Postal or ZIP code of the supplier's address.")]
	public string? PostalCode { get; init; }

	/// <summary>
	/// Gets or sets the country of the supplier's address.
	/// </summary>
	[JsonPropertyName("country")]
	[Description("Country of the supplier's address.")]
	public string? Country { get; init; }

	/// <summary>
	/// Gets or sets the phone number of the supplier.
	/// </summary>
	[JsonPropertyName("phone")]
	[Description("Phone number for the supplier.")]
	public string? Phone { get; init; }

	/// <summary>
	/// Gets or sets the fax number of the supplier.
	/// </summary>
	[JsonPropertyName("fax")]
	[Description("Fax number for the supplier.")]
	public string? Fax { get; init; }

	/// <summary>
	/// Gets or sets the URL of the supplier's home page.
	/// </summary>
	[JsonPropertyName("homePage")]
	[Description("URL of the supplier's home page.")]
	public string? HomePage { get; init; }
}
