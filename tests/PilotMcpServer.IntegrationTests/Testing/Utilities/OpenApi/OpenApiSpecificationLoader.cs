using Microsoft.OpenApi;
using Microsoft.OpenApi.Reader;
using Microsoft.OpenApi.YamlReader;

namespace PilotMcpServer.IntegrationTests.Testing.Utilities.OpenApi;

/// <summary>
/// Loads the shared Pilot API OpenAPI specification (shared/PilotSharedSource/OpenAPI/PilotApi_v1.yaml), which the test
/// project copies to Testing/Resources in the output directory.
/// </summary>
public static class OpenApiSpecificationLoader
{
    public static string SpecificationPath { get; } = Path.Combine(AppContext.BaseDirectory, "Testing", "Resources", "PilotApi_v1.yaml");

    public static OpenApiDocument Load()
    {
        if (!File.Exists(SpecificationPath))
        {
            throw new FileNotFoundException(
                "The Pilot API OpenAPI specification was not found next to the test assembly. It comes from the " +
                "shared/PilotSharedSource git submodule; run 'git submodule update --init' and rebuild.",
                SpecificationPath);
        }

        var settings = new OpenApiReaderSettings();
        settings.AddYamlReader();

        using var stream = new MemoryStream(File.ReadAllBytes(SpecificationPath));
        var result = OpenApiDocument.Load(stream, OpenApiConstants.Yaml, settings);

        if (result.Diagnostic?.Errors is { Count: > 0 } errors)
        {
            throw new InvalidOperationException(
                $"The OpenAPI specification '{SpecificationPath}' could not be parsed:{Environment.NewLine}" +
                string.Join(Environment.NewLine, errors.Select(error => $"  {error.Pointer}: {error.Message}")));
        }

        return result.Document ?? throw new InvalidOperationException($"The OpenAPI specification '{SpecificationPath}' is empty.");
    }
}
