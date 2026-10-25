using System.ComponentModel;
using System.Text.Json.Serialization;

namespace PilotMcpServer.Models.Dto;

/// <summary>
/// A Northwind shipping company record.
/// </summary>
public sealed class ShipperDto
{
	/// <summary>
	/// Unique identifier of the shipper. Omit when adding a new shipper; the API assigns it.
	/// </summary>
	[JsonPropertyName("shipperId")]
	[Description("Unique identifier of the shipper. Omit when adding a new shipper; the API assigns it.")]
	public int? ShipperId { get; init; }

	/// <summary>
	/// Name of the shipping company.
	/// </summary>
	[JsonPropertyName("companyName")]
	[Description("Name of the shipping company.")]
	public string? CompanyName { get; init; }
	
	/// <summary>
	/// Phone number for the shipping company.
	/// </summary>
	[JsonPropertyName("phone")]
	[Description("Phone number for the shipping company.")]
	public string? Phone { get; init; }
}
