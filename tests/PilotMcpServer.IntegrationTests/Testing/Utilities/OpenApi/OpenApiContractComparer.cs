using Microsoft.OpenApi;

namespace PilotMcpServer.IntegrationTests.Testing.Utilities.OpenApi;

/// <summary>
/// Compares the OpenAPI specification with the contract implemented by the client and classifies every difference.
/// <para>
/// <b>Error</b>: a spec operation is not implemented; a required parameter or request body is missing or its required
/// flag is incompatible; a 2xx response is missing; a request schema or property is missing; a base type differs; any
/// security mismatch; incompatible media type, nullability, required list, format, pattern, default, enum or
/// oneOf/anyOf/allOf.
/// </para>
/// <para>
/// <b>Warn</b>: extra operations, parameters, responses or properties; a missing optional parameter; a response
/// property, schema or body the client does not read; document metadata (info, servers, tags, operationIds).
/// </para>
/// <para>
/// <b>Info</b>: descriptions and examples; alternate types such as [integer, string] vs integer; component names; and
/// differences that are compatible in the direction the data flows (see <see cref="SchemaDirection"/>).
/// </para>
/// </summary>
public sealed class OpenApiContractComparer(OpenApiDocument specification, ImplementedContract implementation)
{
    private const string ComponentSchemaPrefix = "#/components/schemas/";

    private readonly List<ComplianceFinding> findings = [];
    private readonly HashSet<(string ComponentId, SchemaDirection Direction, Type ClrType)> comparedComponents = [];
    private readonly HashSet<string> reachedComponents = new(StringComparer.Ordinal);

    public OpenApiComplianceResult Compare()
    {
        var specOperations = GetSpecOperations();
        var implementedOperations = implementation.Operations.ToDictionary(operation => operation.Key, StringComparer.Ordinal);

        CompareDocument();

        foreach (var (key, (pathItem, operation)) in specOperations)
        {
            if (implementedOperations.TryGetValue(key, out var implemented))
            {
                CompareOperation(key, pathItem, operation, implemented);
            }
            else
            {
                Add(ComplianceSeverity.Error, key, "operation", "The specification defines this operation but the client does not implement it.");
            }
        }

        foreach (var key in implementedOperations.Keys.Where(key => !specOperations.ContainsKey(key)))
        {
            Add(ComplianceSeverity.Warn, key, "operation",
                $"The client implements this operation (tool {implementedOperations[key].ToolName}) but the specification does not define it.");
        }

        var componentIds = specification.Components?.Schemas?.Keys.ToArray() ?? [];
        foreach (var componentId in componentIds.Where(id => !reachedComponents.Contains(id)))
        {
            Add(ComplianceSeverity.Warn, ComponentScope(componentId), "schema",
                "No client type corresponds to this schema (it is only reachable through properties the client does not read, or not referenced at all).");
        }

        return new OpenApiComplianceResult
        {
            Findings = findings.Distinct().OrderByDescending(finding => finding.Severity).ToArray(),
            OperationScopes = specOperations.Keys.Union(implementedOperations.Keys).Order(StringComparer.Ordinal).ToArray(),
            ComponentScopes = componentIds.Select(ComponentScope).ToArray(),
        };
    }

    private Dictionary<string, (IOpenApiPathItem PathItem, OpenApiOperation Operation)> GetSpecOperations()
    {
        var operations = new Dictionary<string, (IOpenApiPathItem, OpenApiOperation)>(StringComparer.Ordinal);
        foreach (var (path, pathItem) in specification.Paths ?? [])
        {
            foreach (var (method, operation) in pathItem.Operations ?? [])
            {
                operations[$"{method.ToString().ToUpperInvariant()} {path}"] = (pathItem, operation);
            }
        }

        return operations;
    }

