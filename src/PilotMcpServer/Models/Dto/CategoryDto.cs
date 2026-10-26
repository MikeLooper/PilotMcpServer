using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace PilotMcpServer.Models.Dto;

/// <summary>
/// A Northwind product category record.
/// </summary>
public sealed class CategoryDto
{
	/// <summary>
	/// Unique identifier of the category. Omit when adding a new category; the API assigns it.
	/// </summary>
	[JsonPropertyName("categoryId")]
	[Description("Unique identifier of the category. Omit when adding a new category; the API assigns it.")]
	[Range(1, int.MaxValue)]
	public int? CategoryId { get; init; }
	
	/// <summary>
	/// Name of the category.
	/// </summary>
	[JsonPropertyName("categoryName")]
	[Description("Name of the category.")]
	[StringLength(15, MinimumLength = 1)]
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
