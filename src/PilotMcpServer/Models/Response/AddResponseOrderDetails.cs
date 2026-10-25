using System.ComponentModel;
using System.Text.Json.Serialization;

namespace PilotMcpServer.Models;

/// <summary>
/// Response returned by the Pilot API "order-details/add" endpoint, identifying the new record by its
/// composite (orderId, productId) key.
/// </summary>
public sealed class AddResponseOrderDetails
{
	/// <summary>
	/// Gets the order identifier of the newly created order line.
	/// </summary>
	[JsonPropertyName("orderId")]
	[Description("The order identifier of the newly created order line.")]
	public required int OrderId { get; init; }

	/// <summary>
	/// Gets the product identifier of the newly created order line.
	/// </summary>
	[JsonPropertyName("productId")]
	[Description("The product identifier of the newly created order line.")]
	public required int ProductId { get; init; }
}