    private void CompareDocument()
    {
        const string scope = ComplianceFinding.DocumentScope;

        var specVersion = specification.Info?.Version;
        var specMajorVersion = specVersion?.Split('.')[0];
        if (!string.Equals(specMajorVersion, implementation.ApiVersion, StringComparison.Ordinal))
        {
            Add(ComplianceSeverity.Warn, scope, "info.version",
                $"The specification version is '{specVersion}' but the client targets API version '{implementation.ApiVersion}'.");
        }

        var specServers = (specification.Servers ?? []).Select(server => server.Url?.TrimEnd('/') ?? string.Empty).ToArray();
        var unmatchedServers = specServers.Where(url => !implementation.ServerUrls.Contains(url, StringComparer.OrdinalIgnoreCase)).ToArray();
        if (unmatchedServers.Length > 0 || specServers.Length == 0)
        {
            Add(ComplianceSeverity.Warn, scope, "servers",
                $"Specification servers [{string.Join(", ", specServers)}] differ from the client's configured deployments " +
                $"[{string.Join(", ", implementation.ServerUrls)}].");
        }

        var specTags = ((IEnumerable<OpenApiTag>?)specification.Tags ?? []).Select(tag => tag.Name ?? string.Empty).ToHashSet(StringComparer.Ordinal);
        foreach (var tag in specTags.Except(implementation.Tags))
        {
            Add(ComplianceSeverity.Warn, scope, "tags", $"The specification declares tag '{tag}' but no client operation uses it.");
        }

        foreach (var tag in implementation.Tags.Except(specTags))
        {
            Add(ComplianceSeverity.Warn, scope, "tags", $"Client operations use tag '{tag}' but the specification does not declare it.");
        }

        foreach (var (name, scheme) in specification.Components?.SecuritySchemes ?? new Dictionary<string, IOpenApiSecurityScheme>())
        {
            var location = $"components.securitySchemes.{name}";
            var specType = scheme.Type?.ToString() ?? "none";
            if (!string.Equals(specType, implementation.SecuritySchemeType, StringComparison.OrdinalIgnoreCase) ||
                !string.Equals(scheme.Scheme, implementation.SecurityScheme, StringComparison.OrdinalIgnoreCase))
            {
                Add(ComplianceSeverity.Error, scope, location,
                    $"The specification defines a '{specType}' scheme '{scheme.Scheme}' but the client authenticates with " +
                    $"'{implementation.SecuritySchemeType}' scheme '{implementation.SecurityScheme}'.");
            }

            if (!string.IsNullOrEmpty(scheme.BearerFormat))
            {
                Add(ComplianceSeverity.Info, scope, location,
                    $"The specification documents bearerFormat '{scheme.BearerFormat}'; the client passes the identity provider's token through without checking its format.");
            }

            AddDescriptionInfo(scope, location, scheme.Description, null);
        }
    }

    private void CompareOperation(string scope, IOpenApiPathItem pathItem, OpenApiOperation operation, ImplementedOperation implemented)
    {
        var specTags = ((IEnumerable<OpenApiTagReference>?)operation.Tags ?? []).Select(tag => tag.Name ?? string.Empty).ToHashSet(StringComparer.Ordinal);
        if (!specTags.SetEquals([implemented.Tag]))
        {
            Add(ComplianceSeverity.Warn, scope, "tags", $"The specification tags are [{string.Join(", ", specTags)}] but the client groups the operation under '{implemented.Tag}'.");
        }

        if (!string.IsNullOrEmpty(operation.OperationId))
        {
            Add(ComplianceSeverity.Warn, scope, "operationId",
                $"The specification defines operationId '{operation.OperationId}'; the client identifies the operation only by its tool {implemented.ToolName}.");
        }

        AddDescriptionInfo(scope, "summary", operation.Summary, implemented.Summary);
        AddDescriptionInfo(scope, "description", operation.Description, null);

        CompareSecurity(scope, operation, implemented);
        CompareParameters(scope, pathItem, operation, implemented);
        CompareRequestBody(scope, operation.RequestBody, implemented);
        CompareResponses(scope, operation.Responses, implemented);
    }

