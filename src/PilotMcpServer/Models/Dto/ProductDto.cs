using System.ComponentModel;
using System.Text.Json.Serialization;

namespace PilotMcpServer.Models.Dto;

/// <summary>
/// A Northwind product record.
/// </summary>
public sealed class ProductDto
{
	/// <summary>
	/// Unique identifier of the product.
	/// </summary>
	[JsonPropertyName("productID")]
	[Description("Unique identifier of the product.")]
	public required int ProductId { get; init; }

	/// <summary>
	/// Name of the product.
	/// </summary>
	[JsonPropertyName("productName")]
	[Description("Name of the product.")]
	public required string ProductName { get; init; }

	/// <summary>
	/// Identifier of the supplier of this product.
	/// </summary>
	[JsonPropertyName("supplierID")]
	[Description("Identifier of the supplier of this product.")]
	public int? SupplierId { get; init; }

	/// <summary>
	/// Identifier of the category this product belongs to.
	/// </summary>
	[JsonPropertyName("categoryID")]
	[Description("Identifier of the category this product belongs to.")]
	public int? CategoryId { get; init; }

	/// <summary>
	/// Free-text description of the quantity per unit (e.g. '10 boxes x 20 bags').
	/// </summary>
	[JsonPropertyName("quantityPerUnit")]
	[Description("Free-text description of the quantity per unit (e.g. '10 boxes x 20 bags').")]
	public string? QuantityPerUnit { get; init; }

	/// <summary>
	/// Price per unit of the product.
	/// </summary>
	[JsonPropertyName("unitPrice")]
	[Description("Price per unit of the product.")]
	public double? UnitPrice { get; init; }

	/// <summary>
	/// Number of units currently in stock.
	/// </summary>
	[JsonPropertyName("unitsInStock")]
	[Description("Number of units currently in stock.")]
	public short UnitsInStock { get; init; }

	/// <summary>
	/// Number of units currently on order from the supplier.
	/// </summary>
	[JsonPropertyName("unitsOnOrder")]
	[Description("Number of units currently on order from the supplier.")]
	public short UnitsOnOrder { get; init; }

	/// <summary>
	/// Stock level at which more of the product should be reordered.
	/// </summary>
	[JsonPropertyName("reorderLevel")]
	[Description("Stock level at which more of the product should be reordered.")]
	public short ReorderLevel { get; init; }

	/// <summary>
	/// Whether the product has been discontinued.
	/// </summary>
	[JsonPropertyName("discontinued")]
	[Description("Whether the product has been discontinued.")]
	public bool Discontinued { get; init; }
}
