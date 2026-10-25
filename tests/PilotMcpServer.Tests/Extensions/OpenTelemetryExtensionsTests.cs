using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;
using OpenTelemetry.Trace;
using PilotMcpServer.Configuration;
using PilotMcpServer.Contracts.Services;
using PilotMcpServer.Extensions;

namespace PilotMcpServer.Tests.Extensions;

[TestFixture]
public class OpenTelemetryExtensionsTests
{
	private static IConfiguration CreateConfiguration(bool active)
		=> new ConfigurationBuilder()
			.AddInMemoryCollection(new Dictionary<string, string?> { ["OpenTelemetry:Active"] = active.ToString() })
			.Build();

	[Test]
	public void OpenTelemetryExtensions_AddPilotOpenTelemetry_Inactive_RegistersNoProviders_Test()
	{
		var services = new ServiceCollection();

		services.AddPilotOpenTelemetry(CreateConfiguration(active: false));
		using var provider = services.BuildServiceProvider();

		Assert.That(provider.GetService<TracerProvider>(), Is.Null);
		Assert.That(provider.GetService<LoggerProvider>(), Is.Null);
	}

	[Test]
	public void OpenTelemetryExtensions_AddPilotOpenTelemetry_Active_RegistersProvidersAndAccessor_Test()
	{
		var services = new ServiceCollection();

		services.AddPilotOpenTelemetry(CreateConfiguration(active: true));
		using var provider = services.BuildServiceProvider();

		Assert.That(provider.GetService<TracerProvider>(), Is.Not.Null);
		Assert.That(provider.GetService<LoggerProvider>(), Is.Not.Null);
		Assert.That(provider.GetService<IRequestSettingsAccessor>(), Is.Not.Null);
	}

	[Test]
	public void OpenTelemetryExtensions_AddPilotOpenTelemetry_ReturnsSameServiceCollectionInstance_Test()
	{
		var services = new ServiceCollection();

		var result = services.AddPilotOpenTelemetry(CreateConfiguration(active: false));

		Assert.That(result, Is.SameAs(services));
	}

	[Test]
	public void OpenTelemetryExtensions_AddEmbeddedAppSettings_BindsActiveCollectorSettings_Test()
	{
		var configuration = new ConfigurationBuilder()
			.AddEmbeddedAppSettings()
			.Build();

		var options = configuration.GetSection(OpenTelemetryOptions.SectionName).Get<OpenTelemetryOptions>();

		Assert.That(options!.Active, Is.True);
		Assert.That(options.ContainerName, Is.EqualTo("otel-collector"));
		Assert.That(options.Port, Is.EqualTo(4317));
		Assert.That(options.Protocol, Is.EqualTo(OtlpExportProtocol.Grpc));
	}
}
