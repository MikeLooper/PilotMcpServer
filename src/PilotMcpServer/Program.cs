using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PilotMcpServer.Extensions;

var httpUrl = McpHttpTransport.GetLoopbackUrl(args);

if (httpUrl is null)
{
	var builder = Host.CreateApplicationBuilder(args);
	AddPilotServices(builder).WithStdioServerTransport();
	await builder.Build().RunAsync();
}
else
{
	var builder = WebApplication.CreateBuilder(args);
	builder.WebHost.UseUrls(httpUrl.ToString());
	builder.Configuration[McpHttpTransport.AllowedHostsKey] = McpHttpTransport.AllowedHosts;
	AddPilotServices(builder).WithHttpTransport();

	var app = builder.Build();
	app.MapMcp();
	await app.RunAsync();
}

static ModelContextProtocol.Server.IMcpServerBuilder AddPilotServices(IHostApplicationBuilder builder)
{
	builder.Configuration.AddEmbeddedAppSettings();

	builder.Logging.ConfigureStdioSafeLogging();

	builder.Services.AddPilotDomainServices(builder.Configuration);

	// After ConfigureStdioSafeLogging, which clears logger providers (including OpenTelemetry's).
	builder.Services.AddPilotOpenTelemetry(builder.Configuration);

	return builder.Services
		.AddMcpServer(options => options.ServerInfo = McpServerInfoFactory.Create())
		.WithToolsFromAssembly()
		.WithRequestSettings();
}
