using Microsoft.Extensions.Logging;
using PilotMcpServer.Models;
using PilotMcpServer.OpenTelemetry;
using PilotMcpServer.OpenTelemetry.Processors;
using PilotMcpServer.Services;
using PilotMcpServer.Tests.Testing.Doubles;

namespace PilotMcpServer.Tests.OpenTelemetry.Processors;

[TestFixture]
public class EnrichLogsWithRequestSettingsProcessorTests
{
	[Test]
	public void EnrichLogsWithRequestSettingsProcessor_WithCurrentRequestSettings_AddsCorrelationId_Test()
	{
		var accessor = new RequestSettingsAccessor { Current = new RequestSettings() };

		var attributes = EmitLog(accessor);

		var correlationIds = attributes!.Where(attribute => attribute.Key == OtelAttributeNames.CorrelationIdAttribute).ToArray();
		Assert.That(correlationIds, Has.Length.EqualTo(1));
		Assert.That(correlationIds[0].Value, Is.EqualTo(accessor.Current.CorrelationId));
	}

	[Test]
	public void EnrichLogsWithRequestSettingsProcessor_WithoutRequestSettings_AddsNothing_Test()
	{
		var accessor = new RequestSettingsAccessor { Current = null };

		var attributes = EmitLog(accessor);

		Assert.That(attributes!.Any(attribute => attribute.Key == OtelAttributeNames.CorrelationIdAttribute), Is.False);
	}

	[Test]
	public void EnrichLogsWithRequestSettingsProcessor_KeepsExistingAttributes_Test()
	{
		var accessor = new RequestSettingsAccessor { Current = new RequestSettings() };

		var attributes = EmitLog(accessor);

		Assert.That(attributes!.Any(attribute => attribute.Key == "ToolName" && Equals(attribute.Value, "get_category")), Is.True);
	}

	private static IReadOnlyList<KeyValuePair<string, object?>>? EmitLog(RequestSettingsAccessor accessor)
	{
		var capture = new CapturingLogRecordProcessor();
		using (var loggerFactory = LoggerFactory.Create(builder => builder.AddOpenTelemetry(options =>
		{
			options.AddProcessor(new EnrichLogsWithRequestSettingsProcessor(accessor));
			options.AddProcessor(capture);
		})))
		{
			loggerFactory.CreateLogger("test").LogInformation("Calling {ToolName}.", "get_category");
		}

		return capture.Attributes;
	}
}
