﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Security.Authentication.ExtendedProtection;
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
/// The mapping item for routed JSON API operation.
/// </summary>
public class RoutedJsonOperationMappingItem
{
    /// <summary>
    /// Initializes a new instance of the RoutedJsonOperationMappingItem class.
    /// </summary>
    /// <param name="propertyName">The JSON property name.</param>
    /// <param name="isPropertyPath">true if the property name is a path; otherwise, false.</param>
    /// <param name="argumentName">The argument name for routed API.</param>
    /// <param name="schema">The schema.</param>
    public RoutedJsonOperationMappingItem(string propertyName, bool isPropertyPath, string argumentName, JsonNodeSchemaDescription schema)
    {
        PropertyName = propertyName;
        IsPropertyPath = isPropertyPath;
        ArgumentName = argumentName;
        Schema = schema;
    }

    /// <summary>
    /// Initializes a new instance of the RoutedJsonOperationMappingItem class.
    /// </summary>
    /// <param name="propertyName">The JSON property name.</param>
    /// <param name="argumentName">The argument name for routed API.</param>
    /// <param name="schema">The schema.</param>
    public RoutedJsonOperationMappingItem(string propertyName, string argumentName, JsonNodeSchemaDescription schema)
        : this(propertyName, false, argumentName ?? propertyName, schema)
    {
    }

    /// <summary>
    /// Initializes a new instance of the RoutedJsonOperationMappingItem class.
    /// </summary>
    /// <param name="propertyName">The JSON property name.</param>
    /// <param name="schema">The schema.</param>
    public RoutedJsonOperationMappingItem(string propertyName, JsonNodeSchemaDescription schema)
        : this(propertyName, false, propertyName, schema)
    {
    }

    /// <summary>
    /// Initializes a new instance of the RoutedJsonOperationMappingItem class.
    /// </summary>
    /// <param name="propertyName">The JSON property name.</param>
    /// <param name="isPropertyPath">true if the property name is a path; otherwise, false.</param>
    /// <param name="argumentName">The argument name for routed API.</param>
    /// <param name="description">The description.</param>
    public RoutedJsonOperationMappingItem(string propertyName, bool isPropertyPath, string argumentName, string description = null)
        : this(propertyName, isPropertyPath, argumentName, string.IsNullOrEmpty(description) ? null : new JsonStringSchemaDescription
        {
            Description = description
        })
    {
    }

    /// <summary>
    /// Initializes a new instance of the RoutedJsonOperationMappingItem class.
    /// </summary>
    /// <param name="propertyName">The JSON property name.</param>
    /// <param name="argumentName">The argument name for routed API.</param>
    /// <param name="description">The description.</param>
    public RoutedJsonOperationMappingItem(string propertyName, string argumentName = null, string description = null)
        : this(propertyName, false, argumentName ?? propertyName, description)
    {
    }

    /// <summary>
    /// Gets the JSON property name.
    /// </summary>
    public string PropertyName { get; }

    /// <summary>
    /// Gets a value indicating whether the property name is a path.
    /// </summary>
    public bool IsPropertyPath { get; }

    /// <summary>
    /// Gets the argument name for routed API.
    /// </summary>
    public string ArgumentName { get; }

    /// <summary>
    /// Gets or sets the schema.
    /// </summary>
    public JsonNodeSchemaDescription Schema { get; set; }

    /// <summary>
    /// Gets or sets the description.
    /// </summary>
    public string Description
    {
        get
        {
            return Schema?.Description;
        }

        set
        {
            if (value == null || Schema == null) return;
            try
            {
                Schema ??= new JsonStringSchemaDescription();
                Schema.Description = value;
            }
            catch (NullReferenceException)
            {
            }
        }
    }

    /// <summary>
    /// Gets or sets the tag.
    /// </summary>
    public object Tag { get; set; }

    /// <summary>
    /// Converts the JSON property token data to the argument value.
    /// </summary>
    /// <param name="value">The JSON property token.</param>
    /// <returns>The argument value converted.</returns>
    public virtual string Convert(IJsonValueNode value)
    {
        if (value == null) return null;
        if (value is IJsonValueNode<string> s) return s.Value;
        return value.ValueKind switch
        {
            JsonValueKind.Null or JsonValueKind.Undefined => null,
            _ => value.ToString()
        };
    }

    /// <summary>
    /// Tests to convert an argument.
    /// </summary>
    /// <param name="json">The source JSON object.</param>
    /// <returns>The argument value converted.</returns>
    public string GetArgumentValue(JsonObjectNode json)
    {
        if (json == null) return null;
        var v = json.TryGetValue(PropertyName, IsPropertyPath);
        return Convert(v);
    }
}

