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

	[Test]
	public void EnrichLogsWithRequestSettingsProcessor_WithRequestIdAndCallerName_AddsThem_Test()
	{
		var accessor = new RequestSettingsAccessor { Current = new RequestSettings { RequestId = "7", CallerName = "Visual Studio Code" } };

		var attributes = EmitLog(accessor);

		Assert.That(attributes!.Single(attribute => attribute.Key == OtelAttributeNames.RequestIdAttribute).Value, Is.EqualTo("7"));
		Assert.That(attributes!.Single(attribute => attribute.Key == OtelAttributeNames.CallerNameAttribute).Value, Is.EqualTo("Visual Studio Code"));
	}

	[Test]
	public void EnrichLogsWithRequestSettingsProcessor_WithoutRequestIdOrCallerName_OmitsThem_Test()
	{
		var accessor = new RequestSettingsAccessor { Current = new RequestSettings() };

		var attributes = EmitLog(accessor);

		Assert.That(attributes!.Any(attribute => attribute.Key == OtelAttributeNames.RequestIdAttribute), Is.False);
		Assert.That(attributes!.Any(attribute => attribute.Key == OtelAttributeNames.CallerNameAttribute), Is.False);
	}

	[Test]
	public void EnrichLogsWithRequestSettingsProcessor_AttributeAlreadyOnRecord_IsNotDuplicated_Test()
	{
		var accessor = new RequestSettingsAccessor { Current = new RequestSettings { RequestId = "7" } };

		var attributes = EmitLog(accessor, logger => logger.LogInformation(
			"Started. CorrelationId: {CorrelationId}, McpRequestId: {mcpRequestId}", accessor.Current.CorrelationId, "7"));

		Assert.That(attributes!.Count(attribute => attribute.Key == OtelAttributeNames.CorrelationIdAttribute), Is.EqualTo(1));
		Assert.That(attributes!.Count(attribute => string.Equals(attribute.Key, OtelAttributeNames.RequestIdAttribute, StringComparison.OrdinalIgnoreCase)), Is.EqualTo(1));
	}

	private static IReadOnlyList<KeyValuePair<string, object?>>? EmitLog(RequestSettingsAccessor accessor, Action<ILogger>? write = null)
	{
		var capture = new CapturingLogRecordProcessor();
		using (var loggerFactory = LoggerFactory.Create(builder => builder.AddOpenTelemetry(options =>
		{
			options.AddProcessor(new EnrichLogsWithRequestSettingsProcessor(accessor));
			options.AddProcessor(capture);
		})))
		{
			var logger = loggerFactory.CreateLogger("test");
			if (write is null)
			{
				logger.LogInformation("Calling {ToolName}.", "get_category");
			}
			else
			{
				write(logger);
			}
		}

		return capture.Attributes;
	}
}
