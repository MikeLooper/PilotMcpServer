using System.ComponentModel;
using System.Text.Json.Serialization;

namespace PilotMcpServer.Models;

/// <summary>A Northwind supplier record.</summary>
public sealed class SupplierDto
{
    [JsonPropertyName("supplierID")]
    [Description("Unique identifier of the supplier.")]
    public required int SupplierId { get; init; }

    [JsonPropertyName("companyName")]
    [Description("Name of the supplier's company.")]
    public required string CompanyName { get; init; }

    [JsonPropertyName("contactName")]
    [Description("Name of the primary contact at the supplier.")]
    public string? ContactName { get; init; }

    [JsonPropertyName("contactTitle")]
    [Description("Job title of the primary contact.")]
    public string? ContactTitle { get; init; }

    [JsonPropertyName("address")]
    [Description("Street address of the supplier.")]
    public string? Address { get; init; }

    [JsonPropertyName("city")]
    [Description("City of the supplier's address.")]
    public string? City { get; init; }

    [JsonPropertyName("region")]
    [Description("State or region of the supplier's address.")]
    public string? Region { get; init; }

    [JsonPropertyName("postalCode")]
    [Description("Postal or ZIP code of the supplier's address.")]
    public string? PostalCode { get; init; }

    [JsonPropertyName("country")]
    [Description("Country of the supplier's address.")]
    public string? Country { get; init; }

    [JsonPropertyName("phone")]
    [Description("Phone number for the supplier.")]
    public string? Phone { get; init; }

    [JsonPropertyName("fax")]
    [Description("Fax number for the supplier.")]
    public string? Fax { get; init; }

    [JsonPropertyName("homePage")]
    [Description("URL of the supplier's home page.")]
    public string? HomePage { get; init; }
}
