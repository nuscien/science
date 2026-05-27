using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Trivial.Reflection;
using Trivial.Text;

namespace Trivial.Tasks;

/// <summary>
/// The interface of JSON schema adjustment handler.
/// </summary>
public interface IJsonNodeSchemaAdjustment
{
    /// <summary>
    /// Adjusts the JSON schema.
    /// </summary>
    /// <param name="type">The type.</param>
    /// <param name="schema">The JSON schema.</param>
    /// <param name="code">The additional code.</param>
    void Adjust(Type type, JsonNodeSchemaDescription schema, string code);
}

/// <summary>
/// The schema creation handler for JSON operation.
/// </summary>
public class BaseJsonOperationSchemaHandler : IJsonNodeSchemaCreationHandler<Type>
{
    /// <summary>
    /// Gets a value indicating whether the integer type is enabled.
    /// </summary>
    public bool IsIntegerEnabled { get; set; }

    /// <summary>
    /// Formats or converts the schema instance by customization.
    /// </summary>
    /// <param name="type">The source type.</param>
    /// <param name="result">The JSON schema created to convert or format.</param>
    /// <param name="breadcrumb">The path breadcrumb.</param>
    /// <returns>The JSON schema of final result.</returns>
    public virtual JsonNodeSchemaDescription Convert(Type type, JsonNodeSchemaDescription result, NodePathBreadcrumb<Type> breadcrumb)
    {
        if (!IsIntegerEnabled) result = result is JsonIntegerSchemaDescription i ? new JsonNumberSchemaDescription(i)
        {
            Tag = i.Tag,
            Description = i.Description,
        } : result;

        try
        {
            var attr = type.GetCustomAttributes<JsonNodeSchemaAdjustmentAttribute>()?.FirstOrDefault();
            if (attr?.AdjustmentType != null)
            {
                var adjustment = Activator.CreateInstance(attr.AdjustmentType) as IJsonNodeSchemaAdjustment;
                adjustment?.Adjust(type, result, attr.Code);
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

        return result;
    }

    /// <summary>
    /// Occurs on the JSON operation description is created.
    /// </summary>
    /// <param name="method">The method info.</param>
    /// <param name="description">The JSON operation description.</param>
    public virtual void OnCreate(MethodInfo method, JsonOperationDescription description)
    {
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
/// The path attribute of JSON operation.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class JsonNodeSchemaAdjustmentAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the JsonNodeSchemaAdjustmentAttribute class.
    /// </summary>
    public JsonNodeSchemaAdjustmentAttribute()
    {
    }

    /// <summary>
    /// Initializes a new instance of the JsonNodeSchemaAdjustmentAttribute class.
    /// </summary>
    /// <param name="type">The type of IJsonNodeSchemaAdjustment.</param>
    /// <param name="code">The additional code.</param>
    public JsonNodeSchemaAdjustmentAttribute(Type type, string code = null)
    {
        AdjustmentType = type;
        Code = code;
    }

    /// <summary>
    /// Gets the type of the JSON schema adjustment.
    /// </summary>
    public Type AdjustmentType { get; }
    
    /// <summary>
    /// Gets the additional code string.
    /// </summary>
    public string Code { get; }
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
