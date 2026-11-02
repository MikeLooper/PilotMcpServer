using System.Diagnostics;
using System.Text.Json.Nodes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using Moq;
using PilotMcpServer.Contracts.Services;
using PilotMcpServer.Models;
using PilotMcpServer.OpenTelemetry;
using PilotMcpServer.Services;
using PilotMcpServer.Tests.Testing.Doubles;

namespace PilotMcpServer.Tests.Services;

[TestFixture]
public class RequestSettingsMessageFilterTests
{
	private RequestSettingsAccessor accessor = null!;
	private CapturingLoggerProvider loggerProvider = null!;
	private ServiceProvider services = null!;

	[SetUp]
	public void SetUp()
	{
		accessor = new RequestSettingsAccessor { Current = null };
		loggerProvider = new CapturingLoggerProvider();
		services = new ServiceCollection()
			.AddSingleton<IRequestSettingsAccessor>(accessor)
			.AddLogging(logging => logging.AddProvider(loggerProvider))
			.BuildServiceProvider();
	}

	[TearDown]
	public void TearDown()
	{
		services.Dispose();
		loggerProvider.Dispose();
	}

	private MessageContext CreateContext(JsonRpcMessage message, Implementation? clientInfo = null)
	{
		var server = new Mock<McpServer>();
		server.SetupGet(s => s.Services).Returns(services);
		server.SetupGet(s => s.ClientInfo).Returns(clientInfo);
		return new MessageContext(server.Object, message);
	}

	private IReadOnlyList<CapturedLogEntry> FilterLogEntries()
		=> loggerProvider.Entries.Where(entry => entry.Category == RequestSettingsMessageFilter.LoggerCategory).ToArray();

	private static JsonRpcRequest ToolCall() => new() { Id = new RequestId(1), Method = RequestMethods.ToolsCall };

	[Test]
	public async Task RequestSettingsMessageFilter_Request_ExposesNewRequestSettingsWhileHandled_Test()
	{
		RequestSettings? seen = null;

		var handler = RequestSettingsMessageFilter.Create((_, _) =>
		{
			seen = accessor.Current;
			return Task.CompletedTask;
		});
		await handler(CreateContext(ToolCall()), CancellationToken.None);

		Assert.That(seen, Is.Not.Null);
		Assert.That(Guid.TryParse(seen!.CorrelationId, out _), Is.True);
	}

	[Test]
	public void RequestSettingsMessageFilter_HandlerThrows_RestoresPreviousRequestSettings_Test()
	{
		var previous = new RequestSettings();
		accessor.Current = previous;

		var handler = RequestSettingsMessageFilter.Create((_, _) => throw new InvalidOperationException("Handler failed."));

		Assert.ThrowsAsync<InvalidOperationException>(async () => await handler(CreateContext(ToolCall()), CancellationToken.None));
		Assert.That(accessor.Current, Is.SameAs(previous));
	}

	[Test]
	public async Task RequestSettingsMessageFilter_SeparateRequests_ReceiveDistinctCorrelationIds_Test()
	{
		var seen = new List<string>();

		var handler = RequestSettingsMessageFilter.Create((_, _) =>
		{
			seen.Add(accessor.Current!.CorrelationId);
			return Task.CompletedTask;
		});
		await handler(CreateContext(ToolCall()), CancellationToken.None);
		await handler(CreateContext(ToolCall()), CancellationToken.None);

		Assert.That(seen, Has.Count.EqualTo(2));
		Assert.That(seen[0], Is.Not.EqualTo(seen[1]));
	}

	[Test]
	public async Task RequestSettingsMessageFilter_Notification_PassesThroughWithoutRequestSettings_Test()
	{
		var called = false;
		RequestSettings? seen = new();

		var handler = RequestSettingsMessageFilter.Create((_, _) =>
		{
			called = true;
			seen = accessor.Current;
			return Task.CompletedTask;
		});
		await handler(CreateContext(new JsonRpcNotification { Method = NotificationMethods.InitializedNotification }), CancellationToken.None);

		Assert.That(called, Is.True);
		Assert.That(seen, Is.Null);
	}

	[Test]
	public async Task RequestSettingsMessageFilter_WithCurrentActivity_TagsCorrelationId_Test()
	{
		using var activity = new Activity("tools/call").Start();
		string? correlationId = null;

		var handler = RequestSettingsMessageFilter.Create((_, _) =>
		{
			correlationId = accessor.Current!.CorrelationId;
			return Task.CompletedTask;
		});
		await handler(CreateContext(ToolCall()), CancellationToken.None);

		Assert.That(activity.GetTagItem(OtelAttributeNames.CorrelationIdTag), Is.EqualTo(correlationId));
	}

