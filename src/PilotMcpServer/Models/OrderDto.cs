using System.ComponentModel;
using System.Text.Json.Serialization;

namespace PilotMcpServer.Models;

/// <summary>A Northwind order record.</summary>
public sealed class OrderDto
{
    [JsonPropertyName("orderID")]
    [Description("Unique identifier of the order.")]
    public required int OrderId { get; init; }

    [JsonPropertyName("customerID")]
    [Description("Identifier of the customer who placed the order.")]
    public string? CustomerId { get; init; }

    [JsonPropertyName("employeeID")]
    [Description("Identifier of the employee who handled the order.")]
    public int? EmployeeId { get; init; }

    [JsonPropertyName("orderDate")]
    [Description("Date the order was placed, ISO 8601.")]
    public DateTime? OrderDate { get; init; }

    [JsonPropertyName("requiredDate")]
    [Description("Date the order is required by, ISO 8601.")]
    public DateTime? RequiredDate { get; init; }

    [JsonPropertyName("shippedDate")]
    [Description("Date the order was shipped, ISO 8601.")]
    public DateTime? ShippedDate { get; init; }

    [JsonPropertyName("shipVia")]
    [Description("Identifier of the shipper used for the order.")]
    public int? ShipVia { get; init; }

    [JsonPropertyName("freight")]
    [Description("Freight cost charged for the order.")]
    public double? Freight { get; init; }

    [JsonPropertyName("shipName")]
    [Description("Name of the person/company the order is shipped to.")]
    public string? ShipName { get; init; }

    [JsonPropertyName("shipAddress")]
    [Description("Street address the order is shipped to.")]
    public string? ShipAddress { get; init; }

    [JsonPropertyName("shipCity")]
    [Description("City the order is shipped to.")]
    public string? ShipCity { get; init; }

    [JsonPropertyName("shipRegion")]
    [Description("State or region the order is shipped to.")]
    public string? ShipRegion { get; init; }

    [JsonPropertyName("shipPostalCode")]
    [Description("Postal or ZIP code the order is shipped to.")]
    public string? ShipPostalCode { get; init; }

    [JsonPropertyName("shipCountry")]
    [Description("Country the order is shipped to.")]
    public string? ShipCountry { get; init; }
}
