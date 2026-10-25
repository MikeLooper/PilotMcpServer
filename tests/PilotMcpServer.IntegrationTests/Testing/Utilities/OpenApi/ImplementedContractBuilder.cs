using System.Reflection;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.OpenApi;
using ModelContextProtocol.Server;
using PilotMcpServer.Configuration;
using PilotMcpServer.IntegrationTests.Testing.Doubles;
using PilotMcpServer.Models;
using PilotMcpServer.Services;
using DescriptionAttribute = System.ComponentModel.DescriptionAttribute;

namespace PilotMcpServer.IntegrationTests.Testing.Utilities.OpenApi;

/// <summary>
/// Derives the Pilot API contract implemented by this client by static reflection. No calls reach a Pilot API:
/// <list type="bullet">
/// <item>Operations (method, path, tag, summary) come from <see cref="PilotEndpointCatalog"/>.</item>
/// <item>Each operation maps to the MCP tool method named after it (e.g. Categories/GetAll → CategoriesTools.GetAll*);
/// path parameters, query parameters, the request body and the response come from that method's signature, and
/// schemas come from the DTO types read with the client's own <see cref="JsonSerializerOptions"/>.</item>
/// <item>Headers and bearer authentication come from passing a request for each path through the client's
/// correlation ID and security delegating handlers, in memory, into a recording handler.</item>
/// </list>
/// </summary>
public static partial class ImplementedContractBuilder
{
    public const string JsonMediaType = "application/json";

    private const string ApiNameParameterName = "apiName";

    public static ImplementedContract Build()
    {
        var serializerOptions = GetClientSerializerOptions();
        var requestSchemas = new ImplementedSchemaBuilder(serializerOptions, SchemaDirection.Request);
        var responseSchemas = new ImplementedSchemaBuilder(serializerOptions, SchemaDirection.Response);
        var toolTypes = typeof(PilotEndpointCatalog).Assembly.GetTypes()
            .Where(type => type.IsDefined(typeof(McpServerToolTypeAttribute)))
            .ToArray();

        var operations = PilotEndpointCatalog.All
            .Select(endpoint => BuildOperation(endpoint, toolTypes, requestSchemas, responseSchemas))
            .ToArray();

        var authorization = ProbeHandlers("/v1/probe").Headers.Authorization;

        return new ImplementedContract
        {
            ApiVersion = PilotApiCatalog.Catalog.ApiVersion,
            ServerUrls = PilotApiCatalog.All.Select(api => api.BaseUrl).ToArray(),
            SecuritySchemeType = authorization is null ? "none" : "http",
            SecurityScheme = authorization?.Scheme.ToLowerInvariant() ?? "none",
            Operations = operations,
        };
    }

    private static ImplementedOperation BuildOperation(
        EndpointSummary endpoint,
        Type[] toolTypes,
        ImplementedSchemaBuilder requestSchemas,
        ImplementedSchemaBuilder responseSchemas)
    {
        var tool = FindTool(endpoint, toolTypes);
        var pathParameterNames = PathParameterPattern().Matches(endpoint.PathTemplate)
            .Select(match => match.Groups[1].Value)
            .ToHashSet(StringComparer.Ordinal);

        var parameters = new List<ImplementedParameter>();
        ImplementedRequestBody? requestBody = null;

        foreach (var parameter in tool.GetParameters())
        {
            if (parameter.ParameterType == typeof(CancellationToken) || parameter.Name == ApiNameParameterName)
            {
                continue;
            }

            var description = parameter.GetCustomAttribute<DescriptionAttribute>()?.Description;
            var schema = requestSchemas.Build(parameter, description);
            var alwaysSent = !parameter.HasDefaultValue && !schema.IsNullable;

            if (pathParameterNames.Contains(parameter.Name!))
            {
                parameters.Add(new ImplementedParameter
                {
                    Name = parameter.Name!,
                    Location = ParameterLocation.Path,
                    AlwaysSent = true,
                    Schema = schema,
                    Description = description,
                });
            }
            else if (schema.BaseType is "object" or "array")
            {
                requestBody = new ImplementedRequestBody { MediaType = JsonMediaType, Required = alwaysSent, Schema = schema };
            }
            else
            {
                // Remaining scalar tool arguments are appended as query parameters (see ToolBase.WithPaging), which
                // leaves a null argument out of the query string rather than sending a null value.
                parameters.Add(new ImplementedParameter
                {
                    Name = parameter.Name!,
                    Location = ParameterLocation.Query,
                    AlwaysSent = alwaysSent,
                    Schema = schema with { IsNullable = false },
                    Description = description,
                });
            }
        }

        var probedRequest = ProbeHandlers(PathParameterPattern().Replace(endpoint.PathTemplate, "1"));
        foreach (var header in probedRequest.Headers.Where(header => header.Key != "Authorization"))
        {
            var isGuid = header.Value.All(value => Guid.TryParse(value, out _));
            parameters.Add(new ImplementedParameter
            {
                Name = header.Key,
                Location = ParameterLocation.Header,
                AlwaysSent = true,
                Schema = requestSchemas.Build(isGuid ? typeof(Guid) : typeof(string)),
            });
        }

        var (successType, statusOnly) = UnwrapReturnType(tool.ReturnType);
        var returnNullability = new NullabilityInfoContext().Create(tool.ReturnParameter).GenericTypeArguments.FirstOrDefault();

        return new ImplementedOperation
        {
            Method = endpoint.HttpMethod.ToUpperInvariant(),
            Path = endpoint.PathTemplate,
            Tag = endpoint.Resource,
            Summary = endpoint.Summary,
            ToolName = $"{tool.DeclaringType!.Name}.{tool.Name}",
            Parameters = parameters,
            RequestBody = requestBody,
            ResponseMediaType = JsonMediaType,
            SuccessSchema = successType is null ? null : responseSchemas.Build(successType),
            HandlesNotFound = returnNullability?.ReadState == NullabilityState.Nullable,
            ErrorSchema = statusOnly ? null : responseSchemas.Build(typeof(ProblemDetailsResponse)),
            SendsBearerToken = probedRequest.Headers.Authorization is not null,
        };
    }