/// <summary>
/// The context of routed JSON API operation.
/// </summary>
public class RoutedJsonOperationContext
{
    /// <summary>
    /// Initializes a new instance of the RoutedJsonOperationContext class.
    /// </summary>
    /// <param name="arguments">The arguments.</param>
    public RoutedJsonOperationContext(JsonObjectNode arguments)
    {
        Arguments = arguments;
        Query = new();
    }

    /// <summary>
    /// Initializes a new instance of the RoutedJsonOperationContext class.
    /// </summary>
    /// <param name="arguments">The arguments.</param>
    /// <param name="value">The context value.</param>
    /// <param name="q">The query data.</param>
    public RoutedJsonOperationContext(JsonObjectNode arguments, object value, QueryData q)
    {
        Arguments = arguments;
        Value = value;
        Query = q ?? new();
    }

    /// <summary>
    /// Gets the context value.
    /// </summary>
    public object Value { get; }

    /// <summary>
    /// Gets or sets the tag.
    /// </summary>
    public object Tag { get; set; }

    /// <summary>
    /// Gets the date time when the operation starts.
    /// </summary>
    public DateTime ProcessTime { get; } = DateTime.Now;

    /// <summary>
    /// Gets the arguments.
    /// </summary>
    public JsonObjectNode Arguments { get; }

    /// <summary>
    /// Gets the query data.
    /// </summary>
    public QueryData Query { get; }

    /// <summary>
    /// Gets the additional store data.
    /// </summary>
    public JsonObjectNode Data { get; } = new();

    /// <summary>
    /// Gets the type of the context value.
    /// </summary>
    /// <returns>The type of the context value.</returns>
    public Type GetValueType()
        => Value?.GetType();
}

/// <summary>
/// The base JSON API operation of routed web API.
/// </summary>
public class BaseRoutedJsonOperation : BaseJsonOperation
{
    private readonly Dictionary<string, RoutedJsonOperationMappingItem> dict = new();

    /// <summary>
    /// Initializes a new instance of the BaseRoutedJsonOperation class.
    /// </summary>
    /// <param name="id">The operation identifier.</param>
    /// <param name="description">The operation description.</param>
    public BaseRoutedJsonOperation(string id, string description = null)
    {
        Id = id;
        Description = description;
    }

    /// <summary>
    /// Initializes a new instance of the BaseRoutedJsonOperation class.
    /// </summary>
    /// <param name="uri">The URI of the web API.</param>
    /// <param name="id">The operation identifier.</param>
    /// <param name="description">The operation description.</param>
    public BaseRoutedJsonOperation(Uri uri, string id, string description = null) : this(id, description)
    {
        WebApiUri = uri;
    }

    /// <summary>
    /// Gets or sets the operation identifier.
    /// </summary>
    public string Id { get; }

    /// <summary>
    /// Gets or sets the base URI of the web API.
    /// </summary>
    public Uri WebApiUri { get; set; }

    /// <summary>
    /// Gets or sets the HTTP method.
    /// </summary>
    public HttpMethod HttpMethod { get; set; }

    /// <summary>
    /// Gets or sets the optional operation description.
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// Gets or sets the result schema description.
    /// </summary>
    public JsonNodeSchemaDescription ResultSchema { get; set; }

    /// <summary>
    /// Gets or sets the error schema description.
    /// </summary>
    public JsonNodeSchemaDescription ErrorSchema { get; set; }

    /// <summary>
    /// Gets the error codes supported and their description.
    /// These are following error schema.
    /// </summary>
    public Dictionary<int, string> ErrorCodes { get; } = new();

    /// <summary>
    /// Registers.
    /// </summary>
    /// <param name="item">The mapping item.</param>
    public void Register(RoutedJsonOperationMappingItem item)
    {
        if (string.IsNullOrWhiteSpace(item?.PropertyName)) return;
        dict[item.PropertyName] = item;
    }

    /// <summary>
    /// Registers.
    /// </summary>
    /// <param name="propertyName">The JSON property name.</param>
    /// <param name="isPropertyPath">true if the property name is a path; otherwise, false.</param>
    /// <param name="argumentName">The argument name for routed API.</param>
    /// <param name="schema">The schema.</param>
    /// <returns>The mapping item.</returns>
    public RoutedJsonOperationMappingItem Register(string propertyName, bool isPropertyPath, string argumentName, JsonNodeSchemaDescription schema = null)
    {
        if (string.IsNullOrWhiteSpace(propertyName)) return null;
        var item = new RoutedJsonOperationMappingItem(propertyName, isPropertyPath, argumentName, schema);
        Register(item);
        return item;
    }

