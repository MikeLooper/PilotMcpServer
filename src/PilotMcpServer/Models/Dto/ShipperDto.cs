using System.ComponentModel;
using System.Text.Json.Serialization;

namespace PilotMcpServer.Models.Dto;

/// <summary>
/// A Northwind shipping company record.
/// </summary>
public sealed class ShipperDto
{
	/// <summary>
	/// Unique identifier of the shipper.
	/// </summary>
	[JsonPropertyName("shipperID")]
	[Description("Unique identifier of the shipper.")]
	public required int ShipperId { get; init; }

	/// <summary>
	/// Name of the shipping company.
	/// </summary>
	[JsonPropertyName("companyName")]
	[Description("Name of the shipping company.")]
	public required string CompanyName { get; init; }
	
	/// <summary>
	/// Phone number for the shipping company.
	/// </summary>
	[JsonPropertyName("phone")]
	[Description("Phone number for the shipping company.")]
	public string? Phone { get; init; }
}