	private async Task<string> GetCorrelationIdForParamsAsync(string paramsJson, string method = RequestMethods.ToolsCall)
	{
		string? correlationId = null;

		var handler = RequestSettingsMessageFilter.Create((_, _) =>
		{
			correlationId = accessor.Current!.CorrelationId;
			return Task.CompletedTask;
		});
		var request = new JsonRpcRequest { Id = new RequestId(1), Method = method, Params = JsonNode.Parse(paramsJson) };
		await handler(CreateContext(request), CancellationToken.None);

		return correlationId!;
	}

	[TestCase(RequestMethods.ToolsCall)]
	[TestCase(RequestMethods.ToolsList)]
	[TestCase(RequestMethods.Initialize)]
	public async Task RequestSettingsMessageFilter_WithSuppliedCorrelationId_UsesSuppliedValue_Test(string method)
	{
		const string supplied = "3f2504e0-4f89-41d3-9a0c-0305e82c3301";

		var correlationId = await GetCorrelationIdForParamsAsync($$$"""{"_meta":{"correlationId":"{{{supplied}}}"}}""", method);

		Assert.That(correlationId, Is.EqualTo(supplied));
	}

	[TestCase("3F2504E0-4F89-41D3-9A0C-0305E82C3301")]
	[TestCase("3f2504e04f8941d39a0c0305e82c3301")]
	[TestCase("{3f2504e0-4f89-41d3-9a0c-0305e82c3301}")]
	public async Task RequestSettingsMessageFilter_WithSuppliedCorrelationIdInOtherGuidFormat_UsesCanonicalValue_Test(string supplied)
	{
		var correlationId = await GetCorrelationIdForParamsAsync($$$"""{"_meta":{"correlationId":"{{{supplied}}}"}}""");

		Assert.That(correlationId, Is.EqualTo("3f2504e0-4f89-41d3-9a0c-0305e82c3301"));
	}

	[TestCase("""{"_meta":{"correlationId":"not-a-guid"}}""")]
	[TestCase("""{"_meta":{"correlationId":""}}""")]
	[TestCase("""{"_meta":{"correlationId":null}}""")]
	[TestCase("""{"_meta":{"correlationId":123}}""")]
	[TestCase("""{"_meta":{"correlationId":{"value":"3f2504e0-4f89-41d3-9a0c-0305e82c3301"}}}""")]
	[TestCase("""{"_meta":{"progressToken":"abc"}}""")]
	[TestCase("""{"_meta":"3f2504e0-4f89-41d3-9a0c-0305e82c3301"}""")]
	[TestCase("""{"_meta":["3f2504e0-4f89-41d3-9a0c-0305e82c3301"]}""")]
	[TestCase("""["3f2504e0-4f89-41d3-9a0c-0305e82c3301"]""")]
	[TestCase("""{"name":"get_category"}""")]
	public async Task RequestSettingsMessageFilter_WithoutValidSuppliedCorrelationId_GeneratesNewGuid_Test(string paramsJson)
	{
		var correlationId = await GetCorrelationIdForParamsAsync(paramsJson);

		Assert.That(Guid.TryParse(correlationId, out _), Is.True);
		Assert.That(correlationId, Is.Not.EqualTo("3f2504e0-4f89-41d3-9a0c-0305e82c3301"));
	}

	[TestCase("""{"arguments":{"correlationId":"3f2504e0-4f89-41d3-9a0c-0305e82c3301"}}""")]
	[TestCase("""{"_meta":{"correlationId":"not-a-guid"},"arguments":{"correlationId":"3f2504e0-4f89-41d3-9a0c-0305e82c3301"}}""")]
	[TestCase("""{"_meta":{"correlationId":"3f2504e0-4f89-41d3-9a0c-0305e82c3301"},"arguments":{"correlationId":"7ea2c7f7-8b46-475d-86f8-7aaaa9e4a35b"}}""")]
	public async Task RequestSettingsMessageFilter_ToolCall_ResolvesSuppliedCorrelationId_Test(string paramsJson)
	{
		var correlationId = await GetCorrelationIdForParamsAsync(paramsJson);

		Assert.That(correlationId, Is.EqualTo("3f2504e0-4f89-41d3-9a0c-0305e82c3301"));
	}