    /// <summary>
    /// Registers.
    /// </summary>
    /// <param name="propertyName">The JSON property name.</param>
    /// <param name="argumentName">The argument name for routed API.</param>
    /// <param name="schema">The schema.</param>
    /// <returns>The mapping item.</returns>
    public RoutedJsonOperationMappingItem Register(string propertyName, string argumentName = null, JsonNodeSchemaDescription schema = null)
    {
        if (string.IsNullOrWhiteSpace(propertyName)) return null;
        var item = new RoutedJsonOperationMappingItem(propertyName, argumentName, schema);
        Register(item);
        return item;
    }

    /// <summary>
    /// Registers.
    /// </summary>
    /// <param name="propertyName">The JSON property name.</param>
    /// <param name="isPropertyPath">true if the property name is a path; otherwise, false.</param>
    /// <param name="argumentName">The argument name for routed API.</param>
    /// <param name="description">The description.</param>
    /// <returns>The mapping item.</returns>
    public RoutedJsonOperationMappingItem Register(string propertyName, bool isPropertyPath, string argumentName, string description = null)
    {
        if (string.IsNullOrWhiteSpace(propertyName)) return null;
        var item = new RoutedJsonOperationMappingItem(propertyName, isPropertyPath, argumentName, description);
        Register(item);
        return item;
    }

    /// <summary>
    /// Registers.
    /// </summary>
    /// <param name="propertyName">The JSON property name.</param>
    /// <param name="argumentName">The argument name for routed API.</param>
    /// <param name="description">The description.</param>
    /// <returns>The mapping item.</returns>
    public RoutedJsonOperationMappingItem Register(string propertyName, string argumentName = null, string description = null)
    {
        if (string.IsNullOrWhiteSpace(propertyName)) return null;
        var item = new RoutedJsonOperationMappingItem(propertyName, argumentName, description);
        Register(item);
        return item;
    }

    /// <summary>
    /// Removes the value with the specified key from the mapping.
    /// </summary>
    /// <param name="propertyName">The property name.</param>
    /// <returns>true if the element is successfully found and removed; otherwise, false. This method returns false if key is not found in the mapping.</returns>
    public bool Remove(string propertyName)
        => propertyName != null && dict.Remove(propertyName);

    /// <summary>
    /// Gets the specific mapping item.
    /// </summary>
    /// <param name="propertyName">The property name.</param>
    /// <returns>The mapping item; or null, if non-exist.</returns>
    public RoutedJsonOperationMappingItem Get(string propertyName)
        => propertyName != null && dict.TryGetValue(propertyName, out var item) ? item : null;

    /// <summary>
    /// Tests if has the specific mapping item.
    /// </summary>
    /// <param name="propertyName">The property name.</param>
    /// <returns>true if contains; otherwise, false.</returns>
    public bool Contains(string propertyName)
        => propertyName != null && dict.TryGetValue(propertyName, out _);

    /// <summary>
    /// Tests if has the specific mapping item.
    /// </summary>
    /// <param name="item">The property to test.</param>
    /// <returns>true if contains; otherwise, false.</returns>
    public bool Contains(RoutedJsonOperationMappingItem item)
        => !string.IsNullOrEmpty(item?.PropertyName) && Get(item.PropertyName) == item;

    /// <summary>
    /// Processes.
    /// </summary>
    /// <param name="json">The input data.</param>
    /// <param name="contextValue">The context value.</param>
    /// <param name="cancellationToken">The optional cancellation token.</param>
    /// <returns>The response.</returns>
    /// <exception cref="ArgumentNullException">json was null.</exception>
    /// <exception cref="InvalidOperationException">The URI of the Web API was null or invalid; or the data is invalid.</exception>
    /// <exception cref="FailedHttpException">HTTP failure.</exception>
    /// <exception cref="JsonException">The response content is not JSON.</exception>
    public override async Task<JsonObjectNode> ProcessAsync(JsonObjectNode json, object contextValue, CancellationToken cancellationToken = default)
    {
        if (json == null) throw new ArgumentNullException(nameof(json), "json was null.");
        var uri = WebApiUri ?? throw new InvalidOperationException("The URI of the Web API does not configured.", new ArgumentNullException(nameof(WebApiUri), "The URI of the Web API was null."));
        var q = new QueryData();
        var context = new RoutedJsonOperationContext(json, contextValue, q);
        OnQueryDataInit(q, context);
        foreach (var kvp in dict)
        {
            var converter = kvp.Value;
            if (string.IsNullOrWhiteSpace(converter?.ArgumentName)) continue;
            q[converter.ArgumentName] = converter.GetArgumentValue(json);
        }

        OnQueryDataFill(q, context);
        var url = q.ToString(uri);
        url = FormatUrl(url, context);
        if (url == null) throw new InvalidOperationException("The URI of the Web API or the JSON input data is invalid.");
        var http = CreateHttpClient(context) ?? new();
        JsonObjectNode resp;
        try
        {
            resp = await SendAsync(http, url, context, cancellationToken);
        }
        catch (JsonException ex)
        {
            OnJsonParsingFailure(ex, context);
            throw;
        }
        catch (NotSupportedException ex)
        {
            OnJsonParsingFailure(ex, context);
            throw;
        }
        catch (FailedHttpException ex)
        {
            resp = OnHttpFailure(ex.StatusCode, ex, context);
            if (resp != null) return resp;
            throw;
        }
        catch (HttpRequestException ex)
        {
            var ex2 = new FailedHttpException(null, ex.Message, ex);
            resp = OnHttpFailure(ex2.StatusCode, ex2, context);
            if (resp != null) return resp;
            throw ex2;
        }

        resp = await ProcessResponseAsync(resp, context, cancellationToken);
        resp = ProcessResponse(resp, context);
        return resp;
    }

