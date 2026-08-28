using System.Text.Json;
using PilotMcpServer.Models;

namespace PilotMcpServer.Tests.Models.Response;

[TestFixture]
public class ProblemDetailsResponseTests
{
    [Test]
    public void ProblemDetailsResponse_Properties_SetAndRead_ReturnSameValues_Test()
    {
        var problem = new ProblemDetailsResponse
        {
            Type = "https://example.com/errors/validation",
            Title = "Invalid",
            Status = 400,
            Detail = "categoryId must be positive",
            Instance = "/categories/delete/-1",
        };

        Assert.That(problem.Type, Is.EqualTo("https://example.com/errors/validation"));
        Assert.That(problem.Title, Is.EqualTo("Invalid"));
        Assert.That(problem.Status, Is.EqualTo(400));
        Assert.That(problem.Detail, Is.EqualTo("categoryId must be positive"));
        Assert.That(problem.Instance, Is.EqualTo("/categories/delete/-1"));
    }

    [Test]
    public void ProblemDetailsResponse_Deserialize_MapsAllPropertiesFromApiJson_Test()
    {
        const string json = """
            {
              "type": "https://example.com/errors/validation",
              "title": "Invalid",
              "status": 400,
              "detail": "categoryId must be positive",
              "instance": "/categories/delete/-1"
            }
            """;

        var problem = JsonSerializer.Deserialize<ProblemDetailsResponse>(json);

        Assert.That(problem, Is.Not.Null);
        Assert.That(problem!.Type, Is.EqualTo("https://example.com/errors/validation"));
        Assert.That(problem.Title, Is.EqualTo("Invalid"));
        Assert.That(problem.Status, Is.EqualTo(400));
        Assert.That(problem.Detail, Is.EqualTo("categoryId must be positive"));
        Assert.That(problem.Instance, Is.EqualTo("/categories/delete/-1"));
    }
}