	[Test]
	public async Task RequestSettingsMessageFilter_ArgumentsOnOtherMethod_GeneratesNewGuid_Test()
	{
		var correlationId = await GetCorrelationIdForParamsAsync(
			"""{"arguments":{"correlationId":"3f2504e0-4f89-41d3-9a0c-0305e82c3301"}}""",
			RequestMethods.PromptsGet);

		Assert.That(correlationId, Is.Not.EqualTo("3f2504e0-4f89-41d3-9a0c-0305e82c3301"));
	}

	[TestCase("""{"arguments":{"correlationId":"not-a-guid"}}""")]
	[TestCase("""{"arguments":{"correlationId":null}}""")]
	[TestCase("""{"arguments":"3f2504e0-4f89-41d3-9a0c-0305e82c3301"}""")]
	[TestCase("""{"_meta":{"CorrelationId":"3f2504e0-4f89-41d3-9a0c-0305e82c3301"}}""")]
	public async Task RequestSettingsMessageFilter_WithoutValidArgumentCorrelationId_GeneratesNewGuid_Test(string paramsJson)
	{
		var correlationId = await GetCorrelationIdForParamsAsync(paramsJson);

		Assert.That(Guid.TryParse(correlationId, out _), Is.True);
		Assert.That(correlationId, Is.Not.EqualTo("3f2504e0-4f89-41d3-9a0c-0305e82c3301"));
	}

	[Test]
	public async Task RequestSettingsMessageFilter_WithSuppliedCorrelationId_TagsSuppliedValue_Test()
	{
		const string supplied = "3f2504e0-4f89-41d3-9a0c-0305e82c3301";
		using var activity = new Activity("tools/call").Start();

		await GetCorrelationIdForParamsAsync($$$"""{"_meta":{"correlationId":"{{{supplied}}}"}}""");

		Assert.That(activity.GetTagItem(OtelAttributeNames.CorrelationIdTag), Is.EqualTo(supplied));
	}

	[Test]
	public async Task RequestSettingsMessageFilter_OutgoingResponse_AddsCorrelationIdToMeta_Test()
	{
		accessor.Current = new RequestSettings();
		var result = new JsonObject();
		var response = new JsonRpcResponse { Id = new RequestId(1), Result = result };

		await RequestSettingsMessageFilter.CreateOutgoing((_, _) => Task.CompletedTask)(CreateContext(response), CancellationToken.None);

		Assert.That(result["_meta"]![RequestSettingsMessageFilter.CorrelationIdKey]!.GetValue<string>(), Is.EqualTo(accessor.Current.CorrelationId));
	}

	[Test]
	public async Task RequestSettingsMessageFilter_OutgoingResponse_WithoutRequestSettings_LeavesResultUnchanged_Test()
	{
		var result = new JsonObject();
		var response = new JsonRpcResponse { Id = new RequestId(1), Result = result };

		await RequestSettingsMessageFilter.CreateOutgoing((_, _) => Task.CompletedTask)(CreateContext(response), CancellationToken.None);

		Assert.That(result.ContainsKey("_meta"), Is.False);
	}

	[Test]
	public async Task RequestSettingsMessageFilter_Request_SetsRequestIdFromJsonRpcId_Test()
	{
		RequestSettings? seen = null;

		var handler = RequestSettingsMessageFilter.Create((_, _) =>
		{
			seen = accessor.Current;
			return Task.CompletedTask;
		});
		await handler(CreateContext(new JsonRpcRequest { Id = new RequestId("abc-7"), Method = RequestMethods.ToolsCall }), CancellationToken.None);

		Assert.That(seen!.RequestId, Is.EqualTo("abc-7"));
	}

	[Test]
	public async Task RequestSettingsMessageFilter_Request_SetsCallerNameFromServerClientInfo_Test()
	{
		RequestSettings? seen = null;

		var handler = RequestSettingsMessageFilter.Create((_, _) =>
		{
			seen = accessor.Current;
			return Task.CompletedTask;
		});
		await handler(CreateContext(ToolCall(), new Implementation { Name = "Visual Studio Code", Version = "1.0.0" }), CancellationToken.None);

		Assert.That(seen!.CallerName, Is.EqualTo("Visual Studio Code"));
	}

	[Test]
	public async Task RequestSettingsMessageFilter_InitializeRequest_SetsCallerNameFromParams_Test()
	{
		RequestSettings? seen = null;
		var request = new JsonRpcRequest
		{
			Id = new RequestId(1),
			Method = RequestMethods.Initialize,
			Params = JsonNode.Parse("""{"clientInfo":{"name":"Bruno","version":"1.0.0"}}"""),
		};

		var handler = RequestSettingsMessageFilter.Create((_, _) =>
		{
			seen = accessor.Current;
			return Task.CompletedTask;
		});
		await handler(CreateContext(request), CancellationToken.None);

		Assert.That(seen!.CallerName, Is.EqualTo("Bruno"));
	}