    /// <summary>
    /// Creates operation description.
    /// </summary>
    /// <returns>The operation description.</returns>
    public override JsonOperationDescription CreateDescription()
    {
        var type = GetType();
        var desc = new JsonOperationDescription()
        {
            Id = Id ?? (type == typeof(BaseRoutedJsonOperation) ? null : type.Name),
            Description = Description ?? StringExtensions.GetDescription(type),
            ArgumentSchema = CreateArgumentSchema(),
            ResultSchema = ResultSchema,
            ErrorSchema = ErrorSchema,
        };
        desc.Data.SetValue(JsonOperations.HttpErrorProperty, out JsonObjectNode errors);
        foreach (var error in ErrorCodes)
        {
            errors.SetValueIfNotEmpty(error.Key.ToString("g"), error.Value);
        }

        JsonOperations.UpdatePath(desc, GetPathInfo(), type);
        OnOperationDescriptionDataFill(desc.Data);
        return desc;
    }

    /// <summary>
    /// Creates the JSON schema.
    /// </summary>
    /// <returns>The JSON schema description instance.</returns>
    public virtual JsonObjectSchemaDescription CreateArgumentSchema()
    {
        var schema = new JsonObjectSchemaDescription();
        foreach (var kvp in dict)
        {
            var converter = kvp.Value;
            if (string.IsNullOrWhiteSpace(converter?.PropertyName)) continue;
            var desc = converter.Schema ?? CreateDefaultPropertySchema();
            if (desc != null) schema.Properties[converter.PropertyName] = desc;
        }

        return schema;
    }

    /// <summary>
    /// Formats the URL to send request.
    /// </summary>
    /// <param name="url">The URL.</param>
    /// <param name="context">The context object.</param>
    /// <returns>A new URL.</returns>
    /// <exception cref="InvalidOperationException">The data is invalid.</exception>
    protected virtual string FormatUrl(string url, RoutedJsonOperationContext context)
        => url;

    /// <summary>
    /// Occurs on the query data is created.
    /// </summary>
    /// <param name="context">The context object.</param>
    /// <param name="q">The query data.</param>
    protected virtual void OnQueryDataInit(QueryData q, RoutedJsonOperationContext context)
    {
    }

    /// <summary>
    /// Occurs on the query data is ready to generate URL.
    /// </summary>
    /// <param name="context">The context object.</param>
    /// <param name="q">The query data.</param>
    protected virtual void OnQueryDataFill(QueryData q, RoutedJsonOperationContext context)
    {
    }

    /// <summary>
    /// Processes the response data.
    /// </summary>
    /// <param name="json">The response content.</param>
    /// <param name="context">The context object.</param>
    /// <param name="cancellationToken">The optional cancellation token.</param>
    /// <returns>The result.</returns>
    protected virtual Task<JsonObjectNode> ProcessResponseAsync(JsonObjectNode json, RoutedJsonOperationContext context, CancellationToken cancellationToken = default)
        => Task.FromResult(json);

    /// <summary>
    /// Processes the response data.
    /// </summary>
    /// <param name="json">The response content.</param>
    /// <param name="context">The context object.</param>
    /// <returns>The result.</returns>
    protected virtual JsonObjectNode ProcessResponse(JsonObjectNode json, RoutedJsonOperationContext context)
        => json;

    /// <summary>
    /// Creates the default property schema.
    /// </summary>
    /// <returns>The schema description instance.</returns>
    protected virtual JsonNodeSchemaDescription CreateDefaultPropertySchema()
        => new JsonStringSchemaDescription();

    /// <summary>
    /// Creates a JSON HTTP client.
    /// </summary>
    /// <param name="context">The context object.</param>
    /// <returns>A JSON HTTP client.</returns>
    protected virtual JsonHttpClient<JsonObjectNode> CreateHttpClient(RoutedJsonOperationContext context)
        => new();