    private void CompareSecurity(string scope, OpenApiOperation operation, ImplementedOperation implemented)
    {
        var requirements = operation.Security ?? specification.Security ?? [];
        var allowsAnonymous = requirements.Count == 0 || requirements.Any(requirement => requirement.Count == 0);
        var acceptsClientToken = requirements.Any(requirement => requirement.Count > 0 && requirement.Keys.All(scheme =>
            scheme.Type == SecuritySchemeType.Http &&
            string.Equals(implementation.SecuritySchemeType, "http", StringComparison.OrdinalIgnoreCase) &&
            string.Equals(scheme.Scheme, implementation.SecurityScheme, StringComparison.OrdinalIgnoreCase)));
        var specSchemes = string.Join(" or ", requirements.Select(requirement => $"[{string.Join(", ", requirement.Keys.Select(key => key.Reference.Id))}]"));

        if (implemented.SendsBearerToken && !acceptsClientToken && !allowsAnonymous)
        {
            Add(ComplianceSeverity.Error, scope, "security",
                $"The specification requires {specSchemes} but the client authenticates with '{implementation.SecurityScheme}'.");
        }
        else if (!implemented.SendsBearerToken && !allowsAnonymous)
        {
            Add(ComplianceSeverity.Error, scope, "security", $"The specification requires {specSchemes} but the client sends no credentials.");
        }
        else if (implemented.SendsBearerToken && requirements.Count == 0)
        {
            Add(ComplianceSeverity.Error, scope, "security",
                "The specification defines no security for this operation but the client sends a bearer token.");
        }
    }

    private void CompareParameters(string scope, IOpenApiPathItem pathItem, OpenApiOperation operation, ImplementedOperation implemented)
    {
        var specParameters = new Dictionary<(ParameterLocation, string), IOpenApiParameter>();
        foreach (var parameter in (pathItem.Parameters ?? []).Concat(operation.Parameters ?? []))
        {
            specParameters[(parameter.In ?? ParameterLocation.Query, NormalizeParameterName(parameter))] = parameter;
        }

        var implementedParameters = implemented.Parameters.ToDictionary(
            parameter => (parameter.Location, parameter.Location == ParameterLocation.Header ? parameter.Name.ToLowerInvariant() : parameter.Name));

        foreach (var (key, specParameter) in specParameters)
        {
            var location = $"parameter {key.Item1.ToString().ToLowerInvariant()} '{specParameter.Name}'";
            if (!implementedParameters.TryGetValue(key, out var implementedParameter))
            {
                if (specParameter.Required)
                {
                    Add(ComplianceSeverity.Error, scope, location, "The specification requires this parameter but the client never sends it.");
                }
                else
                {
                    Add(ComplianceSeverity.Warn, scope, location, "The specification defines this optional parameter but the client never sends it.");
                }

                continue;
            }

            if (specParameter.Required && !implementedParameter.AlwaysSent)
            {
                Add(ComplianceSeverity.Error, scope, location, "The specification requires this parameter but the client can omit it.");
            }
            else if (!specParameter.Required && implementedParameter.AlwaysSent)
            {
                Add(ComplianceSeverity.Info, scope, location, "The parameter is optional in the specification; the client always sends it (compatible).");
            }

            AddDescriptionInfo(scope, location, specParameter.Description, implementedParameter.Description);
            if (specParameter.Example is not null || specParameter.Examples is { Count: > 0 })
            {
                Add(ComplianceSeverity.Info, scope, location, "The specification provides an example that the client does not represent.");
            }

            if (specParameter.Schema is not null)
            {
                CompareSchema(specParameter.Schema, implementedParameter.Schema, SchemaDirection.Request, scope, location);
            }
        }

        foreach (var (key, implementedParameter) in implementedParameters.Where(pair => !specParameters.ContainsKey(pair.Key)))
        {
            Add(ComplianceSeverity.Warn, scope, $"parameter {key.Item1.ToString().ToLowerInvariant()} '{implementedParameter.Name}'",
                "The client sends this parameter but the specification does not define it.");
        }
    }

    private static string NormalizeParameterName(IOpenApiParameter parameter)
        => parameter.In == ParameterLocation.Header ? (parameter.Name ?? string.Empty).ToLowerInvariant() : parameter.Name ?? string.Empty;

