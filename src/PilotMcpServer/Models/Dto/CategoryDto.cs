using System.ComponentModel;
using System.Text.Json.Serialization;

namespace PilotMcpServer.Models.Dto;

/// <summary>
/// A Northwind product category record.
/// </summary>
public sealed class CategoryDto
{
	/// <summary>
	/// Unique identifier of the category.
	/// </summary>
	[JsonPropertyName("categoryID")]
	[Description("Unique identifier of the category.")]
	public required int CategoryId { get; init; }
	
	/// <summary>
	/// Name of the category.
	/// </summary>
	[JsonPropertyName("categoryName")]
	[Description("Name of the category.")]
	public required string CategoryName { get; init; }
	
	/// <summary>
	/// Optional free-text description of the category.
	/// </summary>
	[JsonPropertyName("description")]
	[Description("Optional free-text description of the category.")]
	public string? Description { get; init; }
	
	/// <summary>
	/// Optional image data for the category, base64-encoded.
	/// </summary>
	[JsonPropertyName("picture")]
	[Description("Optional image data for the category, base64-encoded.")]
	public byte[]? Picture { get; init; }
}
