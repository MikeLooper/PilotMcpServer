using PilotMcpServer.Models;
using PilotMcpServer.Services;

namespace PilotMcpServer.Tests.Services;

[TestFixture]
public class RequestSettingsAccessorTests
{
	[Test]
	public void RequestSettingsAccessor_Current_IsSharedAcrossInstances_Test()
	{
		var settings = new RequestSettings();

		new RequestSettingsAccessor().Current = settings;

		Assert.That(new RequestSettingsAccessor().Current, Is.SameAs(settings));
	}

	[Test]
	public async Task RequestSettingsAccessor_Current_IsIsolatedPerAsyncFlow_Test()
	{
		var accessor = new RequestSettingsAccessor { Current = null };
		var first = new RequestSettings();
		var second = new RequestSettings();

		async Task<RequestSettings?> RunAsync(RequestSettings settings)
		{
			accessor.Current = settings;
			await Task.Yield();
			return accessor.Current;
		}

		var results = await Task.WhenAll(RunAsync(first), RunAsync(second));

		Assert.That(results[0], Is.SameAs(first));
		Assert.That(results[1], Is.SameAs(second));
		Assert.That(accessor.Current, Is.Null);
	}

	[Test]
	public void RequestSettings_CorrelationId_DefaultsToNewGuid_Test()
	{
		var first = new RequestSettings();
		var second = new RequestSettings();

		Assert.That(Guid.TryParse(first.CorrelationId, out _), Is.True);
		Assert.That(first.CorrelationId, Is.Not.EqualTo(second.CorrelationId));
	}
}
