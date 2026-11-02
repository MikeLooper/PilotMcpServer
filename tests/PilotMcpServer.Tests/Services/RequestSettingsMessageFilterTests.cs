using System.Diagnostics;
using System.Text.Json.Nodes;
using Microsoft.Extensions.DependencyInjection;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using Moq;
using PilotMcpServer.Contracts.Services;
using PilotMcpServer.Models;
using PilotMcpServer.OpenTelemetry;
using PilotMcpServer.Services;

namespace PilotMcpServer.Tests.Services;

[TestFixture]
public class RequestSettingsMessageFilterTests
{
	private RequestSettingsAccessor accessor = null!;
	private ServiceProvider services = null!;

	[SetUp]
	public void SetUp()
	{
		accessor = new RequestSettingsAccessor { Current = null };
		services = new ServiceCollection().AddSingleton<IRequestSettingsAccessor>(accessor).BuildServiceProvider();
	}

	[TearDown]
	public void TearDown() => services.Dispose();

	private MessageContext CreateContext(JsonRpcMessage message)
	{
		var server = new Mock<McpServer>();
		server.SetupGet(s => s.Services).Returns(services);
		return new MessageContext(server.Object, message);
	}

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
}
