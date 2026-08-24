using System.ComponentModel;
using System.Text.Json.Serialization;

namespace PilotMcpServer.Models;

/// <summary>A Northwind product record.</summary>
public sealed class ProductDto
{
    [JsonPropertyName("productID")]
    [Description("Unique identifier of the product.")]
    public required int ProductId { get; init; }

    [JsonPropertyName("productName")]
    [Description("Name of the product.")]
    public required string ProductName { get; init; }

    [JsonPropertyName("supplierID")]
    [Description("Identifier of the supplier of this product.")]
    public int? SupplierId { get; init; }

    [JsonPropertyName("categoryID")]
    [Description("Identifier of the category this product belongs to.")]
    public int? CategoryId { get; init; }

    [JsonPropertyName("quantityPerUnit")]
    [Description("Free-text description of the quantity per unit (e.g. '10 boxes x 20 bags').")]
    public string? QuantityPerUnit { get; init; }

    [JsonPropertyName("unitPrice")]
    [Description("Price per unit of the product.")]
    public double? UnitPrice { get; init; }

    [JsonPropertyName("unitsInStock")]
    [Description("Number of units currently in stock.")]
    public short UnitsInStock { get; init; }

    [JsonPropertyName("unitsOnOrder")]
    [Description("Number of units currently on order from the supplier.")]
    public short UnitsOnOrder { get; init; }

    [JsonPropertyName("reorderLevel")]
    [Description("Stock level at which more of the product should be reordered.")]
    public short ReorderLevel { get; init; }

    [JsonPropertyName("discontinued")]
    [Description("Whether the product has been discontinued.")]
    public bool Discontinued { get; init; }
}
