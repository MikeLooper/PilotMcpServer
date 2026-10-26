using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace PilotMcpServer.Models.Dto;

/// <summary>
/// A Northwind order record.
/// </summary>
public sealed class OrderDto
{
	/// <summary>
	/// Unique identifier of the order. Omit when adding a new order; the API assigns it.
	/// </summary>
	[JsonPropertyName("orderId")]
	[Description("Unique identifier of the order. Omit when adding a new order; the API assigns it.")]
	[Range(1, int.MaxValue)]
	public int? OrderId { get; init; }
	
	/// <summary>
	/// Identifier of the customer who placed the order.
	/// </summary>
	[JsonPropertyName("customerId")]
	[Description("Identifier of the customer who placed the order.")]
	[StringLength(5, MinimumLength = 5)]
	public string? CustomerId { get; init; }

	/// <summary>
	/// Identifier of the employee who handled the order.
	/// </summary>
	[JsonPropertyName("employeeId")]
	[Description("Identifier of the employee who handled the order.")]
	[Range(1, int.MaxValue)]
	public int? EmployeeId { get; init; }

	/// <summary>
	/// Date the order was placed, ISO 8601. Valid range is 1753-01-01 through 9999-12-31.
	/// </summary>
	[JsonPropertyName("orderDate")]
	[Description("Date the order was placed, ISO 8601. Valid range is 1753-01-01 through 9999-12-31.")]
	public DateTime? OrderDate { get; init; }

	/// <summary>
	/// Date the order is required by, ISO 8601. Valid range is 1753-01-01 through 9999-12-31.
	/// </summary>
	[JsonPropertyName("requiredDate")]
	[Description("Date the order is required by, ISO 8601. Valid range is 1753-01-01 through 9999-12-31.")]
	public DateTime? RequiredDate { get; init; }

	/// <summary>
	/// Date the order was shipped, ISO 8601. Valid range is 1753-01-01 through 9999-12-31.
	/// </summary>
	[JsonPropertyName("shippedDate")]
	[Description("Date the order was shipped, ISO 8601. Valid range is 1753-01-01 through 9999-12-31.")]
	public DateTime? ShippedDate { get; init; }

	/// <summary>
	/// Identifier (shipperId) of the shipper used for the order.
	/// </summary>
	[JsonPropertyName("shipVia")]
	[Description("Identifier (shipperId) of the shipper used for the order.")]
	[Range(1, int.MaxValue)]
	public int? ShipVia { get; init; }

	/// <summary>
	/// Freight cost charged for the order. Defaults to 0 when omitted.
	/// </summary>
	[JsonPropertyName("freight")]
	[Description("Freight cost charged for the order. Defaults to 0 when omitted.")]
	[Range(-922337203685477.5808d, 922337203685477.5807d)]
	public double? Freight { get; init; }

	/// <summary>
	/// Name of the person/company the order is shipped to.
	/// </summary>
	[JsonPropertyName("shipName")]
	[Description("Name of the person/company the order is shipped to.")]
	[StringLength(40)]
	public string? ShipName { get; init; }

	/// <summary>
	/// Street address the order is shipped to.
	/// </summary>
	[JsonPropertyName("shipAddress")]
	[Description("Street address the order is shipped to.")]
	[StringLength(60)]
	public string? ShipAddress { get; init; }
	
	/// <summary>
	/// City the order is shipped to.
	/// </summary>
	[JsonPropertyName("shipCity")]
	[Description("City the order is shipped to.")]
	[StringLength(15)]
	public string? ShipCity { get; init; }
	
	/// <summary>
	/// State or region the order is shipped to.
	/// </summary>
	[JsonPropertyName("shipRegion")]
	[Description("State or region the order is shipped to.")]
	[StringLength(15)]
	public string? ShipRegion { get; init; }
	
	/// <summary>
	/// Postal or ZIP code the order is shipped to.
	/// </summary>
	[JsonPropertyName("shipPostalCode")]
	[Description("Postal or ZIP code the order is shipped to.")]
	[StringLength(10)]
	public string? ShipPostalCode { get; init; }
	
	/// <summary>
	/// Country the order is shipped to.
	/// </summary>
	[JsonPropertyName("shipCountry")]
	[Description("Country the order is shipped to.")]
	[StringLength(15)]
	public string? ShipCountry { get; init; }
}
