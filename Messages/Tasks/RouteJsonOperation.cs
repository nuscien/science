﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
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
/// The mapping item for route JSON API operation.
/// </summary>
public class RouteJsonOperationMappingItem
{
    /// <summary>
    /// Initializes a new instance of the RouteJsonOperationMappingItem class.
    /// </summary>
    /// <param name="propertyName">The JSON property name.</param>
    /// <param name="isPropertyPath">true if the property name is a path; otherwise, false.</param>
    /// <param name="argumentName">The argument name for routed API.</param>
    public RouteJsonOperationMappingItem(string propertyName, bool isPropertyPath, string argumentName)
    {
        PropertyName = propertyName;
        IsPropertyPath = isPropertyPath;
        ArgumentName = argumentName;
    }

    /// <summary>
    /// Initializes a new instance of the RouteJsonOperationMappingItem class.
    /// </summary>
    /// <param name="propertyName">The JSON property name.</param>
    /// <param name="argumentName">The argument name for routed API.</param>
    public RouteJsonOperationMappingItem(string propertyName, string argumentName = null)
    {
        PropertyName = propertyName;
        ArgumentName = argumentName ?? propertyName;
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
    /// Converts the JSON property token data to the argument value.
    /// </summary>
    /// <param name="value">The JSON property token.</param>
    /// <returns>The argument value converted.</returns>
    public virtual string Convert(IJsonDataNode value)
    {
        if (value == null) return null;
        if (value is IJsonStringNode s) return s.StringValue;
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
/// The context of route JSON API operation.
/// </summary>
public class RouteJsonOperationContext
{
    /// <summary>
    /// Initializes a new instance of the RouteJsonOperationContext class.
    /// </summary>
    /// <param name="args">The arguments.</param>
    public RouteJsonOperationContext(JsonObjectNode args)
    {
        Arguments = args;
        Query = new();
    }

    /// <summary>
    /// Initializes a new instance of the RouteJsonOperationContext class.
    /// </summary>
    /// <param name="args">The arguments.</param>
    /// <param name="value">The context value.</param>
    /// <param name="q">The query data.</param>
    public RouteJsonOperationContext(JsonObjectNode args, object value, QueryData q)
    {
        Arguments = args;
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
/// The base JSON API operation of route web API.
/// </summary>
public class BaseRouteJsonOperation : BaseJsonOperation
{
    private readonly Dictionary<string, RouteJsonOperationMappingItem> dict = new();

    /// <summary>
    /// Initializes a new instance of the BaseRouteJsonOperation class.
    /// </summary>
    /// <param name="id">The operation identifier.</param>
    /// <param name="description">The operation description.</param>
    public BaseRouteJsonOperation(string id, string description = null)
    {
        Id = id;
        Description = description;
    }

    /// <summary>
    /// Initializes a new instance of the BaseRouteJsonOperation class.
    /// </summary>
    /// <param name="uri">The URI of the web API.</param>
    /// <param name="id">The operation identifier.</param>
    /// <param name="description">The operation description.</param>
    public BaseRouteJsonOperation(Uri uri, string id, string description = null) : this(id, description)
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
    public void Register(RouteJsonOperationMappingItem item)
    {
        if (string.IsNullOrWhiteSpace(item.PropertyName)) return;
        dict[item.PropertyName] = item;
    }

    /// <summary>
    /// Removes the value with the specified key from the mapping.
    /// </summary>
    /// <param name="propertyName">The property name.</param>
    /// <returns>true if the element is successfully found and removed; otherwise, false. This method returns false if key is not found in the mapping.</returns>
    public bool Remove(string propertyName)
        => dict.Remove(propertyName);

    /// <summary>
    /// Gets the specific mapping item.
    /// </summary>
    /// <param name="propertyName">The property name.</param>
    /// <returns>The mapping item; or null, if non-exist.</returns>
    public RouteJsonOperationMappingItem Get(string propertyName)
        => dict.TryGetValue(propertyName, out var item) ? item : null;

    /// <summary>
    /// Tests if has the specific mapping item.
    /// </summary>
    /// <param name="propertyName">The property name.</param>
    /// <returns>true if contains; otherwise, false.</returns>
    public bool Contains(string propertyName)
        => dict.TryGetValue(propertyName, out _);

    /// <summary>
    /// Tests if has the specific mapping item.
    /// </summary>
    /// <param name="item">The property to test.</param>
    /// <returns>true if contains; otherwise, false.</returns>
    public bool Contains(RouteJsonOperationMappingItem item)
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
        var context = new RouteJsonOperationContext(json, contextValue, q);
        foreach (var kvp in dict)
        {
            var converter = kvp.Value;
            if (string.IsNullOrWhiteSpace(converter?.ArgumentName)) continue;
            q[converter.ArgumentName] = converter.Convert(json);
        }

        var url = q.ToString(uri);
        url = FormatUrl(url, context);
        if (url == null) throw new InvalidOperationException("The URI of the Web API or the JSON input data is invalid.");
        var http = CreateHttpClient(context) ?? new();
        try
        {
            var resp = await SendAsync(http, url, context, cancellationToken);
            resp = await ProcessResponseAsync(resp, context, cancellationToken);
            return resp;
        }
        catch (FailedHttpException ex)
        {
            var resp = OnHttpFailure(ex, context);
            if (resp != null) return resp;
            throw;
        }
        catch (HttpRequestException ex)
        {
            var ex2 = new FailedHttpException(null, ex.Message, ex);
            var resp = OnHttpFailure(ex2, context);
            if (resp != null) return resp;
            throw ex2;
        }
        catch (JsonException ex)
        {
            OnJsonParsingFailure(ex, context);
            throw;
        }
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
            Id = Id ?? (type == typeof(BaseRouteJsonOperation) ? null : GetType().Name),
            Description = Description,
            ArgumentSchema = CreateArgumentSchema(),
            ResultSchema = ResultSchema,
            ErrorSchema = ErrorSchema,
        };
        desc.Data.SetValue("httpError", out JsonObjectNode errors);
        foreach (var error in ErrorCodes)
        {
            errors.SetValueIfNotEmpty(error.Key.ToString("g"), error.Value);
        }

        return desc;
    }

    /// <summary>
    /// Creates the JSON schema.
    /// </summary>
    /// <returns>The JSON schema description instance.</returns>
    public JsonObjectSchemaDescription CreateArgumentSchema()
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
    /// Formats the URL to send request.
    /// </summary>
    /// <param name="url">The URL.</param>
    /// <param name="context">The context object.</param>
    /// <returns>A new URL.</returns>
    /// <exception cref="InvalidOperationException">The data is invalid.</exception>
    protected virtual string FormatUrl(string url, RouteJsonOperationContext context)
        => url;

    /// <summary>
    /// Processes the response data.
    /// </summary>
    /// <param name="json">The response content.</param>
    /// <param name="context">The context object.</param>
    /// <param name="cancellationToken">The optional cancellation token.</param>
    /// <returns>The result.</returns>
    protected virtual Task<JsonObjectNode> ProcessResponseAsync(JsonObjectNode json, RouteJsonOperationContext context, CancellationToken cancellationToken = default)
        => Task.FromResult(json);

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
    protected virtual JsonHttpClient<JsonObjectNode> CreateHttpClient(RouteJsonOperationContext context)
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
    protected virtual Task<JsonObjectNode> SendAsync(JsonHttpClient<JsonObjectNode> http, string url, RouteJsonOperationContext context, CancellationToken cancellationToken = default)
        => http.SendAsync(HttpMethod, url, cancellationToken);

    /// <summary>
    /// Occurs on HTTP failure.
    /// </summary>
    /// <param name="ex">The exception</param>
    /// <param name="context">The context object.</param>
    protected virtual JsonObjectNode OnHttpFailure(FailedHttpException ex, RouteJsonOperationContext context)
        => null;

    /// <summary>
    /// Occurs on response JSON parsing failure.
    /// </summary>
    /// <param name="ex">The exception</param>
    /// <param name="context">The context object.</param>
    protected virtual void OnJsonParsingFailure(JsonException ex, RouteJsonOperationContext context)
    {
    }
}
