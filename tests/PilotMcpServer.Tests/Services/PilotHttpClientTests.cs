using System.Net;
using PilotMcpServer.Configuration;
using PilotMcpServer.Models;
using PilotMcpServer.Services;
using PilotMcpServer.Tests.Testing.Doubles;

namespace PilotMcpServer.Tests.Services;

[TestFixture]
public class PilotHttpClientTests
{
    private static (PilotHttpClient Client, FakeHttpMessageHandler Handler) CreateClient(HttpStatusCode statusCode, string content)
    {
        var handler = new FakeHttpMessageHandler(statusCode, content);
        var httpClient = new HttpClient(handler);
        var client = new PilotHttpClient(httpClient, new PilotApiSelectionState());
        return (client, handler);
    }

    [Test]
    public async Task PilotHttpClient_GetJsonListAsync_NoOverride_BuildsUriAgainstSelectedApiAndDeserializesList_Test()
    {
        var (client, handler) = CreateClient(HttpStatusCode.OK, """[{"categoryID":1,"categoryName":"Beverages"}]""");

        var result = await client.GetJsonListAsync<CategoryDto>("/categories/get-all", null, CancellationToken.None);

        Assert.That(result, Has.Count.EqualTo(1));
        Assert.That(result[0].CategoryName, Is.EqualTo("Beverages"));
        Assert.That(handler.LastRequest!.RequestUri!.ToString(), Is.EqualTo("http://localhost:55101/categories/get-all"));
        Assert.That(handler.LastRequest.Method, Is.EqualTo(HttpMethod.Get));
    }

    [Test]
    public async Task PilotHttpClient_GetJsonListAsync_ApiNameOverride_BuildsUriAgainstOverriddenApi_Test()
    {
        var (client, handler) = CreateClient(HttpStatusCode.OK, "[]");

        await client.GetJsonListAsync<CategoryDto>("/categories/get-all", "Python with PostgreSQL", CancellationToken.None);

        Assert.That(handler.LastRequest!.RequestUri!.ToString(), Is.EqualTo("http://localhost:55801/categories/get-all"));
    }

    [Test]
    public void PilotHttpClient_ResolveEndpoint_UnknownApiName_ThrowsArgumentException_Test()
    {
        var (client, _) = CreateClient(HttpStatusCode.OK, "{}");

        Assert.Throws<ArgumentException>(() => client.ResolveEndpoint("Not A Real Api"));
    }

    [Test]
    public void PilotHttpClient_ResolveEndpoint_NullApiName_ReturnsCurrentSelection_Test()
    {
        var (client, _) = CreateClient(HttpStatusCode.OK, "{}");

        var endpoint = client.ResolveEndpoint(null);

        Assert.That(endpoint, Is.EqualTo(PilotApiCatalog.Default));
    }

    [Test]
    public async Task PilotHttpClient_GetJsonAsync_NotFoundResponse_ReturnsNull_Test()
    {
        var (client, _) = CreateClient(HttpStatusCode.NotFound, "");

        var result = await client.GetJsonAsync<CategoryDto>("/categories/get/999", null, CancellationToken.None);

        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task PilotHttpClient_GetJsonAsync_SuccessResponse_ReturnsDeserializedObject_Test()
    {
        var (client, _) = CreateClient(HttpStatusCode.OK, """{"categoryID":7,"categoryName":"Produce"}""");

        var result = await client.GetJsonAsync<CategoryDto>("/categories/get/7", null, CancellationToken.None);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.CategoryId, Is.EqualTo(7));
        Assert.That(result.CategoryName, Is.EqualTo("Produce"));
    }

    [Test]
    public async Task PilotHttpClient_PostJsonAsync_SendsBodyAndReturnsTypedResponse_Test()
    {
        var (client, handler) = CreateClient(HttpStatusCode.OK, """{"id":42}""");
        var category = new CategoryDto { CategoryId = 0, CategoryName = "Snacks" };

        var result = await client.PostJsonAsync<CategoryDto, AddResponse>("/categories/add", category, null, CancellationToken.None);

        Assert.That(result.Id, Is.EqualTo(42));
        Assert.That(handler.LastRequest!.Method, Is.EqualTo(HttpMethod.Post));
        Assert.That(handler.LastRequestBody, Does.Contain("Snacks"));
    }

    [Test]
    public async Task PilotHttpClient_PutJsonAsync_SendsPutWithBody_Test()
    {
        var (client, handler) = CreateClient(HttpStatusCode.OK, "");
        var category = new CategoryDto { CategoryId = 1, CategoryName = "Updated" };

        await client.PutJsonAsync("/categories/update", category, null, CancellationToken.None);

        Assert.That(handler.LastRequest!.Method, Is.EqualTo(HttpMethod.Put));
        Assert.That(handler.LastRequestBody, Does.Contain("Updated"));
    }

    [Test]
    public async Task PilotHttpClient_DeleteAsync_SendsDeleteRequest_Test()
    {
        var (client, handler) = CreateClient(HttpStatusCode.NoContent, "");

        await client.DeleteAsync("/categories/delete/1", null, CancellationToken.None);

        Assert.That(handler.LastRequest!.Method, Is.EqualTo(HttpMethod.Delete));
        Assert.That(handler.LastRequest.RequestUri!.ToString(), Is.EqualTo("http://localhost:55101/categories/delete/1"));
    }

    [Test]
    public void PilotHttpClient_DeleteAsync_BadRequestResponse_ThrowsWithParsedProblemDetails_Test()
    {
        var (client, _) = CreateClient(HttpStatusCode.BadRequest, """{"title":"Invalid","detail":"categoryId must be positive","status":400}""");

        var ex = Assert.ThrowsAsync<PilotApiException>(() => client.DeleteAsync("/categories/delete/-1", null, CancellationToken.None));

        Assert.That(ex!.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        Assert.That(ex.Problem, Is.Not.Null);
        Assert.That(ex.Problem!.Detail, Is.EqualTo("categoryId must be positive"));
        Assert.That(ex.Message, Does.Contain("categoryId must be positive"));
    }

    [Test]
    public void PilotHttpClient_GetJsonListAsync_ServerErrorWithoutProblemDetailsBody_ThrowsWithoutParsedProblem_Test()
    {
        var (client, _) = CreateClient(HttpStatusCode.InternalServerError, "not json");

        var ex = Assert.ThrowsAsync<PilotApiException>(() => client.GetJsonListAsync<CategoryDto>("/categories/get-all", null, CancellationToken.None));

        Assert.That(ex!.StatusCode, Is.EqualTo(HttpStatusCode.InternalServerError));
        Assert.That(ex.Problem, Is.Null);
    }

    [Test]
    public async Task PilotHttpClient_GetAboutAsync_DeserializesAboutResponseForGivenEndpoint_Test()
    {
        var (client, handler) = CreateClient(HttpStatusCode.OK, """{"name":"PilotApiDotNet","apiVersion":"1.0.0","buildVersion":"1.0.0.123","deployDate":"2026-01-01"}""");
        var endpoint = PilotApiCatalog.Default;

        var result = await client.GetAboutAsync(endpoint, CancellationToken.None);

        Assert.That(result.Name, Is.EqualTo("PilotApiDotNet"));
        Assert.That(result.ApiVersion, Is.EqualTo("1.0.0"));
        Assert.That(handler.LastRequest!.RequestUri!.ToString(), Is.EqualTo("http://localhost:55101/about"));
    }
}
