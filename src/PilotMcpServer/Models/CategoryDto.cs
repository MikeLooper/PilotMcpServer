using System.ComponentModel;
using System.Text.Json.Serialization;

namespace PilotMcpServer.Models;

/// <summary>A Northwind product category record.</summary>
public sealed class CategoryDto
{
    [JsonPropertyName("categoryID")]
    [Description("Unique identifier of the category.")]
    public required int CategoryId { get; init; }

    [JsonPropertyName("categoryName")]
    [Description("Name of the category.")]
    public required string CategoryName { get; init; }

    [JsonPropertyName("description")]
    [Description("Optional free-text description of the category.")]
    public string? Description { get; init; }

    [JsonPropertyName("picture")]
    [Description("Optional image data for the category, base64-encoded.")]
    public byte[]? Picture { get; init; }
}
