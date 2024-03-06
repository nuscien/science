using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Trivial.Net;
using Trivial.Reflection;
using Trivial.Text;

namespace Trivial.Tasks;

/// <summary>
/// The utilities and extensions for JSON operation.
/// </summary>
public static class JsonOperations
{
    internal const string PathProperty = "path";
    internal const string HttpMethodProperty = "httpMethod";
    internal const string HttpErrorProperty = "httpError";

    internal static BaseJsonOperationSchemaHandler SchemaHandler { get; } = new();

    /// <summary>
    /// Creates JSON operation description collection by a given type.
    /// </summary>
    /// <param name="obj">The object to load description.</param>
    /// <param name="handler">The additional handler to control the creation.</param>
    /// <returns>A collection of the JSON operation description.</returns>
    public static IEnumerable<JsonOperationDescription> CreateDescription(object obj, BaseJsonOperationSchemaHandler handler = null)
    {
        if (obj is null) return YieldReturn<JsonOperationDescription>();
        if (obj is Type t) return CreateDescription(t, handler);
        if (obj is JsonOperationApi api) return api.CreateDescription(handler);
        if (obj is IJsonOperationDescriptive desc) return YieldReturn(desc.CreateDescription());
        if (obj is IEnumerable<IJsonOperationDescriptive> col2) return CreateDescription(col2);
        return CreateDescriptionByProperties(obj, handler);
    }

    /// <summary>
    /// Creates JSON operation description collection.
    /// </summary>
    /// <param name="col">A collection of route JSON operation.</param>
    /// <returns>A collection of the JSON operation description.</returns>
    public static IEnumerable<JsonOperationDescription> CreateDescription<T>(this IEnumerable<T> col) where T : IJsonOperationDescriptive
    {
        if (col == null) yield break;
        foreach (var item in col)
        {
            var desc = item?.CreateDescription();
            if (desc != null) yield return desc;
        }
    }

    /// <summary>
    /// Creates a JSON operation.
    /// </summary>
    /// <param name="target">The target object.</param>
    /// <param name="method">The method info.</param>
    /// <param name="schemaHandler">The optional schema handler.</param>
    /// <param name="id">The operation identifier.</param>
    /// <returns>The JSON operation.</returns>
    public static BaseJsonOperation Create(object target, MethodInfo method, BaseJsonOperationSchemaHandler schemaHandler = null, string id = null)
    {
        if (method == null) return null;
        var op = new InternalMethodJsonOperation(target, method, id)
        {
            SchemaHandler = schemaHandler
        };
        return op.IsValid ? op : null;
    }

    /// <summary>
    /// Creates a JSON operation.
    /// </summary>
    /// <param name="target">The target object.</param>
    /// <param name="property">The property info.</param>
    /// <param name="id">The operation identifier.</param>
    /// <returns>The JSON operation.</returns>
    public static BaseJsonOperation Create(object target, PropertyInfo property, string id = null)
    {
        var op = new InternalPropertyJsonOperation(target, property, id);
        return op.IsValid ? op : null;
    }

    /// <summary>
    /// Creates a JSON operation.
    /// </summary>
    /// <typeparam name="TIn">The type of input data.</typeparam>
    /// <typeparam name="TOut">The type of output data.</typeparam>
    /// <param name="handler">The processing handler.</param>
    /// <param name="id">The operation identifier.</param>
    /// <param name="schemaHandler">The optional schema handler.</param>
    /// <returns>The JSON operation.</returns>
    public static BaseJsonOperation Create<TIn, TOut>(Func<TIn, object, CancellationToken, Task<TOut>> handler, string id = null, BaseJsonOperationSchemaHandler schemaHandler = null)
        => handler == null ? null : new InternalJsonOperation<TIn, TOut>(handler, id)
        {
            SchemaHandler = schemaHandler
        };

    /// <summary>
    /// Creates a JSON operation.
    /// </summary>
    /// <typeparam name="TIn">The type of input data.</typeparam>
    /// <typeparam name="TOut">The type of output data.</typeparam>
    /// <param name="handler">The processing handler.</param>
    /// <param name="id">The operation identifier.</param>
    /// <param name="schemaHandler">The optional schema handler.</param>
    /// <returns>The JSON operation.</returns>
    public static BaseJsonOperation Create<TIn, TOut>(Func<TIn, CancellationToken, Task<TOut>> handler, string id = null, BaseJsonOperationSchemaHandler schemaHandler = null)
        => handler == null ? null : new InternalSimpleJsonOperation<TIn, TOut>(handler, id)
        {
            SchemaHandler = schemaHandler
        };

