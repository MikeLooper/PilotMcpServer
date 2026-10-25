using OpenTelemetry.Exporter;
using PilotMcpServer.Configuration;

namespace PilotMcpServer.Tests.Configuration;

[TestFixture]
public class OpenTelemetryOptionsTests
{
	[Test]
	public void OpenTelemetryOptions_GetEndpoint_OutsideDocker_UsesHost_Test()
	{
		var options = new OpenTelemetryOptions { Host = "localhost", ContainerName = "otel-collector", Port = 4317 };

		Assert.That(options.GetEndpoint(isRunningInDocker: false), Is.EqualTo(new Uri("http://localhost:4317")));
	}

	[Test]
	public void OpenTelemetryOptions_GetEndpoint_InDocker_UsesContainerName_Test()
	{
		var options = new OpenTelemetryOptions { Host = "localhost", ContainerName = "otel-collector", Port = 4318 };

		Assert.That(options.GetEndpoint(isRunningInDocker: true), Is.EqualTo(new Uri("http://otel-collector:4318")));
	}

	[Test]
	public void OpenTelemetryOptions_Defaults_TargetCollectorGrpcPort_Test()
	{
		var options = new OpenTelemetryOptions();

		Assert.That(options.Active, Is.False);
		Assert.That(options.Port, Is.EqualTo(4317));
		Assert.That(options.Protocol, Is.EqualTo(OtlpExportProtocol.Grpc));
	}
}
