using System.ComponentModel;
using System.Text.Json.Serialization;

namespace PilotMcpServer.Models;

/// <summary>A Northwind order-line record, keyed by the composite (orderID, productID) pair.</summary>
public sealed class OrderDetailDto
{
    [JsonPropertyName("orderID")]
    [Description("Identifier of the order this line belongs to.")]
    public required int OrderId { get; init; }

    [JsonPropertyName("productID")]
    [Description("Identifier of the product on this order line.")]
    public required int ProductId { get; init; }

    [JsonPropertyName("unitPrice")]
    [Description("Price per unit charged for the product on this order line.")]
    public required double UnitPrice { get; init; }

    [JsonPropertyName("quantity")]
    [Description("Number of units of the product ordered.")]
    public required short Quantity { get; init; }

    [JsonPropertyName("discount")]
    [Description("Discount applied to this order line, expressed as a fraction (e.g. 0.15 for 15%).")]
    public required float Discount { get; init; }
}