    /// <summary>
    /// Creates a JSON operation.
    /// </summary>
    /// <typeparam name="TIn">The type of input data.</typeparam>
    /// <typeparam name="TOut">The type of output data.</typeparam>
    /// <param name="handler">The processing handler.</param>
    /// <param name="id">The operation identifier.</param>
    /// <param name="schemaHandler">The optional schema handler.</param>
    /// <returns>The JSON operation.</returns>
    public static BaseJsonOperation Create<TIn, TOut>(Func<TIn, object, TOut> handler, string id = null, BaseJsonOperationSchemaHandler schemaHandler = null)
        => handler == null ? null : new InternalSyncJsonOperation<TIn, TOut>(handler, id)
        {
            SchemaHandler = schemaHandler
        };

    /// <summary>
    /// Creates a JSON operation.
    /// </summary>
    /// <typeparam name="TIn">The type of input data.</typeparam>
    /// <typeparam name="TOut">The type of output data.</typeparam>
    /// <param name="handler">The processing handler.</param>
    /// <param name="id">The operation identifier.</param>
    /// <param name="schemaHandler">The optional schema handler.</param>
    /// <returns>The JSON operation.</returns>
    public static BaseJsonOperation Create<TIn, TOut>(Func<TIn, TOut> handler, string id = null, BaseJsonOperationSchemaHandler schemaHandler = null)
        => handler == null ? null : new InternalSimpleSyncJsonOperation<TIn, TOut>(handler, id)
        {
            SchemaHandler = schemaHandler
        };

    /// <summary>
    /// Converts to JSON.
    /// </summary>
    /// <param name="col">The JSON operation description list.</param>
    /// <param name="info">The operations information; or null, if does not add info properties.</param>
    /// <param name="uris">The optional server URIs.</param>
    /// <param name="schemaCol">The optional schema collection.</param>
    /// <returns>A JSON object.</returns>
    public static JsonObjectNode ToJson(this IEnumerable<JsonOperationDescription> col, JsonObjectNode info = null, IEnumerable<Uri> uris = null, JsonNodeSchemaDescriptionCollection schemaCol = null)
        => ToJson(null, col, info, uris);

    /// <summary>
    /// Creates the JSON schema for data result.
    /// </summary>
    /// <param name="data">The data schema.</param>
    /// <returns>The schema.</returns>
    public static JsonObjectSchemaDescription CreateDataResultSchema(JsonObjectSchemaDescription data = null)
    {
        var schema = new JsonObjectSchemaDescription();
        schema.SetStringProperty("message", "The operation status description.");
        schema.SetStringProperty("track", "The operation tracking identifier.");
        schema.SetProperty("data", data ?? new JsonObjectSchemaDescription
        {
            Description = "The result details."
        });
        schema.SetProperty("info", new JsonObjectSchemaDescription
        {
            Description = "Additional information."
        });
        return schema;
    }

    /// <summary>
    /// Creates the JSON schema for error result.
    /// </summary>
    /// <returns>The schema.</returns>
    public static JsonObjectSchemaDescription CreateErrorResultSchema()
    {
        var schema = new JsonObjectSchemaDescription();
        schema.SetStringProperty("message", "The error message.");
        schema.SetStringProperty("track", "The operation tracking identifier.");
        schema.SetProperty("details", new JsonArraySchemaDescription
        {
            Description = "The error information details.",
            DefaultItems = new JsonStringSchemaDescription
            {
                Description = "The error item."
            }
        });
        schema.SetProperty("info", new JsonObjectSchemaDescription
        {
            Description = "Additional information."
        });
        return schema;
    }

    /// <summary>
    /// Sets the properties into the JSON operation description.
    /// </summary>
    /// <param name="desc">The JSON operation description instance to fill properties.</param>
    /// <param name="attr">The additional information.</param>
    public static void SetValue(this JsonOperationDescription desc, JsonOperationPathAttribute attr)
    {
        if (desc?.Data == null || attr == null) return;
        desc.Data.SetValue(PathProperty, attr.Path);
        if (attr.HttpMethod != null) desc.Data.SetValue(HttpMethodProperty, attr.HttpMethod.Method);
    }

