using OpenTelemetry.Exporter;

namespace PilotMcpServer.Configuration;

/// <summary>
/// Configuration for exporting traces, metrics and logs to an OpenTelemetry collector over OTLP. Bound from the
/// "OpenTelemetry" configuration section.
/// </summary>
public sealed class OpenTelemetryOptions
{
	public const string SectionName = "OpenTelemetry";

	/// <summary>When <see langword="false"/>, no OpenTelemetry providers or exporters are registered.</summary>
	public bool Active { get; set; }

	/// <summary>Collector hostname used when not running in Docker.</summary>
	public string Host { get; set; } = "localhost";

	/// <summary>Collector hostname used when <c>RUNNING_IN_DOCKER</c> is <c>true</c>.</summary>
	public string ContainerName { get; set; } = "otel-collector";

	/// <summary>Collector OTLP port: 4317 for <see cref="OtlpExportProtocol.Grpc"/>, 4318 for <see cref="OtlpExportProtocol.HttpProtobuf"/>.</summary>
	public int Port { get; set; } = 4317;

	/// <summary>The OTLP transport protocol.</summary>
	public OtlpExportProtocol Protocol { get; set; } = OtlpExportProtocol.Grpc;

	/// <summary>The <c>service.name</c> resource attribute (the <c>service_name</c> label in Loki).</summary>
	public string ServiceName { get; set; } = "PilotMcpServer";

	/// <summary>Builds the collector base URL, choosing <see cref="ContainerName"/> or <see cref="Host"/>.</summary>
	public Uri GetEndpoint(bool isRunningInDocker) => new($"http://{(isRunningInDocker ? ContainerName : Host)}:{Port}");
}
