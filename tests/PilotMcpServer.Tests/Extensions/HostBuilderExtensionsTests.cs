using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PilotMcpServer.Configuration;
using PilotMcpServer.Contracts.Services;
using PilotMcpServer.Extensions;
using PilotMcpServer.Services;
using PilotMcpServer.Tests.Testing.Doubles;

namespace PilotMcpServer.Tests.Extensions;

[TestFixture]
public class HostBuilderExtensionsTests
{
	[Test]
	public void HostBuilderExtensions_ConfigureStdioSafeLogging_RegistersConsoleLoggerProvider_Test()
	{
		var services = new ServiceCollection();
		var loggingBuilder = new LoggingBuilderTestDouble(services);

		loggingBuilder.ConfigureStdioSafeLogging();

		using var provider = services.BuildServiceProvider();
		var loggerProviders = provider.GetServices<ILoggerProvider>();

		Assert.That(loggerProviders.Any(p => p.GetType().Name.Contains("Console")), Is.True);
	}

	[Test]
	public void HostBuilderExtensions_ConfigureStdioSafeLogging_ReturnsSameBuilderInstance_Test()
	{
		var services = new ServiceCollection();
		var loggingBuilder = new LoggingBuilderTestDouble(services);

		var result = loggingBuilder.ConfigureStdioSafeLogging();

		Assert.That(result, Is.SameAs(loggingBuilder));
	}

	[Test]
	public void HostBuilderExtensions_AddPilotDomainServices_RegistersSecurityTokenProvider_Test()
	{
		var services = new ServiceCollection();
		var configuration = new ConfigurationBuilder().Build();

		services.AddPilotDomainServices(configuration);
		using var provider = services.BuildServiceProvider();

		var tokenProvider = provider.GetService<ISecurityTokenProvider>();

		Assert.That(tokenProvider, Is.Not.Null);
	}

	[Test]
	public void HostBuilderExtensions_AddPilotDomainServices_RegistersPilotHttpClient_Test()
	{
		var services = new ServiceCollection();
		var configuration = new ConfigurationBuilder().Build();

		services.AddPilotDomainServices(configuration);
		using var provider = services.BuildServiceProvider();

		var client = provider.GetService<IPilotHttpClient>();

		Assert.That(client, Is.Not.Null);
	}

	[Test]
	public void HostBuilderExtensions_AddPilotDomainServices_RegistersPilotApiSelectionAsSingleton_Test()
	{
		var services = new ServiceCollection();
		var configuration = new ConfigurationBuilder().Build();

		services.AddPilotDomainServices(configuration);
		using var provider = services.BuildServiceProvider();

		var first = provider.GetService<IPilotApiSelection>();
		var second = provider.GetService<IPilotApiSelection>();

		Assert.That(first, Is.SameAs(second));
	}

	[Test]
	public void HostBuilderExtensions_AddPilotDomainServices_ReturnsSameServiceCollectionInstance_Test()
	{
		var services = new ServiceCollection();
		var configuration = new ConfigurationBuilder().Build();

		var result = services.AddPilotDomainServices(configuration);

		Assert.That(result, Is.SameAs(services));
	}

	[Test]
	public void HostBuilderExtensions_AddEmbeddedAppSettings_BindsSecurityOptionsRegardlessOfWorkingDirectory_Test()
	{
		var configuration = new ConfigurationBuilder()
			.AddEmbeddedAppSettings()
			.Build();

		var security = configuration.GetSection(SecurityOptions.SectionName).Get<SecurityOptions>();

		Assert.That(security, Is.Not.Null);
	}

	[Test]
	public void HostBuilderExtensions_AddEmbeddedAppSettings_BindsIdpHostBaseUrl_Test()
	{
		var configuration = new ConfigurationBuilder()
			.AddEmbeddedAppSettings()
			.Build();

		var security = configuration.GetSection(SecurityOptions.SectionName).Get<SecurityOptions>();

		Assert.That(security!.IdpHostBaseUrl, Is.EqualTo("http://localhost:55001"));
	}

	[Test]
	public void HostBuilderExtensions_AddEmbeddedAppSettings_BindsDefaultApiName_Test()
	{
		var configuration = new ConfigurationBuilder()
			.AddEmbeddedAppSettings()
			.Build();

		var defaultApiName = configuration["defaultApiName"];

		Assert.That(defaultApiName, Is.EqualTo(".NET Core with SQL Server"));
	}

	[Test]
	public void HostBuilderExtensions_AddEmbeddedAppSettings_ReturnsSameConfigurationBuilderInstance_Test()
	{
		var configurationBuilder = new ConfigurationBuilder();

		var result = configurationBuilder.AddEmbeddedAppSettings();

		Assert.That(result, Is.SameAs(configurationBuilder));
	}
}