    /// <summary>
    /// Converts to JSON.
    /// </summary>
    /// <param name="baseJson">The base JSON object to fill properties.</param>
    /// <param name="col">The JSON operation description list.</param>
    /// <param name="info">The operations information; or null, if does not add info properties.</param>
    /// <param name="uris">The server URIs; or null, if no such information.</param>
    /// <param name="schemaCol">The optional schema collection.</param>
    /// <returns>A JSON object.</returns>
    internal static JsonObjectNode ToJson(JsonObjectNode baseJson, IEnumerable<JsonOperationDescription> col, JsonObjectNode info = null, IEnumerable<Uri> uris = null, JsonNodeSchemaDescriptionCollection schemaCol = null)
    {
        if (col == null) return baseJson;
        var json = baseJson ?? new();
        json.SetValueIfNotNull("info", info);
        if (uris != null) json.SetValue("servers", uris.Select(ele => ele.OriginalString).Where(ele => !string.IsNullOrWhiteSpace(ele)));
        json.SetValue("paths", out JsonObjectNode paths);
        json.SetValue("components", "schemas", out JsonObjectNode schemas);
        var i = 0;
        schemaCol ??= new JsonNodeSchemaDescriptionCollection();
        foreach (var item in col)
        {
            // Schema
            var op = item.Id ?? string.Concat("op-", i);
            if (item.ArgumentSchema == null || item.ResultSchema == null) continue;
            var opReqKey = schemaCol.GetId(item.ArgumentSchema, string.Concat(op, "-req"));
            var opRespKey = schemaCol.GetId(item.ResultSchema, string.Concat(op, "-resp"));
            var opErrKey = item.ErrorSchema != null ? schemaCol.GetId(item.ResultSchema, string.Concat(op, "-err")) : null;

            // Path and contract
            var path = item.Data.TryGetStringValue(PathProperty);
            if (string.IsNullOrWhiteSpace(path)) continue;
            paths.SetValue(path, out JsonObjectNode server);
            var m = item.Data.TryGetStringTrimmedValue(HttpMethodProperty, true)?.ToLowerInvariant() ?? "post";
            server.SetValue(m, out server);
            server.SetValue("operationId", op);
            server.SetValue("description", item.Description);
            server.SetValue("requestBody", out JsonObjectNode requestBody);
            server.SetValue("responses", out JsonObjectNode responseBody);

            // Request
            requestBody.SetValue("required", !(item.Data.TryGetBooleanValue("argsOptional") ?? false));
            requestBody = SetContentSchema(requestBody, string.Concat("#/components/schemas/", opReqKey));

            // Response
            SetResponseContentSchema(responseBody, 200, string.Concat("#/components/schemas/", opRespKey), item.Data.TryGetStringTrimmedValue("httpRespDesc", true) ?? "Successful resposne.");
            var errorResponse = item.Data.TryGetObjectValue(HttpErrorProperty);
            if (errorResponse == null || item.ErrorSchema == null) continue;
            foreach (var errorItem in errorResponse)
            {
                if (!int.TryParse(errorItem.Key, out var code) || errorItem.Value == null || errorItem.Value.ValueKind != JsonValueKind.String) continue;
                var errorDesc = errorResponse.TryGetStringValue(errorItem.Key);
                SetResponseContentSchema(responseBody, code, string.Concat("#/components/schemas/", opErrKey), errorDesc);
            }
        }

        schemaCol.WriteTo(schemas);
        return json;
    }

    internal static string FormatPath(string s)
        => s?.Trim()?.Trim('/', '\\')?.ToLowerInvariant();

    internal static JsonOperationPathAttribute GetJsonDescriptionPath(MemberInfo method)
    {
        try
        {
            var attr = method.GetCustomAttributes<JsonOperationPathAttribute>()?.FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(attr?.Path)) return attr;
        }
        catch (NotSupportedException)
        {
        }
        catch (TypeLoadException)
        {
        }

