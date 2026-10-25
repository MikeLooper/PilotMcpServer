using System.Net;
using PilotMcpServer.Services;
using PilotMcpServer.Tests.Testing.Doubles;

namespace PilotMcpServer.Tests.Services;

[TestFixture]
public class CorrelationIdDelegatingHandlerTests
{
	private const string HeaderName = "X-Correlation-Id";

	private static (HttpMessageInvoker Invoker, FakeHttpMessageHandler Inner) CreateInvoker()
	{
		var inner = new FakeHttpMessageHandler(HttpStatusCode.OK, "{}");
		var handler = new CorrelationIdDelegatingHandler { InnerHandler = inner };
		return (new HttpMessageInvoker(handler), inner);
	}

	private static string? GetSingleCorrelationId(HttpRequestMessage request)
		=> request.Headers.TryGetValues(HeaderName, out var values) ? values.Single() : null;

	[Test]
	public void CorrelationIdDelegatingHandler_HeaderName_MatchesOpenApiContract_Test()
	{
		Assert.That(CorrelationIdDelegatingHandler.HeaderName, Is.EqualTo(HeaderName));
	}

	[TestCase("GET", "http://localhost:11001/v1/categories/get-all")]
	[TestCase("GET", "http://localhost:11001/v1/categories/get/1")]
	[TestCase("POST", "http://localhost:11001/v1/categories/add")]
	[TestCase("PUT", "http://localhost:11001/v1/categories/update")]
	[TestCase("DELETE", "http://localhost:11001/v1/categories/delete/1")]
	public async Task CorrelationIdDelegatingHandler_DomainRequestWithoutHeader_AddsNewGuid_Test(string method, string uri)
	{
		var (invoker, inner) = CreateInvoker();
		using var request = new HttpRequestMessage(new HttpMethod(method), uri);

		await invoker.SendAsync(request, CancellationToken.None);

		var value = GetSingleCorrelationId(inner.LastRequest!);
		Assert.That(Guid.TryParse(value, out var parsed), Is.True);
		Assert.That(parsed, Is.Not.EqualTo(Guid.Empty));
	}

	[Test]
	public async Task CorrelationIdDelegatingHandler_DomainRequestWithValidGuid_PreservesValue_Test()
	{
		var (invoker, inner) = CreateInvoker();
		const string existing = "7ea2c7f7-8b46-475d-86f8-7aaaa9e4a35b";
		using var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost:11001/v1/categories/get-all");
		request.Headers.Add(HeaderName, existing);

		await invoker.SendAsync(request, CancellationToken.None);

		Assert.That(GetSingleCorrelationId(inner.LastRequest!), Is.EqualTo(existing));
	}

	[TestCase("")]
	[TestCase("not-a-guid")]
	[TestCase("12345")]
	public async Task CorrelationIdDelegatingHandler_DomainRequestWithInvalidValue_ReplacesWithNewGuid_Test(string invalid)
	{
		var (invoker, inner) = CreateInvoker();
		using var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost:11001/v1/categories/get-all");
		request.Headers.TryAddWithoutValidation(HeaderName, invalid);

		await invoker.SendAsync(request, CancellationToken.None);

		var value = GetSingleCorrelationId(inner.LastRequest!);
		Assert.That(value, Is.Not.EqualTo(invalid));
		Assert.That(Guid.TryParse(value, out _), Is.True);
	}

	[Test]
	public async Task CorrelationIdDelegatingHandler_DomainRequestWithMultipleValues_ReplacesWithSingleNewGuid_Test()
	{
		var (invoker, inner) = CreateInvoker();
		var first = Guid.NewGuid().ToString();
		var second = Guid.NewGuid().ToString();
		using var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost:11001/v1/categories/get-all");
		request.Headers.Add(HeaderName, [first, second]);

		await invoker.SendAsync(request, CancellationToken.None);

		var value = GetSingleCorrelationId(inner.LastRequest!);
		Assert.That(value, Is.Not.EqualTo(first).And.Not.EqualTo(second));
		Assert.That(Guid.TryParse(value, out _), Is.True);
	}

	[Test]
	public async Task CorrelationIdDelegatingHandler_SeparateRequests_ReceiveDistinctGuids_Test()
	{
		var (invoker, inner) = CreateInvoker();
		using var firstRequest = new HttpRequestMessage(HttpMethod.Get, "http://localhost:11001/v1/categories/get-all");
		using var secondRequest = new HttpRequestMessage(HttpMethod.Get, "http://localhost:11001/v1/categories/get-all");

		await invoker.SendAsync(firstRequest, CancellationToken.None);
		var first = GetSingleCorrelationId(inner.LastRequest!);
		await invoker.SendAsync(secondRequest, CancellationToken.None);
		var second = GetSingleCorrelationId(inner.LastRequest!);

		Assert.That(first, Is.Not.EqualTo(second));
	}

	[TestCase("http://localhost:11001/about")]
	[TestCase("http://localhost:11001/about?show-details=true")]
	[TestCase("http://localhost:11001/healthcheck")]
	public async Task CorrelationIdDelegatingHandler_SystemRequest_DoesNotAddHeader_Test(string uri)
	{
		var (invoker, inner) = CreateInvoker();
		using var request = new HttpRequestMessage(HttpMethod.Get, uri);

		await invoker.SendAsync(request, CancellationToken.None);

		Assert.That(inner.LastRequest!.Headers.Contains(HeaderName), Is.False);
	}
}
