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
        if (obj is IJsonOperationDescriptive desc) return YieldReturn(desc.CreateDescription());
        if (obj is IEnumerable<IJsonOperationDescriptive> col2) return CreateDescription(col2);
        return CreateDescriptionByProperties(obj);
    }

    private static IEnumerable<JsonOperationDescription> CreateDescriptionByProperties(object obj)
    {
        var type = obj.GetType();
        var props = type.GetProperties();
        foreach (var prop in props)
        {
            var d = CreateDescriptionByAttribute(prop);
            if (d == null && TryGetProperty<BaseJsonOperation>(obj, prop, out var op)) d = op.CreateDescription();
            if (!UpdateOperation(d, prop, string.Concat(type.Name, '-', prop.Name))) continue;
            yield return d;
        }

        var more = CreateDescription(type);
        if (more == null) yield break;
        foreach (var item in more)
        {
            yield return item;
        }
    }

    private static bool TryGetProperty<T>(object obj, PropertyInfo prop, out T result)
    {
        try
        {
            if (prop != null && prop.CanRead && !prop.PropertyType.IsSubclassOf(typeof(T)) && prop.GetValue(obj, null) is T r)
            {
                result = r;
                return true;
            }
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

        result = default;
        return false;
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
    /// Converts to JSON.
    /// </summary>
    /// <param name="col">The JSON operation description list.</param>
    /// <param name="info">The operations information.</param>
    /// <param name="uris">The server URIs.</param>
    /// <returns>A JSON object.</returns>
    public static JsonObjectNode ToJson(this IEnumerable<JsonOperationDescription> col, JsonObjectNode info = null, IEnumerable<Uri> uris = null)
    {
        if (col == null) return null;
        var json = new JsonObjectNode();
        json.SetValue("info", info);
        if (uris != null) json.SetValue("servers", uris.Select(ele => ele.OriginalString).Where(ele => !string.IsNullOrWhiteSpace(ele)));
        json.SetValue("paths", out JsonObjectNode paths);
        json.SetValue("components", "schemas", out JsonObjectNode schemas);
        var i = 0;
        foreach (var item in col)
        {
            // Schema
            var op = item.Id ?? string.Concat("op-", i);
            if (item.ArgumentSchema == null || item.ResultSchema == null) continue;
            schemas.SetValue(string.Concat(op, "-req"), item.ArgumentSchema.ToJson());
            schemas.SetValue(string.Concat(op, "-resp"), item.ResultSchema.ToJson());
            if (item.ErrorSchema != null) schemas.SetValue(string.Concat(op, "-err"), item.ErrorSchema.ToJson());

            // Path and contract
            var path = item.Data.TryGetStringValue("path");
            if (string.IsNullOrWhiteSpace(path)) continue;
            paths.SetValue(path, out JsonObjectNode server);
            var m = item.Data.TryGetStringTrimmedValue("httpMethod", true)?.ToLowerInvariant() ?? "post";
            server.SetValue(m, out server);
            server.SetValue("operationId", op);
            server.SetValue("description", item.Description);
            server.SetValue("requestBody", out JsonObjectNode requestBody);
            server.SetValue("responses", out JsonObjectNode responseBody);

            // Request
            requestBody.SetValue("required", !(item.Data.TryGetBooleanValue("argsOptional") ?? false));
            requestBody = SetContentSchema(requestBody, string.Concat("#/components/schemas/", op, "-req"));
            
            // Response
            SetResponseContentSchema(responseBody, 200, string.Concat("#/components/schemas/", op, "-resp"), item.Data.TryGetStringTrimmedValue("httpRespDesc", true) ?? "Successful resposne.");
            var errorResponse = item.Data.TryGetObjectValue("httpError");
            if (errorResponse == null || item.ErrorSchema == null) continue;
            foreach (var errorItem in errorResponse)
            {
                if (!int.TryParse(errorItem.Key, out var code) || errorItem.Value == null || errorItem.Value.ValueKind != JsonValueKind.String) continue;
                var errorDesc = errorResponse.TryGetStringValue(errorItem.Key);
                SetResponseContentSchema(responseBody, code, string.Concat("#/components/schemas/", op, "-err"), errorDesc);
            }
        }

        return json;
    }

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
        desc.Data.SetValue("path", attr.Path);
        if (attr.HttpMethod != null) desc.Data.SetValue("httpMethod", attr.HttpMethod.Method);
    }

    internal static JsonOperationDescription CreateDescriptionByAttribute(MemberInfo member)
    {
        var attr = member?.GetCustomAttributes<JsonOperationDescriptiveAttribute>()?.FirstOrDefault();
        if (attr?.DescriptiveType == null || !attr.DescriptiveType.IsClass) return null;
        if (member is PropertyInfo prop)
        {
            if (TryCreateInstance<IJsonOperationDescriptive<PropertyInfo>>(attr.DescriptiveType, out var d))
                return d.CreateDescription(attr.Id, prop);
        }

        if (member is MethodInfo method)
        {
            if (TryCreateInstance<IJsonOperationDescriptive<MethodInfo>>(attr.DescriptiveType, out var d))
                return d.CreateDescription(attr.Id, method);
        }

        return TryCreateInstance<IJsonOperationDescriptive>(attr.DescriptiveType, out var desc) ? desc.CreateDescription() : null;
    }

    internal static JsonOperationPathAttribute GetJsonDescriptionPath(MemberInfo method)
    {
        try
        {
            var attr = method.GetCustomAttributes<JsonOperationPathAttribute>()?.FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(attr.Path)) return attr;
        }
        catch (NotSupportedException)
        {
        }
        catch (TypeLoadException)
        {
        }

        return null;
    }

    private static bool TryCreateInstance<T>(Type type, out T result)
    {
        try
        {
            if (typeof(T).IsAssignableFrom(type) && Activator.CreateInstance(type) is T r)
            {
                result = r;
                return true;
            }
        }
        catch (ArgumentException)
        {
        }
        catch (NotSupportedException)
        {
        }
        catch (TargetInvocationException)
        {
        }
        catch (MemberAccessException)
        {
        }
        catch (TypeLoadException)
        {
        }
        catch (InvalidComObjectException)
        {
        }
        catch (ExternalException)
        {
        }

        result = default;
        return false;
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
        foreach (var method in methods)
        {
            var d = CreateDescriptionByAttribute(method) ?? JsonOperationDescription.Create(method, null, handler);
            if (!UpdateOperation(d, method, string.Concat(type.Name, '-', method.Name))) continue;
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
        if (path == null && !d.Data.ContainsKey("path")) d.Data.SetValue("path", fallbackPath);
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
