using Microsoft.Extensions.Logging;
using PilotMcpServer.OpenTelemetry;
using PilotMcpServer.OpenTelemetry.Processors;
using PilotMcpServer.Tests.Testing.Doubles;

namespace PilotMcpServer.Tests.OpenTelemetry.Processors;

[TestFixture]
public class LogExceptionAttributesProcessorTests
{
	[Test]
	public void LogExceptionAttributesProcessor_NoException_AddsNoExceptionAttributes_Test()
	{
		var attributes = EmitLog(logger => logger.LogInformation("No exception here."));

		var keys = attributes!.Select(attribute => attribute.Key).ToArray();
		Assert.That(keys, Does.Not.Contain(OtelAttributeNames.ExceptionMessage));
		Assert.That(keys, Does.Not.Contain(OtelAttributeNames.ExceptionStacktrace));
		Assert.That(keys, Does.Not.Contain(OtelAttributeNames.ExceptionInnerException));
	}

	[Test]
	public void LogExceptionAttributesProcessor_ThrownException_AddsMessageAndStacktrace_Test()
	{
		var exception = Throw(new InvalidOperationException("A failure occurred."));

		var attributes = ToDictionary(EmitLog(logger => logger.LogError(exception, "Failed.")));

		Assert.That(attributes[OtelAttributeNames.ExceptionMessage], Is.EqualTo(exception.Message));
		Assert.That(attributes[OtelAttributeNames.ExceptionStacktrace], Is.EqualTo(exception.StackTrace));
		Assert.That(attributes, Does.Not.ContainKey(OtelAttributeNames.ExceptionInnerException));
	}

	[Test]
	public void LogExceptionAttributesProcessor_InnerException_AddsInnerExceptionToString_Test()
	{
		var inner = new ArgumentException("The inner failure.");
		var exception = Throw(new InvalidOperationException("A failure occurred.", inner));

		var attributes = ToDictionary(EmitLog(logger => logger.LogError(exception, "Failed.")));

		Assert.That(attributes[OtelAttributeNames.ExceptionInnerException], Is.EqualTo(inner.ToString()));
	}

	[Test]
	public void LogExceptionAttributesProcessor_UnthrownException_OmitsUnavailableStacktrace_Test()
	{
		var exception = new InvalidOperationException("Never thrown, so no stack trace.");

		var attributes = ToDictionary(EmitLog(logger => logger.LogError(exception, "Failed.")));

		Assert.That(attributes[OtelAttributeNames.ExceptionMessage], Is.EqualTo(exception.Message));
		Assert.That(attributes, Does.Not.ContainKey(OtelAttributeNames.ExceptionStacktrace));
	}

	[Test]
	public void LogExceptionAttributesProcessor_AttributeAlreadyRecorded_DoesNotDuplicate_Test()
	{
		var exception = Throw(new InvalidOperationException("A failure occurred."));

		// The structured-logging placeholder name collides (case-insensitively) with Exception.Message.
		var attributes = EmitLog(logger => logger.LogError(exception, "Failed: {exception.message}", "already recorded"));

		var messages = attributes!.Where(attribute => string.Equals(attribute.Key, OtelAttributeNames.ExceptionMessage, StringComparison.OrdinalIgnoreCase)).ToArray();
		Assert.That(messages, Has.Length.EqualTo(1));
		Assert.That(messages[0].Value, Is.EqualTo("already recorded"));
		Assert.That(attributes!.Any(attribute => attribute.Key == OtelAttributeNames.ExceptionStacktrace), Is.True);
	}

	[Test]
	public void LogExceptionAttributesProcessor_NothingToAdd_LeavesAttributesUnchanged_Test()
	{
		var exception = new InvalidOperationException(string.Empty);
		var processor = new LogExceptionAttributesProcessor();
		var capture = new CapturingLogRecordProcessor();
		IReadOnlyList<KeyValuePair<string, object?>>? before = null;

		using (var loggerFactory = LoggerFactory.Create(builder => builder.AddOpenTelemetry(options =>
		{
			options.AddProcessor(new DelegatingLogRecordProcessor(record => before = record.Attributes));
			options.AddProcessor(processor);
			options.AddProcessor(new DelegatingLogRecordProcessor(record => Assert.That(record.Attributes, Is.SameAs(before))));
			options.AddProcessor(capture);
		})))
		{
			// An empty message (no Exception.Message) and no stack trace or inner exception: nothing to add.
			loggerFactory.CreateLogger("test").LogError(exception, "Failed.");
		}

		Assert.That(capture.Attributes, Is.Not.Null);
	}

	private static TException Throw<TException>(TException exception) where TException : Exception
	{
		try
		{
			throw exception;
		}
		catch (TException caught)
		{
			return caught;
		}
	}

	private static Dictionary<string, object?> ToDictionary(IReadOnlyList<KeyValuePair<string, object?>>? attributes)
		=> attributes!.ToDictionary(attribute => attribute.Key, attribute => attribute.Value);

	private static IReadOnlyList<KeyValuePair<string, object?>>? EmitLog(Action<ILogger> log)
	{
		var capture = new CapturingLogRecordProcessor();
		using (var loggerFactory = LoggerFactory.Create(builder => builder.AddOpenTelemetry(options =>
		{
			options.AddProcessor(new LogExceptionAttributesProcessor());
			options.AddProcessor(capture);
		})))
		{
			log(loggerFactory.CreateLogger("test"));
		}

		return capture.Attributes;
	}

	private sealed class DelegatingLogRecordProcessor(Action<global::OpenTelemetry.Logs.LogRecord> onEnd) : global::OpenTelemetry.BaseProcessor<global::OpenTelemetry.Logs.LogRecord>
	{
		public override void OnEnd(global::OpenTelemetry.Logs.LogRecord data) => onEnd(data);
	}
}