	[Test]
	public async Task RequestSettingsMessageFilter_WithoutClientInfo_LeavesCallerNameNull_Test()
	{
		RequestSettings? seen = null;

		var handler = RequestSettingsMessageFilter.Create((_, _) =>
		{
			seen = accessor.Current;
			return Task.CompletedTask;
		});
		await handler(CreateContext(ToolCall()), CancellationToken.None);

		Assert.That(seen!.CallerName, Is.Null);
	}

	[Test]
	public async Task RequestSettingsMessageFilter_WithCurrentActivity_TagsRequestIdAndCallerName_Test()
	{
		using var activity = new Activity("tools/call").Start();

		var handler = RequestSettingsMessageFilter.Create((_, _) => Task.CompletedTask);
		await handler(CreateContext(ToolCall(), new Implementation { Name = "Visual Studio Code", Version = "1.0.0" }), CancellationToken.None);

		Assert.That(activity.GetTagItem(OtelAttributeNames.RequestIdTag), Is.EqualTo("1"));
		Assert.That(activity.GetTagItem(OtelAttributeNames.CallerNameTag), Is.EqualTo("Visual Studio Code"));
	}

	[Test]
	public async Task RequestSettingsMessageFilter_ToolCall_LogsStartAndEndWithRequestSettings_Test()
	{
		RequestSettings? seen = null;
		var request = new JsonRpcRequest { Id = new RequestId(5), Method = RequestMethods.ToolsCall, Params = JsonNode.Parse("""{"name":"get_category"}""") };

		var handler = RequestSettingsMessageFilter.Create((_, _) =>
		{
			seen = accessor.Current;
			return Task.CompletedTask;
		});
		await handler(CreateContext(request, new Implementation { Name = "Visual Studio Code", Version = "1.0.0" }), CancellationToken.None);

		var entries = FilterLogEntries();
		Assert.That(entries.Select(entry => entry.EventId.Id), Is.EqualTo(new[] { 1000, 1001 }));
		foreach (var entry in entries)
		{
			Assert.That(entry.Level, Is.EqualTo(LogLevel.Information));
			Assert.That(entry.State["McpOperation"], Is.EqualTo("tools/call get_category"));
			Assert.That(entry.State["CorrelationId"], Is.EqualTo(seen!.CorrelationId));
			Assert.That(entry.State["McpRequestId"], Is.EqualTo("5"));
			Assert.That(entry.State["CallerName"], Is.EqualTo("Visual Studio Code"));
		}

		Assert.That(entries[1].State.ContainsKey("ElapsedMilliseconds"), Is.True);
	}

	[Test]
	public async Task RequestSettingsMessageFilter_OtherRequest_LogsMethodAsOperation_Test()
	{
		var handler = RequestSettingsMessageFilter.Create((_, _) => Task.CompletedTask);
		await handler(CreateContext(new JsonRpcRequest { Id = new RequestId(1), Method = RequestMethods.ToolsList }), CancellationToken.None);

		Assert.That(FilterLogEntries().Select(entry => entry.State["McpOperation"]), Is.All.EqualTo(RequestMethods.ToolsList));
	}

	[Test]
	public void RequestSettingsMessageFilter_HandlerThrows_LogsFailureWithException_Test()
	{
		var failure = new InvalidOperationException("Handler failed.");

		var handler = RequestSettingsMessageFilter.Create((_, _) => throw failure);

		Assert.ThrowsAsync<InvalidOperationException>(async () => await handler(CreateContext(ToolCall()), CancellationToken.None));
		var entries = FilterLogEntries();
		Assert.That(entries.Select(entry => entry.EventId.Id), Is.EqualTo(new[] { 1000, 1002 }));
		Assert.That(entries[1].Level, Is.EqualTo(LogLevel.Error));
		Assert.That(entries[1].Exception, Is.SameAs(failure));
	}

	[Test]
	public async Task RequestSettingsMessageFilter_Notification_WritesNoLogEntries_Test()
	{
		var handler = RequestSettingsMessageFilter.Create((_, _) => Task.CompletedTask);
		await handler(CreateContext(new JsonRpcNotification { Method = NotificationMethods.InitializedNotification }), CancellationToken.None);

		Assert.That(FilterLogEntries(), Is.Empty);
	}
}
