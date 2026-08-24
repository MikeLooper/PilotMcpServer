using PilotMcpServer.Models;

namespace PilotMcpServer.Configuration;

/// <summary>
/// A static, resource-grouped summary of the endpoints defined by the Pilot API OpenAPI contract
/// (see shared/PilotSharedSource/OpenAPI/PilotApi_v1.yaml). Every configured Pilot API deployment
/// implements this exact same set of endpoints, so it is documented once here rather than per-API.
/// </summary>
public static class PilotEndpointCatalog
{
    private sealed record ResourceInfo(string Name, string Route, string Singular, string Plural, string IdRoute, string IdDescription);

    private static readonly ResourceInfo[] Resources =
    [
        new("Categories", "categories", "category", "categories", "{categoryId}", "numeric ID"),
        new("Customers", "customers", "customer", "customers", "{customerId}", "5-character customer code"),
        new("Employees", "employees", "employee", "employees", "{employeeId}", "numeric ID"),
        new("OrderDetails", "order-details", "order line", "order lines", "product/{productId}/order/{orderId}", "product ID and order ID"),
        new("Orders", "orders", "order", "orders", "{orderId}", "numeric ID"),
        new("Products", "products", "product", "products", "{productId}", "numeric ID"),
        new("Shippers", "shippers", "shipper", "shippers", "{shipperId}", "numeric ID"),
        new("Suppliers", "suppliers", "supplier", "suppliers", "{supplierId}", "numeric ID"),
    ];

    public static readonly IReadOnlyList<EndpointSummary> All = BuildCatalog();

    private static IReadOnlyList<EndpointSummary> BuildCatalog()
    {
        var summaries = new List<EndpointSummary>();

        foreach (var resource in Resources)
        {
            summaries.Add(new EndpointSummary
            {
                Resource = resource.Name,
                Operation = "GetAll",
                HttpMethod = "GET",
                PathTemplate = $"/{resource.Route}/get-all",
                Summary = $"Gets all {resource.Plural} records.",
            });
            summaries.Add(new EndpointSummary
            {
                Resource = resource.Name,
                Operation = "Get",
                HttpMethod = "GET",
                PathTemplate = $"/{resource.Route}/get/{resource.IdRoute}",
                Summary = $"Gets a single {resource.Singular} record by its {resource.IdDescription}.",
            });
            summaries.Add(new EndpointSummary
            {
                Resource = resource.Name,
                Operation = "Add",
                HttpMethod = "POST",
                PathTemplate = $"/{resource.Route}/add",
                Summary = $"Adds a new {resource.Singular} record.",
            });
            summaries.Add(new EndpointSummary
            {
                Resource = resource.Name,
                Operation = "Update",
                HttpMethod = "PUT",
                PathTemplate = $"/{resource.Route}/update",
                Summary = $"Updates an existing {resource.Singular} record.",
            });
            summaries.Add(new EndpointSummary
            {
                Resource = resource.Name,
                Operation = "Delete",
                HttpMethod = "DELETE",
                PathTemplate = $"/{resource.Route}/delete/{resource.IdRoute}",
                Summary = $"Deletes a {resource.Singular} record by its {resource.IdDescription}.",
            });
        }

        summaries.Add(new EndpointSummary
        {
            Resource = "System",
            Operation = "HealthCheck",
            HttpMethod = "GET",
            PathTemplate = "/healthcheck",
            Summary = "Returns OK if the API is running.",
        });
        summaries.Add(new EndpointSummary
        {
            Resource = "System",
            Operation = "About",
            HttpMethod = "GET",
            PathTemplate = "/about",
            Summary = "Returns application metadata: name, API version, build version, and deploy date.",
        });

        return summaries;
    }
}
