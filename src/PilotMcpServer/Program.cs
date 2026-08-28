using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PilotMcpServer.Services;

var builder = Host.CreateApplicationBuilder(args);

// The MCP stdio transport uses stdout exclusively for JSON-RPC frames. Any stray write to Console.Out
// (including the default console logger's normal stream) would corrupt that channel, so every log
// provider here is routed to stderr instead.
builder.Logging.ClearProviders();
builder.Logging.AddConsole(options => options.LogToStandardErrorThreshold = LogLevel.Trace);

builder.Services.AddHttpClient<IPilotHttpClient, PilotHttpClient>();
builder.Services.AddSingleton<IPilotApiSelection, PilotApiSelectionState>();

builder.Services
	.AddMcpServer(options =>
	{
		options.ServerInfo = new()
		{
			Description = "A custom MCP Server that will read data from any of 6 flavors of Pilot APIs (reading Northwind data from 2 databases)",
			Name = "Pilot MCP Server",
			Title = "Pilot MCP Server",
			Version = "1.0.0",
			WebsiteUrl = "https://github.com/MikeLooper"
		};
	})
	.WithStdioServerTransport()
	.WithToolsFromAssembly();

await builder.Build().RunAsync();
