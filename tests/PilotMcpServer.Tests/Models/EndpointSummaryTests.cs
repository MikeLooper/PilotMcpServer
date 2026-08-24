using PilotMcpServer.Models;

namespace PilotMcpServer.Tests.Models;

[TestFixture]
public class EndpointSummaryTests
{
    [Test]
    public void EndpointSummary_Properties_SetAndRead_ReturnSameValues_Test()
    {
        var summary = new EndpointSummary
        {
            Resource = "Categories",
            Operation = "GetAll",
            HttpMethod = "GET",
            PathTemplate = "/categories/get-all",
            Summary = "Gets all categories records.",
        };

        Assert.That(summary.Resource, Is.EqualTo("Categories"));
        Assert.That(summary.Operation, Is.EqualTo("GetAll"));
        Assert.That(summary.HttpMethod, Is.EqualTo("GET"));
        Assert.That(summary.PathTemplate, Is.EqualTo("/categories/get-all"));
        Assert.That(summary.Summary, Is.EqualTo("Gets all categories records."));
    }
}