    private void CompareRequestBody(string scope, IOpenApiRequestBody? specBody, ImplementedOperation implemented)
    {
        const string location = "requestBody";
        var implementedBody = implemented.RequestBody;

        if (specBody is null)
        {
            if (implementedBody is not null)
            {
                Add(ComplianceSeverity.Warn, scope, location, "The client sends a request body but the specification does not define one.");
            }

            return;
        }

        if (implementedBody is null)
        {
            Add(specBody.Required ? ComplianceSeverity.Error : ComplianceSeverity.Warn, scope, location,
                $"The specification defines {(specBody.Required ? "a required" : "an optional")} request body but the client never sends one.");
            return;
        }

        if (specBody.Required && !implementedBody.Required)
        {
            Add(ComplianceSeverity.Error, scope, location, "The specification requires a request body but the client can omit it.");
        }
        else if (!specBody.Required && implementedBody.Required)
        {
            Add(ComplianceSeverity.Info, scope, location, "The request body is optional in the specification; the client always sends one (compatible).");
        }

        AddDescriptionInfo(scope, location, specBody.Description, null);

        var content = specBody.Content ?? new Dictionary<string, OpenApiMediaType>();
        if (!content.TryGetValue(implementedBody.MediaType, out var mediaType))
        {
            Add(ComplianceSeverity.Error, scope, location,
                $"The client sends '{implementedBody.MediaType}' but the specification accepts [{string.Join(", ", content.Keys)}].");
            return;
        }

        foreach (var unused in content.Keys.Where(key => key != implementedBody.MediaType))
        {
            Add(ComplianceSeverity.Info, scope, location, $"The specification also accepts '{unused}', which the client does not use.");
        }

        if (mediaType.Schema is not null)
        {
            CompareSchema(mediaType.Schema, implementedBody.Schema, SchemaDirection.Request, scope, location);
        }
    }

    private void CompareResponses(string scope, OpenApiResponses? specResponses, ImplementedOperation implemented)
    {
        var responses = specResponses ?? [];
        var successCodes = responses.Keys.Where(code => code.StartsWith('2')).ToArray();

        if (successCodes.Length == 0)
        {
            Add(ComplianceSeverity.Error, scope, "responses", "The specification defines no 2xx response but the client expects one.");
        }

        // The client treats every 2xx status code as success, so each 2xx response in the specification is handled;
        // what remains is whether the body is read, and read compatibly.
        foreach (var code in successCodes)
        {
            var location = $"response {code}";
            var response = responses[code];
            var content = response.Content ?? new Dictionary<string, OpenApiMediaType>();
            var specSchema = content.TryGetValue(implemented.ResponseMediaType, out var mediaType) ? mediaType.Schema : null;

            if (implemented.SuccessSchema is null)
            {
                if (specSchema is not null)
                {
                    Add(ComplianceSeverity.Warn, scope, location, "The specification defines a response body that the client does not read.");
                }

                continue;
            }

            if (content.Count > 0 && mediaType is null)
            {
                Add(ComplianceSeverity.Error, scope, location,
                    $"The client reads '{implemented.ResponseMediaType}' but the specification returns [{string.Join(", ", content.Keys)}].");
            }
            else if (specSchema is null)
            {
                Add(ComplianceSeverity.Error, scope, location,
                    $"The client expects a {implemented.SuccessSchema.ClrType.Name} body but the specification defines no response schema.");
            }
            else
            {
                CompareSchema(specSchema, implemented.SuccessSchema, SchemaDirection.Response, scope, location);
            }
        }

        if (implemented.HandlesNotFound && !responses.ContainsKey("404") && !responses.ContainsKey("4XX"))
        {
            Add(ComplianceSeverity.Warn, scope, "response 404",
                "The client handles 404 Not Found (returning null) but the specification does not define a 404 response.");
        }

        foreach (var code in responses.Keys.Where(code => !code.StartsWith('2')))
        {
            var location = $"response {code}";
            var content = responses[code].Content ?? new Dictionary<string, OpenApiMediaType>();
            var specSchema = content.TryGetValue(implemented.ResponseMediaType, out var mediaType) ? mediaType.Schema : null;

            if (specSchema is null)
            {
                continue;
            }

            if (implemented.ErrorSchema is null)
            {
                Add(ComplianceSeverity.Warn, scope, location, "The specification defines an error body that the client does not read (it only checks the status code).");
            }
            else if (SpecTypes(Unwrap(specSchema).Schema).SetEquals(["string"]))
            {
                // A plain-text error body; the client falls back to surfacing the raw response text.
            }
            else
            {
                CompareSchema(specSchema, implemented.ErrorSchema, SchemaDirection.Response, scope, location);
            }
        }
    }