    /// <summary>
    /// Sends HTTP request and receives JSON result.
    /// </summary>
    /// <param name="http">The JSON HTTP client.</param>
    /// <param name="url">The URL.</param>
    /// <param name="context">The context object.</param>
    /// <param name="cancellationToken">The optional cancellation token.</param>
    /// <returns>The result.</returns>
    /// <exception cref="FailedHttpException">HTTP failure.</exception>
    /// <exception cref="JsonException">The response content is not JSON.</exception>
    protected virtual Task<JsonObjectNode> SendAsync(JsonHttpClient<JsonObjectNode> http, string url, RoutedJsonOperationContext context, CancellationToken cancellationToken = default)
        => http.SendAsync(HttpMethod ?? HttpMethod.Get, url, cancellationToken);

    /// <summary>
    /// Occurs on HTTP failure.
    /// </summary>
    /// <param name="status">The HTTP status code.</param>
    /// <param name="ex">The exception</param>
    /// <param name="context">The context object.</param>
    protected virtual JsonObjectNode OnHttpFailure(HttpStatusCode? status, FailedHttpException ex, RoutedJsonOperationContext context)
        => null;

    /// <summary>
    /// Occurs on response JSON parsing failure.
    /// </summary>
    /// <param name="ex">The exception</param>
    /// <param name="context">The context object.</param>
    protected virtual void OnJsonParsingFailure(JsonException ex, RoutedJsonOperationContext context)
    {
    }

    /// <summary>
    /// Occurs on response JSON parsing failure.
    /// </summary>
    /// <param name="ex">The exception</param>
    /// <param name="context">The context object.</param>
    protected virtual void OnJsonParsingFailure(NotSupportedException ex, RoutedJsonOperationContext context)
    {
    }

    /// <summary>
    /// Occurs on fill the JSON operation description data.
    /// </summary>
    /// <param name="data"></param>
    protected virtual void OnOperationDescriptionDataFill(JsonObjectNode data)
    {
    }

    /// <summary>
    /// Gets path information.
    /// </summary>
    /// <returns>THe path info.</returns>
    protected virtual JsonOperationPathAttribute GetPathInfo()
        => null;
}

/// <summary>
/// The base JSON API operation of routed web API.
/// </summary>
/// <typeparam name="T">The type of result.</typeparam>
public class BaseRoutedJsonOperation<T> : BaseJsonOperation, IJsonTypeOperationDescriptive
{
    private readonly Dictionary<string, RoutedJsonOperationMappingItem> dict = new();

    /// <summary>
    /// Initializes a new instance of the BaseRoutedJsonOperation class.
    /// </summary>
    /// <param name="id">The operation identifier.</param>
    /// <param name="description">The operation description.</param>
    public BaseRoutedJsonOperation(string id, string description = null)
    {
        Id = id;
        Description = description;
    }

    /// <summary>
    /// Initializes a new instance of the BaseRoutedJsonOperation class.
    /// </summary>
    /// <param name="uri">The URI of the web API.</param>
    /// <param name="id">The operation identifier.</param>
    /// <param name="description">The operation description.</param>
    public BaseRoutedJsonOperation(Uri uri, string id, string description = null) : this(id, description)
    {
        WebApiUri = uri;
    }

    /// <summary>
    /// Gets or sets the operation identifier.
    /// </summary>
    public string Id { get; }

    /// <summary>
    /// Gets or sets the URI of the web API.
    /// </summary>
    public Uri WebApiUri { get; set; }

    /// <summary>
    /// Gets or sets the HTTP method.
    /// </summary>
    public HttpMethod HttpMethod { get; set; }

    /// <summary>
    /// Gets or sets the optional operation description.
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// Gets or sets the error schema description.
    /// </summary>
    public JsonNodeSchemaDescription ErrorSchema { get; set; }

    /// <summary>
    /// Gets the error codes supported and their description.
    /// These are following error schema.
    /// </summary>
    public Dictionary<int, string> ErrorCodes { get; } = new();

    /// <summary>
    /// Gets or sets the schema handler.
    /// </summary>
    public IJsonNodeSchemaCreationHandler<Type> SchemaHandler { get; set; }

    /// <summary>
    /// Registers.
    /// </summary>
    /// <param name="item">The mapping item.</param>
    public void Register(RoutedJsonOperationMappingItem item)
    {
        if (string.IsNullOrWhiteSpace(item?.PropertyName)) return;
        dict[item.PropertyName] = item;
    }

    /// <summary>
    /// Registers.
    /// </summary>
    /// <param name="propertyName">The JSON property name.</param>
    /// <param name="isPropertyPath">true if the property name is a path; otherwise, false.</param>
    /// <param name="argumentName">The argument name for routed API.</param>
    /// <param name="schema">The schema.</param>
    /// <returns>The mapping item.</returns>
    public RoutedJsonOperationMappingItem Register(string propertyName, bool isPropertyPath, string argumentName, JsonNodeSchemaDescription schema = null)
    {
        if (string.IsNullOrWhiteSpace(propertyName)) return null;
        var item = new RoutedJsonOperationMappingItem(propertyName, isPropertyPath, argumentName, schema);
        Register(item);
        return item;
    }

