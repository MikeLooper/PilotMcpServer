using System.ComponentModel;
using System.Text.Json.Serialization;

namespace PilotMcpServer.Models;

/// <summary>A Northwind shipping company record.</summary>
public sealed class ShipperDto
{
    [JsonPropertyName("shipperID")]
    [Description("Unique identifier of the shipper.")]
    public required int ShipperId { get; init; }

    [JsonPropertyName("companyName")]
    [Description("Name of the shipping company.")]
    public required string CompanyName { get; init; }

    [JsonPropertyName("phone")]
    [Description("Phone number for the shipping company.")]
    public string? Phone { get; init; }
}