    private void CompareSchema(IOpenApiSchema specSchema, ImplementedSchema implemented, SchemaDirection direction, string scope, string location)
    {
        var (spec, nullableViaComposition) = Unwrap(specSchema);
        var componentId = ReferenceId(specSchema) ?? ReferenceId(spec);

        if (componentId is not null)
        {
            reachedComponents.Add(componentId);
            scope = ComponentScope(componentId);
            location = string.Empty;

            if (!comparedComponents.Add((componentId, direction, implemented.ClrType)))
            {
                return;
            }

            if (implemented.ComponentName is not null && implemented.ComponentName != componentId)
            {
                Add(ComplianceSeverity.Info, scope, "schema", $"The specification component '{componentId}' is implemented by client type '{implemented.ComponentName}'.");
            }
        }

        var where = $"{(location.Length == 0 ? "schema" : location)} ({direction.ToString().ToLowerInvariant()})";
        var clientType = implemented.ClrType.Name;

        if (spec.OneOf is { Count: > 0 } || spec.AnyOf is { Count: > 0 } || spec.AllOf is { Count: > 0 })
        {
            Add(ComplianceSeverity.Error, scope, where, $"The specification composes this schema with oneOf/anyOf/allOf; client type {clientType} is a plain {implemented.BaseType}.");
            return;
        }

        if (spec.Enum is { Count: > 0 })
        {
            Add(ComplianceSeverity.Error, scope, where,
                $"The specification restricts values to enum [{string.Join(", ", spec.Enum.Select(value => value?.ToJsonString()))}]; client type {clientType} does not.");
        }

        var specTypes = SpecTypes(spec);
        if (nullableViaComposition)
        {
            specTypes.Add("null");
        }

        if (specTypes.Count == 0 && spec.Properties is { Count: > 0 })
        {
            specTypes.Add("object");
        }

        var specBaseType = BaseType(specTypes);
        if (specBaseType != implemented.BaseType)
        {
            Add(ComplianceSeverity.Error, scope, where,
                $"Base type differs: the specification has [{string.Join(", ", specTypes)}] but client type {clientType} is [{string.Join(", ", implemented.TypeSet)}].");
            return;
        }

        var specAlternates = specTypes.Where(type => type != "null" && type != specBaseType).ToHashSet();
        var implementedAlternates = implemented.TypeSet.Where(type => type != "null" && type != implemented.BaseType).ToHashSet();
        if (!specAlternates.SetEquals(implementedAlternates))
        {
            Add(ComplianceSeverity.Info, scope, where,
                $"Alternate types differ: the specification has [{string.Join(", ", specTypes)}], client type {clientType} has [{string.Join(", ", implemented.TypeSet)}].");
        }

        CompareNullability(specTypes.Contains("null"), implemented, direction, scope, where);

        if (!string.Equals(spec.Format, implemented.Format, StringComparison.OrdinalIgnoreCase))
        {
            Add(ComplianceSeverity.Error, scope, where,
                $"Format differs: the specification has '{spec.Format ?? "(none)"}' but client type {clientType} has '{implemented.Format ?? "(none)"}'.");
        }

        if (spec.Pattern is not null)
        {
            // A pattern only constrains string values. A request is only at risk when the client can send a string;
            // a response is always safe because the client applies no pattern of its own.
            var sendsStrings = implemented.BaseType == "string" || implemented.HasStringAlternate;
            if (direction == SchemaDirection.Request && sendsStrings)
            {
                Add(ComplianceSeverity.Error, scope, where, $"The specification requires pattern '{spec.Pattern}' but the client does not enforce it on the strings it sends.");
            }
            else
            {
                Add(ComplianceSeverity.Info, scope, where,
                    $"The specification pattern '{spec.Pattern}' is not mirrored by the client (compatible: " +
                    (direction == SchemaDirection.Request ? "the client never sends this value as a string)." : "the client accepts any value the pattern allows)."));
            }
        }

        CompareDefault(spec.Default?.ToJsonString(), implemented.DefaultJson, scope, where);

        AddDescriptionInfo(scope, where, spec.Description, implemented.Description);
#pragma warning disable CS0618 // Example is obsolete in favor of Examples, but the YAML reader still populates it from "example".
        if (spec.Example is not null || spec.Examples is { Count: > 0 })
#pragma warning restore CS0618
        {
            Add(ComplianceSeverity.Info, scope, where, "The specification provides an example that the client does not represent.");
        }

        if (specBaseType == "object")
        {
            CompareProperties(spec, implemented, direction, scope, location);
        }
        else if (specBaseType == "array" && spec.Items is not null && implemented.Items is not null)
        {
            CompareSchema(spec.Items, implemented.Items, direction, scope, $"{location}[]");
        }
    }

