using System.Net;
using Microsoft.Extensions.Logging;
using PilotMcpServer.Models;
using PilotMcpServer.Services;
using PilotMcpServer.Tests.Testing.Doubles;

namespace PilotMcpServer.Tests.Services;

[TestFixture]
public class PilotApiLoggingDelegatingHandlerTests
{
	private const string CorrelationId = "7ea2c7f7-8b46-475d-86f8-7aaaa9e4a35b";

	private CapturingLoggerProvider loggerProvider = null!;
	private ILoggerFactory loggerFactory = null!;

	[SetUp]
	public void SetUp()
	{
		loggerProvider = new CapturingLoggerProvider();
		loggerFactory = LoggerFactory.Create(builder => builder.AddProvider(loggerProvider));
	}

	[TearDown]
	public void TearDown()
	{
		loggerFactory.Dispose();
		loggerProvider.Dispose();
	}

	private HttpMessageInvoker CreateInvoker(HttpMessageHandler inner, RequestSettings? requestSettings = null)
	{
		var accessor = new RequestSettingsAccessor { Current = requestSettings };
		var handler = new PilotApiLoggingDelegatingHandler(loggerFactory.CreateLogger<PilotApiLoggingDelegatingHandler>(), accessor)
		{
			InnerHandler = inner,
		};
		return new HttpMessageInvoker(handler);
	}

	private static HttpRequestMessage CreateRequest(string uri = "http://localhost:11001/v1/categories/get/1")
	{
		var request = new HttpRequestMessage(HttpMethod.Get, uri);
		request.Headers.Add(CorrelationIdDelegatingHandler.HeaderName, CorrelationId);
		return request;
	}

	[Test]
	public async Task PilotApiLoggingDelegatingHandler_SuccessfulCall_LogsStartAndEndWithRequestSettings_Test()
	{
		var settings = new RequestSettings { CorrelationId = CorrelationId, RequestId = "42", CallerName = "Visual Studio Code" };
		using var invoker = CreateInvoker(new FakeHttpMessageHandler(HttpStatusCode.OK, "{}"), settings);
		using var request = CreateRequest();

		using var response = await invoker.SendAsync(request, CancellationToken.None);

		var entries = loggerProvider.Entries;
		Assert.That(entries.Select(entry => entry.EventId.Id), Is.EqualTo(new[] { 2000, 2001 }));
		Assert.That(entries.Select(entry => entry.Level), Is.All.EqualTo(LogLevel.Information));
		foreach (var entry in entries)
		{
			Assert.That(entry.State["HttpMethod"], Is.EqualTo("GET"));
			Assert.That(entry.State["PilotApiPath"], Is.EqualTo("/v1/categories/get/1"));
			Assert.That(entry.State["PilotApiName"], Is.EqualTo(".NET Core with SQL Server"));
			Assert.That(entry.State["CorrelationId"], Is.EqualTo(CorrelationId));
			Assert.That(entry.State["McpRequestId"], Is.EqualTo("42"));
			Assert.That(entry.State["CallerName"], Is.EqualTo("Visual Studio Code"));
		}

		Assert.That(entries[1].State["StatusCode"], Is.EqualTo(200));
		Assert.That(entries[1].State.ContainsKey("ElapsedMilliseconds"), Is.True);
	}

	[Test]
	public async Task PilotApiLoggingDelegatingHandler_ErrorStatus_LogsEndWithStatusCode_Test()
	{
		using var invoker = CreateInvoker(new FakeHttpMessageHandler(HttpStatusCode.NotFound, ""));
		using var request = CreateRequest();

		using var response = await invoker.SendAsync(request, CancellationToken.None);

		var end = loggerProvider.Entries.Single(entry => entry.EventId.Id == 2001);
		Assert.That(end.State["StatusCode"], Is.EqualTo(404));
	}

	[Test]
	public async Task PilotApiLoggingDelegatingHandler_OutsideToolCall_LogsWithoutRequestIdOrCallerName_Test()
	{
		using var invoker = CreateInvoker(new FakeHttpMessageHandler(HttpStatusCode.OK, "{}"));
		using var request = CreateRequest("http://localhost:11001/about");

		using var response = await invoker.SendAsync(request, CancellationToken.None);

		var start = loggerProvider.Entries.Single(entry => entry.EventId.Id == 2000);
		Assert.That(start.State["CorrelationId"], Is.EqualTo(CorrelationId));
		Assert.That(start.State["McpRequestId"], Is.Null);
		Assert.That(start.State["CallerName"], Is.Null);
	}

	[Test]
	public async Task PilotApiLoggingDelegatingHandler_UnknownHost_LogsAuthorityAsApiName_Test()
	{
		using var invoker = CreateInvoker(new FakeHttpMessageHandler(HttpStatusCode.OK, "{}"));
		using var request = CreateRequest("http://example.test:9999/about");

		using var response = await invoker.SendAsync(request, CancellationToken.None);

		Assert.That(loggerProvider.Entries[0].State["PilotApiName"], Is.EqualTo("example.test:9999"));
	}

	[Test]
	public void PilotApiLoggingDelegatingHandler_CallThrows_LogsFailureWithExceptionAndRethrows_Test()
	{
		var failure = new HttpRequestException("Connection refused.");
		using var invoker = CreateInvoker(new ThrowingHttpMessageHandler(failure));
		using var request = CreateRequest();

		var thrown = Assert.ThrowsAsync<HttpRequestException>(async () => await invoker.SendAsync(request, CancellationToken.None));

		Assert.That(thrown, Is.SameAs(failure));
		Assert.That(loggerProvider.Entries.Select(entry => entry.EventId.Id), Is.EqualTo(new[] { 2000, 2002 }));
		var failed = loggerProvider.Entries[1];
		Assert.That(failed.Level, Is.EqualTo(LogLevel.Warning));
		Assert.That(failed.Exception, Is.SameAs(failure));
		Assert.That(failed.State["CorrelationId"], Is.EqualTo(CorrelationId));
	}

	private sealed class ThrowingHttpMessageHandler(Exception exception) : HttpMessageHandler
	{
		protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
			=> Task.FromException<HttpResponseMessage>(exception);
	}
}
