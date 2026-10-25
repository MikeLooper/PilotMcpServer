using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using PilotMcpServer.Configuration;
using PilotMcpServer.Contracts.Services;
using PilotMcpServer.OpenTelemetry.Processors;
using PilotMcpServer.Services;

namespace PilotMcpServer.Extensions;

/// <summary>Registers OpenTelemetry tracing, metrics and logging, exported to an OTLP collector.</summary>
public static class OpenTelemetryExtensions
{
	/// <summary>The ActivitySource and Meter name used by the ModelContextProtocol SDK.</summary>
	public const string McpTelemetrySourceName = "Experimental.ModelContextProtocol";

	/// <summary>
	/// Adds OpenTelemetry when the "OpenTelemetry" configuration section is active. No console exporter is added:
	/// it writes to stdout, which the MCP stdio transport reserves for JSON-RPC frames. Call this after
	/// <see cref="HostBuilderExtensions.ConfigureStdioSafeLogging"/>, which clears all logger providers.
	/// </summary>
	public static IServiceCollection AddPilotOpenTelemetry(this IServiceCollection services, IConfiguration configuration)
	{
		var options = configuration.GetSection(OpenTelemetryOptions.SectionName).Get<OpenTelemetryOptions>() ?? new OpenTelemetryOptions();
		if (!options.Active)
		{
			return services;
		}

		services.TryAddSingleton<IRequestSettingsAccessor, RequestSettingsAccessor>();

		var endpoint = options.GetEndpoint(DeploymentEnvironment.IsRunningInDocker());

		// OTLP/HTTP needs each signal posted to its own path once Endpoint is set explicitly; gRPC uses the base URL.
		void ConfigureExporter(OtlpExporterOptions exporter, string signalPath)
		{
			exporter.Protocol = options.Protocol;
			exporter.Endpoint = options.Protocol == OtlpExportProtocol.HttpProtobuf
				? new Uri(endpoint, signalPath)
				: endpoint;
		}

		services.AddOpenTelemetry()
			.ConfigureResource(resource => resource.AddService(
				serviceName: options.ServiceName,
				serviceVersion: McpServerInfoFactory.ServerVersion,
				autoGenerateServiceInstanceId: false))
			.WithTracing(tracing => tracing				// Grafana Tempo
				.AddSource(McpTelemetrySourceName)
				.AddHttpClientInstrumentation()
				.AddProcessor(provider => new EnrichActivityWithRequestSettingsProcessor(provider.GetRequiredService<IRequestSettingsAccessor>()))
				.AddOtlpExporter(exporter => ConfigureExporter(exporter, "v1/traces")))
			.WithMetrics(metrics => metrics				// Grafana Mimir
				.AddMeter(McpTelemetrySourceName)
				.AddHttpClientInstrumentation()
				.AddOtlpExporter(exporter => ConfigureExporter(exporter, "v1/metrics")))
			.WithLogging(									// Grafana Loki
				logging => logging
					// Enrichment processors must precede the exporter so they run before the record is batched.
					.AddProcessor(provider => new EnrichLogsWithRequestSettingsProcessor(provider.GetRequiredService<IRequestSettingsAccessor>()))
					.AddProcessor(new LogExceptionAttributesProcessor())
					.AddOtlpExporter(exporter => ConfigureExporter(exporter, "v1/logs")),
				logging =>
				{
					logging.IncludeFormattedMessage = true;
					logging.IncludeScopes = true;
				});

		return services;
	}
}