        return null;
    }

    internal static void UpdatePath(JsonOperationDescription d, JsonOperationPathAttribute path, Type type)
    {
        if (path != null)
        {
            d.Data.SetValueIfNotNull(PathProperty, path.Path);
            if (path.HttpMethod != null) d.Data.SetValue(HttpMethodProperty, path.HttpMethod.Method);
        }
        else if (type != null && !d.Data.ContainsKey(PathProperty))
        {
            d.Data.SetValue(PathProperty, type.Name.Replace('\'', '-').Replace('`', '-').Replace('.', '-').Replace(',', '-'));
        }
    }

    internal static async Task<object> TryGetTaskResult(Task task)
    {
        await task;
        try
        {
            PropertyInfo propertyInfo = task.GetType().GetProperty("Result");
            if (propertyInfo != null && propertyInfo.CanRead) return propertyInfo.GetValue(task, null);
        }
        catch (ArgumentException)
        {
        }
        catch (AmbiguousMatchException)
        {
        }
        catch (TargetException)
        {
        }
        catch (TargetInvocationException)
        {
        }
        catch (TargetParameterCountException)
        {
        }
        catch (MemberAccessException)
        {
        }
        catch (InvalidOperationException)
        {
        }
        catch (NullReferenceException)
        {
        }
        catch (ExternalException)
        {
        }

        return default;
    }

    internal static void Empty()
    {
    }

    internal static JsonObjectNode ToResultJson(object value)
    {
        try
        {
            return JsonObjectNode.ConvertFrom(value);
        }
        catch (JsonException ex)
        {
            if (ex.InnerException is JsonException jsonEx) ex = jsonEx;
            throw new InvalidOperationException("Cannot convert the result to JSON.", ex);
        }
        catch (NotSupportedException ex)
        {
            throw new InvalidOperationException("Cannot convert the result to JSON.", ex);
        }
    }

    internal static T DeserializeArguments<T>(string arguments)
    {
        try
        {
            return JsonSerializer.Deserialize<T>(arguments);
        }
        catch (JsonException ex)
        {
            throw new ArgumentException("The arguments should be in JSON format with required schema.", ex);
        }
        catch (NotSupportedException ex)
        {
            throw new ArgumentException("The target type of arguments does not support for deserializing.", ex);
        }
    }

    internal static object DeserializeArguments(string json, Type type)
    {
        try
        {
            return JsonSerializer.Deserialize(json, type);
        }
        catch (JsonException ex)
        {
            throw new ArgumentException("The arguments should be in JSON format with required schema.", ex);
        }
        catch (NotSupportedException ex)
        {
            throw new ArgumentException("The target type of arguments does not support for deserializing.", ex);
        }
    }

    internal static string SerializeResult(object value)
    {
        try
        {
            return JsonSerializer.Serialize(value);
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException("Serialize result to JSON failed.", ex);
        }
        catch (NotSupportedException ex)
        {
            throw new InvalidOperationException("Serialize result to JSON failed.", ex);
        }
    }

    private static IEnumerable<JsonOperationDescription> CreateDescriptionByProperties(object obj, BaseJsonOperationSchemaHandler handler)
    {
        var type = obj.GetType();
        var props = type.GetProperties();
        var typeName = type.Name.Replace('\'', '-').Replace('`', '-').Replace('.', '-').Replace(',', '-');
        foreach (var prop in props)
        {
            var d = JsonOperationDescription.CreateFromProperty(obj, prop.Name);
            if (d == null || !UpdateOperation(d, prop, string.Concat(typeName, '-', prop.Name))) continue;
            yield return d;
        }

        var more = CreateDescription(type, handler);
        if (more == null) yield break;
        foreach (var item in more)
        {
            yield return item;
        }
    }

    private static JsonObjectNode SetContentSchema(JsonObjectNode source, string reference)
    {
        source.SetValue("content", Web.WebFormat.JsonMIME, out source);
        source.SetValue("schema", out source);
        source.SetValue("$ref", reference);
        return source;
    }

    private static JsonObjectNode SetResponseContentSchema(JsonObjectNode body, int code, string reference, string description)
    {
        body.SetValue(code.ToString("g"), out body);
        body.SetValue("description", description);
        body = SetContentSchema(body, reference);
        return body;
    }

    private static IEnumerable<T> YieldReturn<T>()
    {
        yield break;
    }

    private static IEnumerable<T> YieldReturn<T>(T obj)
    {
        if (obj is not null) yield return obj;
    }

    /// <summary>
    /// Creates JSON operation description collection by a given type.
    /// </summary>
    /// <param name="type">The type defined with the opertion set.</param>
    /// <param name="handler">The additional handler to control the creation.</param>
    /// <returns>A collection of the JSON operation description.</returns>
    private static IEnumerable<JsonOperationDescription> CreateDescription(Type type, BaseJsonOperationSchemaHandler handler = null)
    {
        if (type == null) yield break;
        var methods = type.GetMethods();
        handler ??= SchemaHandler;
        var typeName = type.Name.Replace('\'', '-').Replace('`', '-').Replace('.', '-').Replace(',', '-');
        foreach (var method in methods)
        {
            var d = JsonOperationDescription.Create(method, null, handler);
            if (!UpdateOperation(d, method, string.Concat(typeName, '-', method.Name))) continue;
            handler.OnCreate(method, d);
            yield return d;
        }
    }

    private static bool UpdateOperation(JsonOperationDescription d, MemberInfo member, string fallbackPath)
    {
        if (d == null) return false;
        var description = StringExtensions.GetDescription(member);
        if (!string.IsNullOrWhiteSpace(description)) d.Description = description;
        if (d.Description == null) return false;
        var path = GetJsonDescriptionPath(member);
        if (path == null && !d.Data.ContainsKey(PathProperty)) d.Data.SetValue(PathProperty, fallbackPath);
        else SetValue(d, path);
        return true;
    }
}

