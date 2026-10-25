namespace PilotMcpServer.IntegrationTests.Testing.Utilities.OpenApi;

/// <summary>
/// The JSON shape the client actually produces (request) or accepts (response) for a CLR type, described in
/// OpenAPI/JSON Schema terms. A schema is always built for one <see cref="SchemaDirection"/>, because nullability and
/// numeric string handling differ between serialization and deserialization.
/// </summary>
public sealed record ImplementedSchema
{
    /// <summary>The CLR type the schema was built from (with any <see cref="Nullable{T}"/> wrapper removed).</summary>
    public required Type ClrType { get; init; }

    /// <summary>The component (class) name for object schemas; <see langword="null"/> for primitives and arrays.</summary>
    public string? ComponentName { get; init; }

    /// <summary>The JSON Schema base type: integer, number, string, boolean, array or object.</summary>
    public required string BaseType { get; init; }

    /// <summary>Whether a numeric value is also produced/accepted as a JSON string (e.g. <c>[integer, string]</c>).</summary>
    public bool HasStringAlternate { get; init; }

    /// <summary>Whether <c>null</c> may be sent (request) or is accepted (response).</summary>
    public bool IsNullable { get; init; }

    public string? Format { get; init; }

    public string? Description { get; init; }

    /// <summary>The default value as JSON text, when the client supplies one.</summary>
    public string? DefaultJson { get; init; }

    public ImplementedSchema? Items { get; init; }

    public IReadOnlyDictionary<string, ImplementedSchema> Properties { get; init; } = new Dictionary<string, ImplementedSchema>();

    /// <summary>
    /// Request: properties the caller must always supply. Response: properties the deserializer insists are present.
    /// </summary>
    public IReadOnlySet<string> Required { get; init; } = new HashSet<string>();

    /// <summary>The full JSON type set, e.g. <c>null, integer, string</c>.</summary>
    public IReadOnlySet<string> TypeSet
    {
        get
        {
            var types = new HashSet<string>(StringComparer.Ordinal) { BaseType };
            if (HasStringAlternate)
            {
                types.Add("string");
            }

            if (IsNullable)
            {
                types.Add("null");
            }

            return types;
        }
    }
}