    /// <summary>
    /// Finds the tool method for an endpoint: "{Resource}Tools" declares a [McpServerTool] method named
    /// "{Operation}…Async" (where "Get…" excludes "GetAll…") or "Get{Operation}Async".
    /// </summary>
    private static MethodInfo FindTool(EndpointSummary endpoint, Type[] toolTypes)
    {
        var toolTypeName = $"{endpoint.Resource}Tools";
        var toolType = toolTypes.SingleOrDefault(type => type.Name == toolTypeName)
            ?? throw new InvalidOperationException($"No [McpServerToolType] class named '{toolTypeName}' implements {endpoint.HttpMethod} {endpoint.PathTemplate}.");

        var candidates = toolType.GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Where(method => method.IsDefined(typeof(McpServerToolAttribute)))
            .Where(method => IsToolForOperation(method.Name, endpoint.Operation))
            .ToArray();

        return candidates.Length == 1
            ? candidates[0]
            : throw new InvalidOperationException(
                $"Expected exactly one [McpServerTool] method on {toolTypeName} for operation '{endpoint.Operation}' " +
                $"({endpoint.HttpMethod} {endpoint.PathTemplate}) but found {candidates.Length}.");
    }

    private static bool IsToolForOperation(string methodName, string operation)
        => methodName == $"Get{operation}Async" ||
            (methodName.StartsWith(operation, StringComparison.Ordinal) &&
             methodName.EndsWith("Async", StringComparison.Ordinal) &&
             !(operation == "Get" && methodName.StartsWith("GetAll", StringComparison.Ordinal)));

    /// <summary>
    /// Returns the type the client deserializes a success body into (<see langword="null"/> when it ignores the body)
    /// and whether the tool only inspects the status code (a <see cref="bool"/> result, as for the health check).
    /// </summary>
    private static (Type? SuccessType, bool StatusOnly) UnwrapReturnType(Type returnType)
    {
        if (!returnType.IsGenericType || returnType.GetGenericTypeDefinition() != typeof(Task<>))
        {
            return (null, false);
        }

        var resultType = returnType.GetGenericArguments()[0];
        return resultType == typeof(bool) ? (null, true) : (resultType, false);
    }

    /// <summary>
    /// Sends a request for <paramref name="path"/> through the client's delegating handlers (in the same order as
    /// HostBuilderExtensions registers them, with security active) and returns the request as it would leave the client.
    /// </summary>
    private static HttpRequestMessage ProbeHandlers(string path)
    {
        var recorder = new RecordingHttpMessageHandler();
        var pipeline = new CorrelationIdDelegatingHandler
        {
            InnerHandler = new SecurityDelegatingHandler(new StaticSecurityTokenProvider("probe-token")) { InnerHandler = recorder },
        };

        using var invoker = new HttpMessageInvoker(pipeline);
        var request = new HttpRequestMessage(HttpMethod.Get, $"{PilotApiCatalog.Default.BaseUrl}{path}");
        using var response = invoker.SendAsync(request, CancellationToken.None).GetAwaiter().GetResult();

        return recorder.LastRequest ?? throw new InvalidOperationException($"The handler pipeline did not forward the request for '{path}'.");
    }

    private static JsonSerializerOptions GetClientSerializerOptions()
        => typeof(PilotHttpClient).GetField("SerializerOptions", BindingFlags.NonPublic | BindingFlags.Static)?.GetValue(null) as JsonSerializerOptions
            ?? throw new InvalidOperationException("Could not read PilotHttpClient.SerializerOptions; the compliance tests need the client's JSON settings.");

    [GeneratedRegex(@"\{(\w+)\}")]
    private static partial Regex PathParameterPattern();
}