/// <summary>
/// The path attribute of JSON operation.
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Property)]
public class JsonOperationPathAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the JsonOperationPathAttribute class.
    /// </summary>
    public JsonOperationPathAttribute()
    {
    }

    /// <summary>
    /// Initializes a new instance of the JsonOperationPathAttribute class.
    /// </summary>
    /// <param name="path">The path.</param>
    public JsonOperationPathAttribute(string path)
    {
        Path = path;
    }

    /// <summary>
    /// Initializes a new instance of the JsonOperationPathAttribute class.
    /// </summary>
    /// <param name="path">The path.</param>
    /// <param name="httpMethod">The HTTP method.</param>
    public JsonOperationPathAttribute(string path, HttpMethod httpMethod)
        : this(path)
    {
        HttpMethod = httpMethod;
    }

    /// <summary>
    /// Gets or sets the path.
    /// </summary>
    public string Path { get; set; }

    /// <summary>
    /// Gets or sets the HTTP method.
    /// </summary>
    public HttpMethod HttpMethod { get; set; }
}

/// <summary>
/// The JSON schema description collection.
/// </summary>
public class JsonNodeSchemaDescriptionCollection
{
    private readonly Dictionary<string, JsonNodeSchemaDescriptionMappingItem> list = new();

    /// <summary>
    /// Sets a JSON schema.
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <param name="value">The JSON schema.</param>
    public void Set(string id, JsonNodeSchemaDescription value)
    {
        if (string.IsNullOrWhiteSpace(id)) return;
        list[id] = new(id, value);
        var col = new List<JsonNodeSchemaDescriptionMappingItem>(list.Values);
        foreach (var item in col)
        {
            if (item.Id == id || !ReferenceEquals(item.Value, value)) continue;
            col.Remove(item);
        }
    }

    /// <summary>
    /// Gets a JSON schema by given identifier.
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <returns>The JSON schema; or null, if does not exist.</returns>
    public JsonNodeSchemaDescription Get(string id)
        => !string.IsNullOrEmpty(id) && list.TryGetValue(id, out var desc) ? desc?.Value : null;

    /// <summary>
    /// Removes the value with the specified identifier from the collection.
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <returns>true if the element is successfully found and removed; otherwise, false. This method returns false if key is not found in the mapping.</returns>
    public bool Remove(string id)
        => list.Remove(id);

    /// <summary>
    /// Removes the value with the specified JSON schema from the collection.
    /// </summary>
    /// <param name="value">The JSON schema to remove.</param>
    /// <returns>true if the element is successfully found and removed; otherwise, false. This method returns false if key is not found in the mapping.</returns>
    public bool Remove(JsonNodeSchemaDescription value)
    {
        if (value == null) return false;
        var col = new List<JsonNodeSchemaDescriptionMappingItem>(list.Values);
        var i = 0;
        foreach (var item in col)
        {
            if (!ReferenceEquals(item.Value, value)) continue;
            col.Remove(item);
            i++;
        }

        return i > 0;
    }

    /// <summary>
    /// Gets the identifier of a JSON schema.
    /// </summary>
    /// <param name="value">The JSON schema.</param>
    /// <param name="id">The identifier.</param>
    /// <returns>The identifier.</returns>
    public string GetId(JsonNodeSchemaDescription value, string id)
    {
        if (string.IsNullOrWhiteSpace(id)) return null;
        if (value == null) return null;
        foreach (var item in list.Values)
        {
            if (ReferenceEquals(item.Value, value)) return item.Id;
        }

        list[id] = new(id, value);
        return id;
    }

    internal void WriteTo(JsonObjectNode schema)
    {
        foreach (var kvp in list)
        {
            var json = kvp.Value.Value?.ToJson();
            if (json != null) schema.SetValue(kvp.Key, json);
        }
    }
}
