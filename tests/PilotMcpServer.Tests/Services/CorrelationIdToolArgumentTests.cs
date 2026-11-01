using System.IO.Pipelines;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using PilotMcpServer.Contracts.Services;
using PilotMcpServer.Extensions;
using PilotMcpServer.Services;
using PilotMcpServer.Tools;

namespace PilotMcpServer.Tests.Services;

/// <summary>
/// Runs the real MCP server pipeline over in-memory streams, so these cover what a client sees: the tool schemas it
/// lists and the correlation ID a tool call ends up with.
/// </summary>
[TestFixture]
public class CorrelationIdToolArgumentTests
{
	private const string Supplied = "3f2504e0-4f89-41d3-9a0c-0305e82c3301";
	private const string ToolName = "echo_correlation_id";

	private IHost host = null!;
	private StreamWriter writer = null!;
	private StreamReader reader = null!;

	[SetUp]
	public async Task SetUp()
	{
		var clientToServer = new Pipe();
		var serverToClient = new Pipe();
		var accessor = new RequestSettingsAccessor();
		var builder = Host.CreateApplicationBuilder();
		builder.Logging.ClearProviders();
		builder.Services.AddSingleton<IRequestSettingsAccessor>(accessor);
		builder.Services
			.AddMcpServer()
			.WithStreamServerTransport(clientToServer.Reader.AsStream(), serverToClient.Writer.AsStream())
			.WithToolsFromAssembly(typeof(SystemTools).Assembly)
			.WithTools([McpServerTool.Create(() => accessor.Current!.CorrelationId, new McpServerToolCreateOptions { Name = ToolName })])
			.WithRequestSettings();

		host = builder.Build();
		await host.StartAsync();

		writer = new StreamWriter(clientToServer.Writer.AsStream()) { AutoFlush = true };
		reader = new StreamReader(serverToClient.Reader.AsStream());

		await SendAsync("""{"jsonrpc":"2.0","id":0,"method":"initialize","params":{"protocolVersion":"2025-06-18","capabilities":{},"clientInfo":{"name":"test","version":"1"}}}""");
		await writer.WriteLineAsync("""{"jsonrpc":"2.0","method":"notifications/initialized"}""");
	}

	[TearDown]
	public async Task TearDown()
	{
		await host.StopAsync();
		host.Dispose();
		writer.Dispose();
		reader.Dispose();
	}

	private async Task<JsonElement> SendAsync(string message)
	{
		await writer.WriteLineAsync(message);
		var line = await reader.ReadLineAsync().WaitAsync(TimeSpan.FromSeconds(10));
		return JsonDocument.Parse(line!).RootElement.GetProperty("result");
	}

	private async Task<string> CallEchoToolAsync(string paramsWithoutName)
	{
		var result = await SendAsync($$$"""{"jsonrpc":"2.0","id":1,"method":"tools/call","params":{"name":"{{{ToolName}}}",{{{paramsWithoutName}}}}}""");
		return result.GetProperty("content")[0].GetProperty("text").GetString()!;
	}

	[Test]
	public async Task CorrelationIdToolArgument_ToolsList_EveryToolDeclaresOptionalCorrelationId_Test()
	{
		var result = await SendAsync("""{"jsonrpc":"2.0","id":1,"method":"tools/list"}""");
		var tools = result.GetProperty("tools").EnumerateArray().ToArray();

		Assert.That(tools, Has.Length.EqualTo(46));
		foreach (var tool in tools)
		{
			var schema = tool.GetProperty("inputSchema");
			var name = tool.GetProperty("name").GetString();

			var correlationId = schema.GetProperty("properties").GetProperty(RequestSettingsMessageFilter.CorrelationIdKey);
			Assert.That(correlationId.GetProperty("type").GetString(), Is.EqualTo("string"), name);
			Assert.That(correlationId.GetProperty("format").GetString(), Is.EqualTo("uuid"), name);

			var required = schema.TryGetProperty("required", out var values) ? values.EnumerateArray().Select(value => value.GetString()) : [];
			Assert.That(required, Does.Not.Contain(RequestSettingsMessageFilter.CorrelationIdKey), name);
		}
	}

	[Test]
	public async Task CorrelationIdToolArgument_SuppliedAsToolArgument_IsUsedDuringToolCall_Test()
	{
		var correlationId = await CallEchoToolAsync($$"""
			"arguments":{"correlationId":"{{Supplied}}"}
			""");

		Assert.That(correlationId, Is.EqualTo(Supplied));
	}

	[Test]
	public async Task CorrelationIdToolArgument_SuppliedInMeta_IsUsedDuringToolCall_Test()
	{
		var correlationId = await CallEchoToolAsync($$"""
			"arguments":{},"_meta":{"correlationId":"{{Supplied}}"}
			""");

		Assert.That(correlationId, Is.EqualTo(Supplied));
	}

	[Test]
	public async Task CorrelationIdToolArgument_NotSupplied_GeneratesGuidDuringToolCall_Test()
	{
		var correlationId = await CallEchoToolAsync("\"arguments\":{}");

		Assert.That(Guid.TryParse(correlationId, out _), Is.True);
		Assert.That(correlationId, Is.Not.EqualTo(Supplied));
	}
}
