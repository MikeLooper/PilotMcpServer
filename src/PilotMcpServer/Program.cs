using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PilotMcpServer.Extensions;

var builder = Host.CreateApplicationBuilder(args);

builder.Configuration.AddEmbeddedAppSettings();

builder.Logging.ConfigureStdioSafeLogging();

builder.Services.AddPilotDomainServices(builder.Configuration);

// After ConfigureStdioSafeLogging, which clears logger providers (including OpenTelemetry's).
builder.Services.AddPilotOpenTelemetry(builder.Configuration);

builder.Services
	.AddMcpServer(options => options.ServerInfo = McpServerInfoFactory.Create())
	.WithStdioServerTransport()
	.WithToolsFromAssembly()
	.WithRequestSettings();

await builder.Build().RunAsync();