    /// <summary>
    /// Registers.
    /// </summary>
    /// <param name="propertyName">The JSON property name.</param>
    /// <param name="argumentName">The argument name for routed API.</param>
    /// <param name="schema">The schema.</param>
    /// <returns>The mapping item.</returns>
    public RoutedJsonOperationMappingItem Register(string propertyName, string argumentName = null, JsonNodeSchemaDescription schema = null)
    {
        if (string.IsNullOrWhiteSpace(propertyName)) return null;
        var item = new RoutedJsonOperationMappingItem(propertyName, argumentName, schema);
        Register(item);
        return item;
    }

    /// <summary>
    /// Registers.
    /// </summary>
    /// <param name="propertyName">The JSON property name.</param>
    /// <param name="isPropertyPath">true if the property name is a path; otherwise, false.</param>
    /// <param name="argumentName">The argument name for routed API.</param>
    /// <param name="description">The description.</param>
    /// <returns>The mapping item.</returns>
    public RoutedJsonOperationMappingItem Register(string propertyName, bool isPropertyPath, string argumentName, string description = null)
    {
        if (string.IsNullOrWhiteSpace(propertyName)) return null;
        var item = new RoutedJsonOperationMappingItem(propertyName, isPropertyPath, argumentName, description);
        Register(item);
        return item;
    }

    /// <summary>
    /// Registers.
    /// </summary>
    /// <param name="propertyName">The JSON property name.</param>
    /// <param name="argumentName">The argument name for routed API.</param>
    /// <param name="description">The description.</param>
    /// <returns>The mapping item.</returns>
    public RoutedJsonOperationMappingItem Register(string propertyName, string argumentName = null, string description = null)
    {
        if (string.IsNullOrWhiteSpace(propertyName)) return null;
        var item = new RoutedJsonOperationMappingItem(propertyName, argumentName, description);
        Register(item);
        return item;
    }

    /// <summary>
    /// Removes the value with the specified key from the mapping.
    /// </summary>
    /// <param name="propertyName">The property name.</param>
    /// <returns>true if the element is successfully found and removed; otherwise, false. This method returns false if key is not found in the mapping.</returns>
    public bool Remove(string propertyName)
        => propertyName != null && dict.Remove(propertyName);

    /// <summary>
    /// Gets the specific mapping item.
    /// </summary>
    /// <param name="propertyName">The property name.</param>
    /// <returns>The mapping item; or null, if non-exist.</returns>
    public RoutedJsonOperationMappingItem Get(string propertyName)
        => propertyName != null && dict.TryGetValue(propertyName, out var item) ? item : null;

    /// <summary>
    /// Tests if has the specific mapping item.
    /// </summary>
    /// <param name="propertyName">The property name.</param>
    /// <returns>true if contains; otherwise, false.</returns>
    public bool Contains(string propertyName)
        => propertyName != null && dict.TryGetValue(propertyName, out _);

    /// <summary>
    /// Tests if has the specific mapping item.
    /// </summary>
    /// <param name="item">The property to test.</param>
    /// <returns>true if contains; otherwise, false.</returns>
    public bool Contains(RoutedJsonOperationMappingItem item)
        => !string.IsNullOrEmpty(item?.PropertyName) && Get(item.PropertyName) == item;

    /// <summary>
    /// Processes.
    /// </summary>
    /// <param name="json">The input data.</param>
    /// <param name="contextValue">The context value.</param>
    /// <param name="cancellationToken">The optional cancellation token.</param>
    /// <returns>The response.</returns>
    /// <exception cref="ArgumentNullException">json was null.</exception>
    /// <exception cref="InvalidOperationException">The URI of the Web API was null or invalid; or the data is invalid.</exception>
    /// <exception cref="FailedHttpException">HTTP failure.</exception>
    /// <exception cref="JsonException">The response content is not JSON.</exception>
    public override async Task<JsonObjectNode> ProcessAsync(JsonObjectNode json, object contextValue, CancellationToken cancellationToken = default)
    {
        if (json == null) throw new ArgumentNullException(nameof(json), "json was null.");
        var args = json.ToString();
        var result = await ProcessAsync(args, contextValue, cancellationToken);
        return JsonObjectNode.Parse(result);
    }

