using PilotMcpServer.Configuration;

namespace PilotMcpServer.Tests.Configuration;

[TestFixture]
public class PilotEndpointCatalogTests
{
    [Test]
    public void PilotEndpointCatalog_All_ContainsFiveEndpointsPerResourcePlusTwoSystemEndpoints_Test()
    {
        var all = PilotEndpointCatalog.All;

        // 8 resources * 5 CRUD operations + healthcheck + about
        Assert.That(all, Has.Count.EqualTo(8 * 5 + 2));
    }

    [Test]
    public void PilotEndpointCatalog_All_GroupsByExpectedResourceNames_Test()
    {
        var all = PilotEndpointCatalog.All;

        var resources = all.Select(e => e.Resource).Distinct().OrderBy(r => r).ToList();

        Assert.That(resources, Is.EquivalentTo(new[]
        {
            "Categories", "Customers", "Employees", "OrderDetails", "Orders", "Products", "Shippers", "Suppliers", "System",
        }));
    }

    [Test]
    public void PilotEndpointCatalog_All_ContainsGetAllCategoriesEndpoint_Test()
    {
        var all = PilotEndpointCatalog.All;

        var entry = all.SingleOrDefault(e => e.Resource == "Categories" && e.Operation == "GetAll");

        Assert.That(entry, Is.Not.Null);
        Assert.That(entry!.HttpMethod, Is.EqualTo("GET"));
        Assert.That(entry.PathTemplate, Is.EqualTo("/categories/get-all"));
    }

    [Test]
    public void PilotEndpointCatalog_All_ContainsCompositeKeyOrderDetailEndpoints_Test()
    {
        var all = PilotEndpointCatalog.All;

        var get = all.Single(e => e.Resource == "OrderDetails" && e.Operation == "Get");
        var delete = all.Single(e => e.Resource == "OrderDetails" && e.Operation == "Delete");

        Assert.That(get.PathTemplate, Is.EqualTo("/order-details/get/product/{productId}/order/{orderId}"));
        Assert.That(delete.PathTemplate, Is.EqualTo("/order-details/delete/product/{productId}/order/{orderId}"));
    }

    [Test]
    public void PilotEndpointCatalog_All_ContainsSystemEndpoints_Test()
    {
        var all = PilotEndpointCatalog.All;

        var health = all.Single(e => e.Resource == "System" && e.Operation == "HealthCheck");
        var about = all.Single(e => e.Resource == "System" && e.Operation == "About");

        Assert.That(health.PathTemplate, Is.EqualTo("/healthcheck"));
        Assert.That(about.PathTemplate, Is.EqualTo("/about"));
    }
}
