using OpenTelemetry;
using OpenTelemetry.Logs;

namespace PilotMcpServer.Tests.Testing.Doubles;

/// <summary>Log record processor that records the attributes of the last record it saw.</summary>
public sealed class CapturingLogRecordProcessor : BaseProcessor<LogRecord>
{
	public IReadOnlyList<KeyValuePair<string, object?>>? Attributes { get; private set; }

	public override void OnEnd(LogRecord data)
	{
		Attributes = data.Attributes?.ToList();
	}
}
