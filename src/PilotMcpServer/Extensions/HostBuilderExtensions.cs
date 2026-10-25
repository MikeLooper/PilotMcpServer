using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using PilotMcpServer.Configuration;
using PilotMcpServer.Contracts.Services;
using PilotMcpServer.Services;

namespace PilotMcpServer.Extensions;

/// <summary>
/// Extension methods that build up the host's logging and dependency injection configuration. Extracted from
/// <c>Program.cs</c> so the wiring can be exercised by unit tests.
/// </summary>
public static class HostBuilderExtensions
{
	private const string EmbeddedConfigResourceName = "PilotMcpServer.appsettings.json";

	/// <summary>
	/// Routes all logging to stderr. The MCP stdio transport uses stdout exclusively for JSON-RPC frames, so any
	/// stray write to <see cref="Console.Out"/> (including the default console logger's normal stream) would
	/// corrupt that channel.
	/// </summary>
	public static ILoggingBuilder ConfigureStdioSafeLogging(this ILoggingBuilder logging)
	{
		logging.ClearProviders();
		logging.AddConsole(options => options.LogToStandardErrorThreshold = LogLevel.Trace);
		return logging;
	}

	/// <summary>
	/// Adds the embedded <c>appsettings.json</c> resource as a configuration source. <c>appsettings.json</c> is
	/// only embedded (not copied to the output directory), so it is not found by the default file-based
	/// configuration providers when the process's working directory differs from the build output directory
	/// (e.g. when launched by an external tool/skill). This ensures settings such as <c>Security</c> bind
	/// correctly regardless of the caller's working directory.
	/// </summary>
	public static IConfigurationBuilder AddEmbeddedAppSettings(this IConfigurationBuilder configuration)
	{
		var assembly = typeof(HostBuilderExtensions).Assembly;
		var stream = assembly.GetManifestResourceStream(EmbeddedConfigResourceName)
			?? throw new InvalidOperationException($"Embedded configuration resource '{EmbeddedConfigResourceName}' was not found.");

		// Ownership of the stream is transferred to the configuration provider, which reads it lazily
		// during Build() and disposes it afterwards; it must not be disposed here.
		return configuration.AddJsonStream(stream);
	}

	/// <summary>
	/// Registers the Pilot domain services: the request settings accessor, the security token provider, the Pilot
	/// HTTP client (with its correlation ID and security message handlers), and API selection state.
	/// </summary>
	public static IServiceCollection AddPilotDomainServices(this IServiceCollection services, IConfiguration configuration)
	{
		services.TryAddSingleton<IRequestSettingsAccessor, RequestSettingsAccessor>();
		services.Configure<SecurityOptions>(configuration.GetSection(SecurityOptions.SectionName));
		services.AddHttpClient<ISecurityTokenProvider, SecurityTokenProvider>();
		services.AddTransient<CorrelationIdDelegatingHandler>();
		services.AddTransient<SecurityDelegatingHandler>();
		services
			.AddHttpClient<IPilotHttpClient, PilotHttpClient>()
			.AddHttpMessageHandler<CorrelationIdDelegatingHandler>()
			.AddHttpMessageHandler<SecurityDelegatingHandler>();
		services.AddSingleton<IPilotApiSelection, PilotApiSelectionState>();

		return services;
	}

	/// <summary>
	/// Gives each incoming MCP request (e.g. a tool call) its own <see cref="Models.RequestSettings"/>, so its
	/// correlation ID is shared by the Pilot API requests, spans and log records the request produces.
	/// </summary>
	public static IMcpServerBuilder WithRequestSettings(this IMcpServerBuilder builder)
		=> builder.WithMessageFilters(filters => filters.AddIncomingFilter(RequestSettingsMessageFilter.Create));
}
