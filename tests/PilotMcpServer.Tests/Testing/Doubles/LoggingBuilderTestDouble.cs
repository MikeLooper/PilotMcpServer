using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace PilotMcpServer.Tests.Testing.Doubles;

/// <summary>Minimal <see cref="ILoggingBuilder"/> implementation used to test logging configuration extension methods.</summary>
public sealed class LoggingBuilderTestDouble(IServiceCollection services) : ILoggingBuilder
{
	public IServiceCollection Services { get; } = services;
}
