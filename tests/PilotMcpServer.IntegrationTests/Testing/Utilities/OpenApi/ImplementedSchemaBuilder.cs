using System.Collections;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using DescriptionAttribute = System.ComponentModel.DescriptionAttribute;

namespace PilotMcpServer.IntegrationTests.Testing.Utilities.OpenApi;

/// <summary>
/// Describes, by reflection, the JSON a CLR type is serialized to (request) or deserialized from (response) using the
/// client's actual <see cref="JsonSerializerOptions"/>.
/// </summary>
public sealed class ImplementedSchemaBuilder(JsonSerializerOptions serializerOptions, SchemaDirection direction)
{
    private readonly NullabilityInfoContext nullabilityContext = new();

    public SchemaDirection Direction => direction;

    public ImplementedSchema Build(ParameterInfo parameter, string? description = null)
    {
        var defaultJson = parameter.HasDefaultValue && parameter.DefaultValue is not null
            ? JsonSerializer.Serialize(parameter.DefaultValue, serializerOptions)
            : null;

        return Build(parameter.ParameterType, nullabilityContext.Create(parameter), null, description, defaultJson, []);
    }

    public ImplementedSchema Build(Type type) => Build(type, null, null, null, null, []);

    private ImplementedSchema Build(
        Type type,
        NullabilityInfo? nullability,
        JsonNumberHandling? numberHandlingOverride,
        string? description,
        string? defaultJson,
        HashSet<Type> inProgress)
    {
        var underlying = Nullable.GetUnderlyingType(type);
        var clrType = underlying ?? type;
        var isNullable = IsNullable(type, underlying is not null, nullability);
        var numberHandling = numberHandlingOverride ?? serializerOptions.NumberHandling;
        var stringAlternateFlag = direction == SchemaDirection.Request
            ? JsonNumberHandling.WriteAsString
            : JsonNumberHandling.AllowReadingFromString;

        if (TryGetPrimitive(clrType, out var baseType, out var format))
        {
            return new ImplementedSchema
            {
                ClrType = clrType,
                BaseType = baseType,
                Format = format,
                HasStringAlternate = baseType is "integer" or "number" && numberHandling.HasFlag(stringAlternateFlag),
                IsNullable = isNullable,
                Description = description,
                DefaultJson = defaultJson,
            };
        }

        var elementType = GetElementType(clrType);
        if (elementType is not null)
        {
            var elementNullability = nullability?.ElementType ?? nullability?.GenericTypeArguments.FirstOrDefault();
            return new ImplementedSchema
            {
                ClrType = clrType,
                BaseType = "array",
                IsNullable = isNullable,
                Description = description,
                Items = Build(elementType, elementNullability, numberHandlingOverride, null, null, inProgress),
            };
        }

        var properties = new Dictionary<string, ImplementedSchema>(StringComparer.Ordinal);
        var required = new HashSet<string>(StringComparer.Ordinal);

        // A cycle is described as an empty object rather than recursing forever.
        if (inProgress.Add(clrType))
        {
            foreach (var property in clrType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (property.GetMethod is null || property.GetIndexParameters().Length > 0 ||
                    property.GetCustomAttribute<JsonIgnoreAttribute>()?.Condition == JsonIgnoreCondition.Always)
                {
                    continue;
                }

                var name = property.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name
                    ?? serializerOptions.PropertyNamingPolicy?.ConvertName(property.Name)
                    ?? property.Name;

                properties[name] = Build(
                    property.PropertyType,
                    nullabilityContext.Create(property),
                    property.GetCustomAttribute<JsonNumberHandlingAttribute>()?.Handling,
                    property.GetCustomAttribute<DescriptionAttribute>()?.Description,
                    null,
                    inProgress);

                if (property.IsDefined(typeof(RequiredMemberAttribute)) || property.IsDefined(typeof(JsonRequiredAttribute)))
                {
                    required.Add(name);
                }
            }

            inProgress.Remove(clrType);
        }

        return new ImplementedSchema
        {
            ClrType = clrType,
            ComponentName = clrType.Name,
            BaseType = "object",
            IsNullable = isNullable,
            Description = description ?? clrType.GetCustomAttribute<DescriptionAttribute>()?.Description,
            Properties = properties,
            Required = required,
        };
    }

    private bool IsNullable(Type type, bool isNullableValueType, NullabilityInfo? nullability)
    {
        if (type.IsValueType)
        {
            return isNullableValueType;
        }

        // Unless RespectNullableAnnotations is on, System.Text.Json happily deserializes null into any reference type.
        if (direction == SchemaDirection.Response && !serializerOptions.RespectNullableAnnotations)
        {
            return true;
        }

        // Serialization reads the property (getter); deserialization writes it (setter/init).
        var state = direction == SchemaDirection.Request ? nullability?.ReadState : nullability?.WriteState;
        return state is null or NullabilityState.Nullable or NullabilityState.Unknown;
    }

    private static bool TryGetPrimitive(Type type, out string baseType, out string? format)
    {
        (baseType, format) = type switch
        {
            _ when type == typeof(string) || type == typeof(char) => ("string", null),
            _ when type == typeof(bool) => ("boolean", null),
            _ when type == typeof(byte) => ("integer", "uint8"),
            _ when type == typeof(short) => ("integer", "int16"),
            _ when type == typeof(int) => ("integer", "int32"),
            _ when type == typeof(long) => ("integer", "int64"),
            _ when type == typeof(float) => ("number", "float"),
            _ when type == typeof(double) || type == typeof(decimal) => ("number", "double"),
            _ when type == typeof(DateTime) || type == typeof(DateTimeOffset) => ("string", "date-time"),
            _ when type == typeof(DateOnly) => ("string", "date"),
            _ when type == typeof(TimeOnly) => ("string", "time"),
            _ when type == typeof(Guid) => ("string", "uuid"),
            _ when type == typeof(Uri) => ("string", "uri"),
            _ when type == typeof(byte[]) => ("string", "byte"),
            _ when type.IsEnum => ("integer", null),
            _ => (string.Empty, null),
        };

        return baseType.Length > 0;
    }

    private static Type? GetElementType(Type type)
    {
        if (type.IsArray)
        {
            return type.GetElementType();
        }

        if (!typeof(IEnumerable).IsAssignableFrom(type))
        {
            return null;
        }

        var enumerable = type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>)
            ? type
            : type.GetInterfaces().FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>));

        return enumerable?.GetGenericArguments()[0];
    }
}
