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
            Name = "Pilot MCP Server",
            Version = "1.0.0",
        };
    })
    .WithStdioServerTransport()
    .WithToolsFromAssembly();

await builder.Build().RunAsync();