    private void CompareNullability(bool specNullable, ImplementedSchema implemented, SchemaDirection direction, string scope, string where)
    {
        if (direction == SchemaDirection.Request && implemented.IsNullable && !specNullable)
        {
            Add(ComplianceSeverity.Error, scope, where, $"The client may send null ({implemented.ClrType.Name} is nullable) but the specification does not allow null.");
        }
        else if (direction == SchemaDirection.Response && specNullable && !implemented.IsNullable)
        {
            Add(ComplianceSeverity.Error, scope, where, $"The specification allows null but client type {implemented.ClrType.Name} cannot accept null.");
        }
        else if (specNullable != implemented.IsNullable)
        {
            Add(ComplianceSeverity.Info, scope, where,
                $"Nullability differs (compatible): the specification {(specNullable ? "allows" : "disallows")} null, the client {(implemented.IsNullable ? "allows" : "disallows")} it.");
        }
    }

    private void CompareDefault(string? specDefault, string? implementedDefault, string scope, string where)
    {
        if (specDefault is not null && implementedDefault is not null && specDefault != implementedDefault)
        {
            Add(ComplianceSeverity.Error, scope, where, $"Default differs: the specification has {specDefault} but the client uses {implementedDefault}.");
        }
        else if (specDefault != implementedDefault)
        {
            Add(ComplianceSeverity.Info, scope, where,
                specDefault is not null
                    ? $"The specification default {specDefault} is applied by the server; the client omits the value instead of sending a default."
                    : $"The client defaults to {implementedDefault}; the specification defines no default.");
        }
    }

    private void CompareProperties(IOpenApiSchema spec, ImplementedSchema implemented, SchemaDirection direction, string scope, string location)
    {
        var specProperties = spec.Properties ?? new Dictionary<string, IOpenApiSchema>();
        var specRequired = spec.Required ?? new HashSet<string>();
        string PropertyPath(string name) => location.Length == 0 ? name : $"{location}.{name}";
        string Where(string name) => $"{PropertyPath(name)} ({direction.ToString().ToLowerInvariant()})";

        foreach (var (name, propertySchema) in specProperties)
        {
            if (implemented.Properties.TryGetValue(name, out var implementedProperty))
            {
                CompareSchema(propertySchema, implementedProperty, direction, scope, PropertyPath(name));
            }
            else if (direction == SchemaDirection.Request)
            {
                Add(ComplianceSeverity.Error, scope, Where(name), $"The specification defines this property but client type {implemented.ClrType.Name} never sends it.");
            }
            else
            {
                Add(ComplianceSeverity.Warn, scope, Where(name), $"The specification defines this property but client type {implemented.ClrType.Name} does not read it.");
            }
        }

        foreach (var name in implemented.Properties.Keys.Where(name => !specProperties.ContainsKey(name)))
        {
            Add(ComplianceSeverity.Warn, scope, Where(name), $"Client type {implemented.ClrType.Name} has this property but the specification does not define it.");
        }

        // Request: everything the specification requires must be required of the client's caller.
        // Response: everything the client's deserializer requires must be guaranteed by the specification.
        var mustMatch = direction == SchemaDirection.Request
            ? specRequired.Where(name => implemented.Properties.ContainsKey(name) && !implemented.Required.Contains(name))
            : implemented.Required.Where(name => !specRequired.Contains(name));
        var compatible = direction == SchemaDirection.Request
            ? implemented.Required.Where(name => !specRequired.Contains(name))
            : specRequired.Where(name => implemented.Properties.ContainsKey(name) && !implemented.Required.Contains(name));

        foreach (var name in mustMatch)
        {
            Add(ComplianceSeverity.Error, scope, Where(name), direction == SchemaDirection.Request
                ? $"The specification requires this property but client type {implemented.ClrType.Name} does not."
                : $"Client type {implemented.ClrType.Name} requires this property but the specification does not guarantee it is returned.");
        }

        foreach (var name in compatible)
        {
            Add(ComplianceSeverity.Info, scope, Where(name), direction == SchemaDirection.Request
                ? $"Client type {implemented.ClrType.Name} requires this property; the specification does not (compatible)."
                : $"The specification requires this property; client type {implemented.ClrType.Name} does not (compatible).");
        }
    }

