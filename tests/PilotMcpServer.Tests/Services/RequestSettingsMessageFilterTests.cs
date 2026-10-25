using System.Diagnostics;
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
}
