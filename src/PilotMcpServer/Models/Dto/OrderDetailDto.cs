using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace PilotMcpServer.Models.Dto;

/// <summary>
/// A Northwind order-line record, keyed by the composite (orderID, productID) pair.
/// </summary>
public sealed class OrderDetailDto
{
	/// <summary>
	/// Identifier of the order this line belongs to.
	/// </summary>
	[JsonPropertyName("orderId")]
	[Description("Identifier of the order this line belongs to.")]
	[Range(1, int.MaxValue)]
	public required int OrderId { get; init; }

	/// <summary>
	/// Identifier of the product on this order line.
	/// </summary>
	[JsonPropertyName("productId")]
	[Description("Identifier of the product on this order line.")]
	[Range(1, int.MaxValue)]
	public required int ProductId { get; init; }

	/// <summary>
	/// Price per unit charged for the product on this order line.
	/// </summary>
	[JsonPropertyName("unitPrice")]
	[Description("Price per unit charged for the product on this order line.")]
	[Range(0d, 922337203685477.5807d)]
	public required double UnitPrice { get; init; }

	/// <summary>
	/// Number of units of the product ordered.
	/// </summary>
	[JsonPropertyName("quantity")]
	[Description("Number of units of the product ordered.")]
	[Range(1, short.MaxValue)]
	public required short Quantity { get; init; }

	/// <summary>
	/// Discount applied to this order line, expressed as a fraction of the unit price from 0 to 1 (e.g. 0.15 for 15%).
	/// </summary>
	[JsonPropertyName("discount")]
	[Description("Discount applied to this order line, expressed as a fraction of the unit price from 0 to 1 (e.g. 0.15 for 15%).")]
	[Range(0f, 1f)]
	public required float Discount { get; init; }
}