    /// <summary>
    /// Processes.
    /// </summary>
    /// <param name="arguments">The input data.</param>
    /// <param name="contextValue">The context value.</param>
    /// <param name="cancellationToken">The optional cancellation token.</param>
    /// <returns>The response.</returns>
    /// <exception cref="ArgumentNullException">arguments was null.</exception>
    /// <exception cref="ArgumentException">arguments was invalid.</exception>
    /// <exception cref="InvalidOperationException">The URI of the Web API was null or invalid; or the data is invalid; or the result is failed to serialize.</exception>
    /// <exception cref="FailedHttpException">HTTP failure.</exception>
    /// <exception cref="JsonException">arguments or response content is not JSON.</exception>
    public override async Task<string> ProcessAsync(string arguments, object contextValue, CancellationToken cancellationToken = default)
    {
        if (arguments == null) throw new ArgumentNullException(nameof(arguments), "arguments string was null.");
        var uri = WebApiUri ?? throw new InvalidOperationException("The URI of the Web API does not configured.", new ArgumentNullException(nameof(WebApiUri), "The URI of the Web API was null."));
        var q = new QueryData();
        var json = JsonObjectNode.Parse(arguments) ?? throw new ArgumentException(nameof(arguments), "arguments string was not a JSON object format string.");
        var context = new RoutedJsonOperationContext(json, contextValue, q);
        OnQueryDataInit(q, context);
        foreach (var kvp in dict)
        {
            var converter = kvp.Value;
            if (string.IsNullOrWhiteSpace(converter?.ArgumentName)) continue;
            q[converter.ArgumentName] = converter.GetArgumentValue(json);
        }

        OnQueryDataFill(q, context);
        var url = q.ToString(uri);
        url = FormatUrl(url, context);
        if (url == null) throw new InvalidOperationException("The URI of the Web API or the JSON input data is invalid.");
        var http = CreateHttpClient(context) ?? new();
        T result;
        try
        {
            var resp = await SendAsync(http, url, context, cancellationToken);
            result = await ProcessResponseAsync(resp, context, cancellationToken);
        }
        catch (FailedHttpException ex)
        {
            OnHttpFailure(ex.StatusCode, ex, context);
            throw;
        }
        catch (HttpRequestException ex)
        {
            var ex2 = new FailedHttpException(null, ex.Message, ex);
            OnHttpFailure(ex2.StatusCode, ex2, context);
            throw ex2;
        }

        try
        {
            return Serialize(result, context);
        }
        catch (JsonException ex)
        {
            OnResultSerializeFailure(ex, context);
            throw;
        }
        catch (NotSupportedException ex)
        {
            OnResultSerializeFailure(ex, context);
            throw;
        }
    }

    /// <summary>
    /// Creates operation description.
    /// </summary>
    /// <returns>The operation description.</returns>
    public override JsonOperationDescription CreateDescription()
        => CreateDescription(null);

    /// <summary>
    /// Creates operation description.
    /// </summary>
    /// <param name="schemaHandler">The optional schema handler.</param>
    /// <returns>The operation description.</returns>
    public virtual JsonOperationDescription CreateDescription(IJsonNodeSchemaCreationHandler<Type> schemaHandler)
    {
        var type = GetType();
        var desc = new JsonOperationDescription()
        {
            Id = Id ?? (type == typeof(BaseRoutedJsonOperation) ? null : type.Name),
            Description = Description ?? StringExtensions.GetDescription(type),
            ArgumentSchema = CreateArgumentSchema(),
            ResultSchema = CreateResultSchema(schemaHandler ?? SchemaHandler),
            ErrorSchema = ErrorSchema,
        };
        desc.Data.SetValue(JsonOperations.HttpErrorProperty, out JsonObjectNode errors);
        foreach (var error in ErrorCodes)
        {
            errors.SetValueIfNotEmpty(error.Key.ToString("g"), error.Value);
        }

        JsonOperations.UpdatePath(desc, GetPathInfo(), type);
        OnOperationDescriptionDataFill(desc.Data);
        return desc;
    }

    /// <summary>
    /// Creates the JSON schema for argument.
    /// </summary>
    /// <returns>The JSON schema description instance.</returns>
    public virtual JsonObjectSchemaDescription CreateArgumentSchema()
    {
        var schema = new JsonObjectSchemaDescription();
        foreach (var kvp in dict)
        {
            var converter = kvp.Value;
            if (string.IsNullOrWhiteSpace(converter?.ArgumentName)) continue;
            var desc = converter.Schema ?? CreateDefaultPropertySchema();
            if (desc != null) schema.Properties[converter.ArgumentName] = desc;
        }

        return schema;
    }

    /// <summary>
    /// Creates the JSON schema for result.
    /// </summary>
    /// <returns>The JSON schema description instance.</returns>
    public JsonNodeSchemaDescription CreateResultSchema()
        => CreateResultSchema(null);

    /// <summary>
    /// Creates the JSON schema for result.
    /// </summary>
    /// <param name="schemaHandler">The optional schema handler.</param>
    /// <returns>The JSON schema description instance.</returns>
    public virtual JsonNodeSchemaDescription CreateResultSchema(IJsonNodeSchemaCreationHandler<Type> schemaHandler)
        => JsonValues.CreateSchema(typeof(T), null, schemaHandler ?? SchemaHandler);