    /// <summary>
    /// Removes the nullable wrapper OpenAPI 3.1 generators emit for references, <c>oneOf: [{type: "null"}, {$ref}]</c>,
    /// returning the inner schema and whether null is allowed.
    /// </summary>
    private static (IOpenApiSchema Schema, bool Nullable) Unwrap(IOpenApiSchema schema)
    {
        foreach (var branches in new[] { schema.OneOf, schema.AnyOf })
        {
            if (branches is { Count: 2 } && branches.Count(IsNullOnly) == 1)
            {
                return (branches.Single(branch => !IsNullOnly(branch)), true);
            }
        }

        return (schema, false);
    }

    private static bool IsNullOnly(IOpenApiSchema schema)
        => schema is not OpenApiSchemaReference && schema.Type == JsonSchemaType.Null && schema.Properties is not { Count: > 0 };

    private static string? ReferenceId(IOpenApiSchema schema) => (schema as OpenApiSchemaReference)?.Reference.Id;

    private static HashSet<string> SpecTypes(IOpenApiSchema schema)
    {
        var types = new HashSet<string>(StringComparer.Ordinal);
        if (schema.Type is not { } type)
        {
            return types;
        }

        foreach (var flag in Enum.GetValues<JsonSchemaType>().Where(flag => type.HasFlag(flag)))
        {
            types.Add(flag.ToString().ToLowerInvariant());
        }

        return types;
    }

    /// <summary>
    /// The primary JSON type of a type set: for <c>[null, integer, string]</c> it is integer (string being the
    /// alternate form of the number); otherwise the single non-null type.
    /// </summary>
    private static string BaseType(IReadOnlySet<string> types)
    {
        var nonNull = types.Where(type => type != "null").ToArray();
        return nonNull.Contains("integer") ? "integer"
            : nonNull.Contains("number") ? "number"
            : nonNull.Length == 1 ? nonNull[0]
            : nonNull.Length == 0 ? "(any)"
            : string.Join("|", nonNull.Order(StringComparer.Ordinal));
    }

    private void AddDescriptionInfo(string scope, string location, string? specDescription, string? implementedDescription)
    {
        if (!string.IsNullOrWhiteSpace(specDescription) && !string.Equals(specDescription.Trim(), implementedDescription?.Trim(), StringComparison.Ordinal))
        {
            Add(ComplianceSeverity.Info, scope, location, implementedDescription is null
                ? $"Specification description not carried by the client: \"{specDescription.Trim()}\""
                : $"Description differs: specification \"{specDescription.Trim()}\" vs client \"{implementedDescription.Trim()}\".");
        }
    }

    private static string ComponentScope(string componentId) => $"{ComponentSchemaPrefix}{componentId}";

    private void Add(ComplianceSeverity severity, string scope, string location, string message)
        => findings.Add(new ComplianceFinding(severity, scope, location, message));
}
