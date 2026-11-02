using Microsoft.Extensions.Logging;

namespace PilotMcpServer.Tests.Testing.Doubles;

/// <summary>One log entry recorded by <see cref="CapturingLoggerProvider"/>.</summary>
public sealed record CapturedLogEntry(
	string Category,
	LogLevel Level,
	EventId EventId,
	string Message,
	IReadOnlyDictionary<string, object?> State,
	Exception? Exception);

/// <summary>Logger provider that records every entry written through its loggers, for asserting on log output.</summary>
public sealed class CapturingLoggerProvider : ILoggerProvider
{
	private readonly List<CapturedLogEntry> entries = [];

	public IReadOnlyList<CapturedLogEntry> Entries
	{
		get
		{
			lock (entries)
			{
				return entries.ToArray();
			}
		}
	}

	public ILogger CreateLogger(string categoryName) => new CapturingLogger(this, categoryName);

	public void Dispose()
	{
	}

	private void Add(CapturedLogEntry entry)
	{
		lock (entries)
		{
			entries.Add(entry);
		}
	}

	private sealed class CapturingLogger(CapturingLoggerProvider provider, string category) : ILogger
	{
		public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

		public bool IsEnabled(LogLevel logLevel) => true;

		public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
		{
			var values = state is IEnumerable<KeyValuePair<string, object?>> pairs
				? pairs.ToDictionary(pair => pair.Key, pair => pair.Value)
				: [];
			provider.Add(new CapturedLogEntry(category, logLevel, eventId, formatter(state, exception), values, exception));
		}
	}
}
