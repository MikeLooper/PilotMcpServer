using Microsoft.Extensions.Logging;
using PilotMcpServer.OpenTelemetry;
using PilotMcpServer.OpenTelemetry.Processors;
using PilotMcpServer.Services;
using PilotMcpServer.Tests.Testing.Doubles;

namespace PilotMcpServer.Tests.OpenTelemetry.Processors;

[TestFixture]
public class EnrichLogsWithServiceInstanceIdProcessorTests
{
	[Test]
	public void EnrichLogsWithServiceInstanceIdProcessor_AddsServiceInstanceId_Test()
	{
		var attributes = EmitLog();

		Assert.That(attributes!.Single(attribute => attribute.Key == "service_instance_id").Value, Is.EqualTo("pilot-mcp-server-dotnet-mssql"));
	}

	[Test]
	public void EnrichLogsWithServiceInstanceIdProcessor_KeepsExistingAttributes_Test()
	{
		var attributes = EmitLog();

		Assert.That(attributes!.Any(attribute => attribute.Key == "ToolName" && Equals(attribute.Value, "get_category")), Is.True);
	}

	private static IReadOnlyList<KeyValuePair<string, object?>>? EmitLog()
	{
		var provider = new ServiceInstanceIdProvider("PilotMcpServer", new PilotApiSelectionState());
		var capture = new CapturingLogRecordProcessor();
		using (var loggerFactory = LoggerFactory.Create(builder => builder.AddOpenTelemetry(options =>
		{
			options.AddProcessor(new EnrichLogsWithServiceInstanceIdProcessor(provider));
			options.AddProcessor(capture);
		})))
		{
			loggerFactory.CreateLogger("test").LogInformation("Calling {ToolName}.", "get_category");
		}

		return capture.Attributes;
	}
}
