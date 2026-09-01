using System.ComponentModel;
using System.Text.Json.Serialization;

namespace PilotMcpServer.Models.Dto;

/// <summary>
/// A Northwind order record.
/// </summary>
public sealed class OrderDto
{
	/// <summary>
	/// Unique identifier of the order.
	/// </summary>
	[JsonPropertyName("orderID")]
	[Description("Unique identifier of the order.")]
	public required int OrderId { get; init; }
	
	/// <summary>
	/// Identifier of the customer who placed the order.
	/// </summary>
	[JsonPropertyName("customerID")]
	[Description("Identifier of the customer who placed the order.")]
	public string? CustomerId { get; init; }

	/// <summary>
	/// Identifier of the employee who handled the order.
	/// </summary>
	[JsonPropertyName("employeeID")]
	[Description("Identifier of the employee who handled the order.")]
	public int? EmployeeId { get; init; }

	/// <summary>
	/// Date the order was placed, ISO 8601.
	/// </summary>
	[JsonPropertyName("orderDate")]
	[Description("Date the order was placed, ISO 8601.")]
	public DateTime? OrderDate { get; init; }

	/// <summary>
	/// Date the order is required by, ISO 8601.
	/// </summary>
	[JsonPropertyName("requiredDate")]
	[Description("Date the order is required by, ISO 8601.")]
	public DateTime? RequiredDate { get; init; }

	/// <summary>
	/// Date the order was shipped, ISO 8601.
	/// </summary>
	[JsonPropertyName("shippedDate")]
	[Description("Date the order was shipped, ISO 8601.")]
	public DateTime? ShippedDate { get; init; }

	/// <summary>
	/// Identifier of the shipper used for the order.
	/// </summary>
	[JsonPropertyName("shipVia")]
	[Description("Identifier of the shipper used for the order.")]
	public int? ShipVia { get; init; }

	/// <summary>
	/// Freight cost charged for the order.
	/// </summary>
	[JsonPropertyName("freight")]
	[Description("Freight cost charged for the order.")]
	public double? Freight { get; init; }

	/// <summary>
	/// Name of the person/company the order is shipped to.
	/// </summary>
	[JsonPropertyName("shipName")]
	[Description("Name of the person/company the order is shipped to.")]
	public string? ShipName { get; init; }

	/// <summary>
	/// Street address the order is shipped to.
	/// </summary>
	[JsonPropertyName("shipAddress")]
	[Description("Street address the order is shipped to.")]
	public string? ShipAddress { get; init; }
	
	/// <summary>
	/// City the order is shipped to.
	/// </summary>
	[JsonPropertyName("shipCity")]
	[Description("City the order is shipped to.")]
	public string? ShipCity { get; init; }
	
	/// <summary>
	/// State or region the order is shipped to.
	/// </summary>
	[JsonPropertyName("shipRegion")]
	[Description("State or region the order is shipped to.")]
	public string? ShipRegion { get; init; }
	
	/// <summary>
	/// Postal or ZIP code the order is shipped to.
	/// </summary>
	[JsonPropertyName("shipPostalCode")]
	[Description("Postal or ZIP code the order is shipped to.")]
	public string? ShipPostalCode { get; init; }
	
	/// <summary>
	/// Country the order is shipped to.
	/// </summary>
	[JsonPropertyName("shipCountry")]
	[Description("Country the order is shipped to.")]
	public string? ShipCountry { get; init; }
}