    /// <summary>
    /// Formats the URL to send request.
    /// </summary>
    /// <param name="url">The URL.</param>
    /// <param name="context">The context object.</param>
    /// <returns>A new URL.</returns>
    /// <exception cref="InvalidOperationException">The data is invalid.</exception>
    protected virtual string FormatUrl(string url, RoutedJsonOperationContext context)
        => url;

    /// <summary>
    /// Occurs on the query data is created.
    /// </summary>
    /// <param name="context">The context object.</param>
    /// <param name="q">The query data.</param>
    protected virtual void OnQueryDataInit(QueryData q, RoutedJsonOperationContext context)
    {
    }

    /// <summary>
    /// Occurs on the query data is ready to generate URL.
    /// </summary>
    /// <param name="context">The context object.</param>
    /// <param name="q">The query data.</param>
    protected virtual void OnQueryDataFill(QueryData q, RoutedJsonOperationContext context)
    {
    }

    /// <summary>
    /// Processes the response data.
    /// </summary>
    /// <param name="json">The response content.</param>
    /// <param name="context">The context object.</param>
    /// <param name="cancellationToken">The optional cancellation token.</param>
    /// <returns>The result.</returns>
    protected virtual Task<T> ProcessResponseAsync(string json, RoutedJsonOperationContext context, CancellationToken cancellationToken = default)
        => Task.FromResult(JsonSerializer.Deserialize<T>(json));

    /// <summary>
    /// Creates the default property schema.
    /// </summary>
    /// <returns>The schema description instance.</returns>
    protected virtual JsonNodeSchemaDescription CreateDefaultPropertySchema()
        => new JsonStringSchemaDescription();

    /// <summary>
    /// Creates a JSON HTTP client.
    /// </summary>
    /// <param name="context">The context object.</param>
    /// <returns>A JSON HTTP client.</returns>
    protected virtual JsonHttpClient<string> CreateHttpClient(RoutedJsonOperationContext context)
        => new();

    /// <summary>
    /// Sends HTTP request and receives JSON result.
    /// </summary>
    /// <param name="http">The JSON HTTP client.</param>
    /// <param name="url">The URL.</param>
    /// <param name="context">The context object.</param>
    /// <param name="cancellationToken">The optional cancellation token.</param>
    /// <returns>The result.</returns>
    /// <exception cref="FailedHttpException">HTTP failure.</exception>
    /// <exception cref="JsonException">The response content is not JSON.</exception>
    protected virtual Task<string> SendAsync(JsonHttpClient<string> http, string url, RoutedJsonOperationContext context, CancellationToken cancellationToken = default)
        => http.SendAsync(HttpMethod ?? HttpMethod.Get, url, cancellationToken);

    /// <summary>
    /// Occurs on HTTP failure.
    /// </summary>
    /// <param name="status">The HTTP status code.</param>
    /// <param name="ex">The exception</param>
    /// <param name="context">The context object.</param>
    protected virtual void OnHttpFailure(HttpStatusCode? status, FailedHttpException ex, RoutedJsonOperationContext context)
    {
    }

    /// <summary>
    /// Occurs on response JSON parsing failure.
    /// </summary>
    /// <param name="ex">The exception</param>
    /// <param name="context">The context object.</param>
    protected virtual void OnResultSerializeFailure(JsonException ex, RoutedJsonOperationContext context)
    {
    }

    /// <summary>
    /// Occurs on response JSON parsing failure.
    /// </summary>
    /// <param name="ex">The exception</param>
    /// <param name="context">The context object.</param>
    protected virtual void OnResultSerializeFailure(NotSupportedException ex, RoutedJsonOperationContext context)
    {
    }

    /// <summary>
    /// Serializes
    /// </summary>
    /// <param name="result">The result.</param>
    /// <param name="context">The context object.</param>
    /// <returns>The JSON format string.</returns>
    /// <exception cref="InvalidOperationException">Deserialize result to JSON failed.</exception>
    /// <exception cref="NotSupportedException">There is no compatible JSON converter for the typeor its serializable members.</exception>
    /// <exception cref="JsonException">JSON serialize failed.</exception>
    protected virtual string Serialize(T result, RoutedJsonOperationContext context)
        => JsonOperations.SerializeResult(result);

    /// <summary>
    /// Occurs on fill the JSON operation description data.
    /// </summary>
    /// <param name="data"></param>
    protected virtual void OnOperationDescriptionDataFill(JsonObjectNode data)
    {
    }

    /// <summary>
    /// Gets path information.
    /// </summary>
    /// <returns>THe path info.</returns>
    protected virtual JsonOperationPathAttribute GetPathInfo()
        => null;
}
