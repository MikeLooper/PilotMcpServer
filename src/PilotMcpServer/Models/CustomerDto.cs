using System.ComponentModel;
using System.Text.Json.Serialization;

namespace PilotMcpServer.Models;

/// <summary>A Northwind customer record.</summary>
public sealed class CustomerDto
{
    [JsonPropertyName("customerID")]
    [Description("Unique identifier of the customer (a 5-character code).")]
    public required string CustomerId { get; init; }

    [JsonPropertyName("companyName")]
    [Description("Name of the customer's company.")]
    public required string CompanyName { get; init; }

    [JsonPropertyName("contactName")]
    [Description("Name of the primary contact at the company.")]
    public string? ContactName { get; init; }

    [JsonPropertyName("contactTitle")]
    [Description("Job title of the primary contact.")]
    public string? ContactTitle { get; init; }

    [JsonPropertyName("address")]
    [Description("Street address of the customer.")]
    public string? Address { get; init; }

    [JsonPropertyName("city")]
    [Description("City of the customer's address.")]
    public string? City { get; init; }

    [JsonPropertyName("region")]
    [Description("State or region of the customer's address.")]
    public string? Region { get; init; }

    [JsonPropertyName("postalCode")]
    [Description("Postal or ZIP code of the customer's address.")]
    public string? PostalCode { get; init; }

    [JsonPropertyName("country")]
    [Description("Country of the customer's address.")]
    public string? Country { get; init; }

    [JsonPropertyName("phone")]
    [Description("Primary phone number for the customer.")]
    public string? Phone { get; init; }

    [JsonPropertyName("fax")]
    [Description("Fax number for the customer.")]
    public string? Fax { get; init; }
}
