using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace PilotMcpServer.Models.Dto;

/// <summary>
/// A Northwind product record.
/// </summary>
public sealed class ProductDto
{
	/// <summary>
	/// Unique identifier of the product. Omit when adding a new product; the API assigns it.
	/// </summary>
	[JsonPropertyName("productId")]
	[Description("Unique identifier of the product. Omit when adding a new product; the API assigns it.")]
	[Range(1, int.MaxValue)]
	public int? ProductId { get; init; }

	/// <summary>
	/// Name of the product.
	/// </summary>
	[JsonPropertyName("productName")]
	[Description("Name of the product.")]
	[StringLength(40, MinimumLength = 1)]
	public required string ProductName { get; init; }

	/// <summary>
	/// Identifier of the supplier of this product.
	/// </summary>
	[JsonPropertyName("supplierId")]
	[Description("Identifier of the supplier of this product.")]
	[Range(1, int.MaxValue)]
	public int? SupplierId { get; init; }

	/// <summary>
	/// Identifier of the category this product belongs to.
	/// </summary>
	[JsonPropertyName("categoryId")]
	[Description("Identifier of the category this product belongs to.")]
	[Range(1, int.MaxValue)]
	public int? CategoryId { get; init; }

	/// <summary>
	/// Free-text description of the quantity per unit (e.g. '10 boxes x 20 bags').
	/// </summary>
	[JsonPropertyName("quantityPerUnit")]
	[Description("Free-text description of the quantity per unit (e.g. '10 boxes x 20 bags').")]
	[StringLength(20)]
	public string? QuantityPerUnit { get; init; }

	/// <summary>
	/// Price per unit of the product. Defaults to 0 when omitted.
	/// </summary>
	[JsonPropertyName("unitPrice")]
	[Description("Price per unit of the product. Defaults to 0 when omitted.")]
	[Range(0d, 922337203685477.5807d)]
	public double? UnitPrice { get; init; }

	/// <summary>
	/// Number of units currently in stock. Defaults to 0 when omitted.
	/// </summary>
	[JsonPropertyName("unitsInStock")]
	[Description("Number of units currently in stock. Defaults to 0 when omitted.")]
	[Range(0, short.MaxValue)]
	public short? UnitsInStock { get; init; }

	/// <summary>
	/// Number of units currently on order from the supplier. Defaults to 0 when omitted.
	/// </summary>
	[JsonPropertyName("unitsOnOrder")]
	[Description("Number of units currently on order from the supplier. Defaults to 0 when omitted.")]
	[Range(0, short.MaxValue)]
	public short? UnitsOnOrder { get; init; }

	/// <summary>
	/// Stock level at which more of the product should be reordered. Defaults to 0 when omitted.
	/// </summary>
	[JsonPropertyName("reorderLevel")]
	[Description("Stock level at which more of the product should be reordered. Defaults to 0 when omitted.")]
	[Range(0, short.MaxValue)]
	public short? ReorderLevel { get; init; }

	/// <summary>
	/// Whether the product has been discontinued. Defaults to false.
	/// </summary>
	[JsonPropertyName("discontinued")]
	[Description("Whether the product has been discontinued. Defaults to false.")]
	public bool Discontinued { get; init; }
}
