using System.Net;
using PilotMcpServer.Configuration;
using PilotMcpServer.Services;
using PilotMcpServer.Tests.Testing.Doubles;

namespace PilotMcpServer.Tests.Services;

[TestFixture]
public class PilotApiHeadersDelegatingHandlerTests
{
	private static (HttpMessageInvoker Invoker, FakeHttpMessageHandler Inner) CreateInvoker()
	{
		var inner = new FakeHttpMessageHandler(HttpStatusCode.OK, "{}");
		var handler = new PilotApiHeadersDelegatingHandler { InnerHandler = inner };
		return (new HttpMessageInvoker(handler), inner);
	}

	private static string? GetSingleValue(HttpRequestMessage request, string headerName)
		=> request.Headers.TryGetValues(headerName, out var values) ? values.Single() : null;

	[Test]
	public void PilotApiHeadersDelegatingHandler_HeaderNames_MatchOpenApiContract_Test()
	{
		Assert.That(PilotApiHeadersDelegatingHandler.ApiVersionHeaderName, Is.EqualTo("X-Api-Version"));
		Assert.That(PilotApiHeadersDelegatingHandler.CallerNameHeaderName, Is.EqualTo("X-Caller-Name"));
	}

	[Test]
	public void PilotApiHeadersDelegatingHandler_CallerName_FitsOpenApiMaxLength_Test()
	{
		Assert.That(PilotApiHeadersDelegatingHandler.CallerName, Has.Length.InRange(1, 100));
	}

	[TestCase("GET", "http://localhost:11001/about")]
	[TestCase("GET", "http://localhost:11001/healthcheck")]
	[TestCase("GET", "http://localhost:11001/v1/categories/get-all")]
	[TestCase("POST", "http://localhost:11001/v1/categories/add")]
	[TestCase("PUT", "http://localhost:11001/v1/categories/update")]
	[TestCase("DELETE", "http://localhost:11001/v1/categories/delete/1")]
	public async Task PilotApiHeadersDelegatingHandler_RequestWithoutHeaders_AddsApiVersionAndCallerName_Test(string method, string uri)
	{
		var (invoker, inner) = CreateInvoker();
		using var request = new HttpRequestMessage(new HttpMethod(method), uri);

		await invoker.SendAsync(request, CancellationToken.None);

		Assert.That(GetSingleValue(inner.LastRequest!, PilotApiHeadersDelegatingHandler.ApiVersionHeaderName), Is.EqualTo(PilotApiCatalog.Catalog.ApiVersion));
		Assert.That(GetSingleValue(inner.LastRequest!, PilotApiHeadersDelegatingHandler.CallerNameHeaderName), Is.EqualTo(PilotApiHeadersDelegatingHandler.CallerName));
	}

	[Test]
	public async Task PilotApiHeadersDelegatingHandler_RequestWithHeaders_PreservesValues_Test()
	{
		var (invoker, inner) = CreateInvoker();
		using var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost:11001/v1/categories/get-all");
		request.Headers.Add(PilotApiHeadersDelegatingHandler.ApiVersionHeaderName, "1.2.3");
		request.Headers.Add(PilotApiHeadersDelegatingHandler.CallerNameHeaderName, "OtherCaller");

		await invoker.SendAsync(request, CancellationToken.None);

		Assert.That(GetSingleValue(inner.LastRequest!, PilotApiHeadersDelegatingHandler.ApiVersionHeaderName), Is.EqualTo("1.2.3"));
		Assert.That(GetSingleValue(inner.LastRequest!, PilotApiHeadersDelegatingHandler.CallerNameHeaderName), Is.EqualTo("OtherCaller"));
	}
}
