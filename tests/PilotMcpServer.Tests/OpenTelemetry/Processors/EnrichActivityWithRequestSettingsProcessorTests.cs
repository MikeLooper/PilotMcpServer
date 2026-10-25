using System.Diagnostics;
using PilotMcpServer.Models;
using PilotMcpServer.OpenTelemetry;
using PilotMcpServer.OpenTelemetry.Processors;
using PilotMcpServer.Services;

namespace PilotMcpServer.Tests.OpenTelemetry.Processors;

[TestFixture]
public class EnrichActivityWithRequestSettingsProcessorTests
{
	[Test]
	public void EnrichActivityWithRequestSettingsProcessor_WithCurrentRequestSettings_TagsCorrelationId_Test()
	{
		var accessor = new RequestSettingsAccessor { Current = new RequestSettings() };
		using var activity = new Activity("test");

		new EnrichActivityWithRequestSettingsProcessor(accessor).OnStart(activity);

		Assert.That(activity.GetTagItem(OtelAttributeNames.CorrelationIdTag), Is.EqualTo(accessor.Current.CorrelationId));
	}

	[Test]
	public void EnrichActivityWithRequestSettingsProcessor_WithoutRequestSettings_AddsNoTag_Test()
	{
		var accessor = new RequestSettingsAccessor { Current = null };
		using var activity = new Activity("test");

		new EnrichActivityWithRequestSettingsProcessor(accessor).OnStart(activity);

		Assert.That(activity.GetTagItem(OtelAttributeNames.CorrelationIdTag), Is.Null);
	}
}
