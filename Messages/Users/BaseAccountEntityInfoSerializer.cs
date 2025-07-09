using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Trivial.Devices;
using Trivial.Net;
using Trivial.Tasks;
using Trivial.Text;

namespace Trivial.Users;

/// <summary>
/// The JSON serializer of account entity info.
/// </summary>
public abstract class BaseAccountEntityInfoSerializer
{
    /// <summary>
    /// Deserializes a JSON to entity.
    /// </summary>
    /// <param name="utf8Json">The JSON in UTF8 stream.</param>
    /// <param name="options">Options to control the reader behavior during parsing.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>Then entity serialized from the given JSON.</returns>
    /// <exception cref="TaskCanceledException">The task is cancelled.</exception>
    /// <exception cref="JsonException">json does not represent a valid single JSON object.</exception>
    /// <exception cref="ArgumentException">options contains unsupported options.</exception>
    public async Task<BaseAccountEntityInfo> DeserializeAsync(Stream utf8Json, JsonDocumentOptions options, CancellationToken cancellationToken = default)
    {
        var obj = await JsonObjectNode.ParseAsync(utf8Json, options, cancellationToken);
        return Deserialize(obj);
    }

    /// <summary>
    /// Deserializes a JSON to entity.
    /// </summary>
    /// <param name="utf8Json">The JSON in UTF8 stream.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>The entity serialized from the given JSON.</returns>
    /// <exception cref="TaskCanceledException">The task is cancelled.</exception>
    /// <exception cref="JsonException">json does not represent a valid single JSON object.</exception>
    /// <exception cref="ArgumentException">options contains unsupported options.</exception>
    public async Task<BaseAccountEntityInfo> DeserializeAsync(Stream utf8Json, CancellationToken cancellationToken = default)
    {
        var obj = await JsonObjectNode.ParseAsync(utf8Json, cancellationToken);
        return Deserialize(obj);
    }

    /// <summary>
    /// Deserializes a JSON to entity.
    /// </summary>
    /// <param name="file">A file with JSON object string content to parse.</param>
    /// <param name="options">Options to control the reader behavior during parsing.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>The entity serialized from the given JSON.</returns>
    /// <exception cref="JsonException">json does not represent a valid single JSON object.</exception>
    /// <exception cref="ArgumentException">options contains unsupported options.</exception>
    /// <exception cref="IOException">The entry is already currently open for writing, or the entry has been deleted from the archive.</exception>
    /// <exception cref="ObjectDisposedException">The zip archive has been disposed.</exception>
    /// <exception cref="NotSupportedException">The zip archive does not support reading.</exception>
    /// <exception cref="InvalidDataException">The zip archive is corrupt, and the entry cannot be retrieved.</exception>
    public async Task<BaseAccountEntityInfo> DeserializeAsync(FileInfo file, JsonDocumentOptions options = default, CancellationToken cancellationToken = default)
    {
        var obj = await JsonObjectNode.ParseAsync(file, options, cancellationToken);
        return Deserialize(obj);
    }

    /// <summary>
    /// Deserializes a JSON to entity.
    /// </summary>
    /// <param name="httpContent">The http response content.</param>
    /// <param name="options">The options for serialization.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the work if it has not yet started.</param>
    /// <returns>The entity serialized from the given JSON.</returns>
    /// <exception cref="JsonException">json does not represent a valid single JSON object.</exception>
    /// <exception cref="ArgumentException">options contains unsupported options.</exception>
    /// <exception cref="IOException">The entry is already currently open for writing, or the entry has been deleted from the archive.</exception>
    /// <exception cref="ObjectDisposedException">The zip archive has been disposed.</exception>
    /// <exception cref="NotSupportedException">The zip archive does not support reading.</exception>
    /// <exception cref="InvalidDataException">The zip archive is corrupt, and the entry cannot be retrieved.</exception>
    public async Task<BaseAccountEntityInfo> DeserializeAsync(HttpContent httpContent, JsonSerializerOptions options, CancellationToken cancellationToken = default)
    {
        var json = await HttpClientExtensions.DeserializeJsonAsync<JsonObjectNode>(httpContent, options, cancellationToken);
        return Deserialize(json);
    }

    /// <summary>
    /// Deserializes a JSON to entity.
    /// </summary>
    /// <param name="httpContent">The http response content.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the work if it has not yet started.</param>
    /// <returns>The entity serialized from the given JSON.</returns>
    /// <exception cref="JsonException">json does not represent a valid single JSON object.</exception>
    /// <exception cref="ArgumentException">options contains unsupported options.</exception>
    /// <exception cref="IOException">The entry is already currently open for writing, or the entry has been deleted from the archive.</exception>
    /// <exception cref="ObjectDisposedException">The zip archive has been disposed.</exception>
    /// <exception cref="NotSupportedException">The zip archive does not support reading.</exception>
    /// <exception cref="InvalidDataException">The zip archive is corrupt, and the entry cannot be retrieved.</exception>
    public async Task<BaseAccountEntityInfo> DeserializeAsync(HttpContent httpContent, CancellationToken cancellationToken = default)
    {
        var json = await HttpClientExtensions.DeserializeJsonAsync<JsonObjectNode>(httpContent, cancellationToken);
        return Deserialize(json);
    }

    /// <summary>
    /// Deserializes a JSON to entity.
    /// </summary>
    /// <param name="source">The source entity to fill when matches.</param>
    /// <param name="utf8Json">The JSON in UTF8 stream.</param>
    /// <param name="options">Options to control the reader behavior during parsing.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>Then entity serialized from the given JSON.</returns>
    /// <exception cref="TaskCanceledException">The task is cancelled.</exception>
    /// <exception cref="JsonException">json does not represent a valid single JSON object.</exception>
    /// <exception cref="ArgumentException">options contains unsupported options.</exception>
    public async Task<BaseAccountEntityInfo> DeserializeAsync(BaseAccountEntityInfo source, Stream utf8Json, JsonDocumentOptions options, CancellationToken cancellationToken = default)
    {
        var obj = await JsonObjectNode.ParseAsync(utf8Json, cancellationToken);
        return Deserialize(source, obj);
    }

    /// <summary>
    /// Deserializes a JSON to entity.
    /// </summary>
    /// <param name="source">The source entity to fill when matches.</param>
    /// <param name="utf8Json">The JSON in UTF8 stream.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>The entity serialized from the given JSON.</returns>
    /// <exception cref="TaskCanceledException">The task is cancelled.</exception>
    /// <exception cref="JsonException">json does not represent a valid single JSON object.</exception>
    /// <exception cref="ArgumentException">options contains unsupported options.</exception>
    public async Task<BaseAccountEntityInfo> DeserializeAsync(BaseAccountEntityInfo source, Stream utf8Json, CancellationToken cancellationToken = default)
    {
        var obj = await JsonObjectNode.ParseAsync(utf8Json, cancellationToken);
        return Deserialize(source, obj);
    }

    /// <summary>
    /// Deserializes a JSON to entity.
    /// </summary>
    /// <param name="source">The source entity to fill when matches.</param>
    /// <param name="file">A file with JSON object string content to parse.</param>
    /// <param name="options">Options to control the reader behavior during parsing.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>The entity serialized from the given JSON.</returns>
    /// <exception cref="JsonException">json does not represent a valid single JSON object.</exception>
    /// <exception cref="ArgumentException">options contains unsupported options.</exception>
    /// <exception cref="IOException">The entry is already currently open for writing, or the entry has been deleted from the archive.</exception>
    /// <exception cref="ObjectDisposedException">The zip archive has been disposed.</exception>
    /// <exception cref="NotSupportedException">The zip archive does not support reading.</exception>
    /// <exception cref="InvalidDataException">The zip archive is corrupt, and the entry cannot be retrieved.</exception>
    public async Task<BaseAccountEntityInfo> DeserializeAsync(BaseAccountEntityInfo source, FileInfo file, JsonDocumentOptions options = default, CancellationToken cancellationToken = default)
    {
        var obj = await JsonObjectNode.ParseAsync(file, options, cancellationToken);
        return Deserialize(source, obj);
    }

    /// <summary>
    /// Deserializes a JSON to entity.
    /// </summary>
    /// <param name="source">The source entity to fill when matches.</param>
    /// <param name="httpContent">The http response content.</param>
    /// <param name="options">The options for serialization.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the work if it has not yet started.</param>
    /// <returns>The entity serialized from the given JSON.</returns>
    /// <exception cref="JsonException">json does not represent a valid single JSON object.</exception>
    /// <exception cref="ArgumentException">options contains unsupported options.</exception>
    /// <exception cref="IOException">The entry is already currently open for writing, or the entry has been deleted from the archive.</exception>
    /// <exception cref="ObjectDisposedException">The zip archive has been disposed.</exception>
    /// <exception cref="NotSupportedException">The zip archive does not support reading.</exception>
    /// <exception cref="InvalidDataException">The zip archive is corrupt, and the entry cannot be retrieved.</exception>
    public async Task<BaseAccountEntityInfo> DeserializeAsync(BaseAccountEntityInfo source, HttpContent httpContent, JsonSerializerOptions options, CancellationToken cancellationToken = default)
    {
        var json = await HttpClientExtensions.DeserializeJsonAsync<JsonObjectNode>(httpContent, options, cancellationToken);
        return Deserialize(source, json);
    }

    /// <summary>
    /// Deserializes a JSON to entity.
    /// </summary>
    /// <param name="source">The source entity to fill when matches.</param>
    /// <param name="httpContent">The http response content.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the work if it has not yet started.</param>
    /// <returns>The entity serialized from the given JSON.</returns>
    /// <exception cref="JsonException">json does not represent a valid single JSON object.</exception>
    /// <exception cref="ArgumentException">options contains unsupported options.</exception>
    /// <exception cref="IOException">The entry is already currently open for writing, or the entry has been deleted from the archive.</exception>
    /// <exception cref="ObjectDisposedException">The zip archive has been disposed.</exception>
    /// <exception cref="NotSupportedException">The zip archive does not support reading.</exception>
    /// <exception cref="InvalidDataException">The zip archive is corrupt, and the entry cannot be retrieved.</exception>
    public async Task<BaseAccountEntityInfo> DeserializeAsync(BaseAccountEntityInfo source, HttpContent httpContent, CancellationToken cancellationToken = default)
    {
        var json = await HttpClientExtensions.DeserializeJsonAsync<JsonObjectNode>(httpContent, cancellationToken);
        return Deserialize(source, json);
    }

    /// <summary>
    /// Deserializes a JSON to entity.
    /// </summary>
    /// <param name="json">The JSON to serialize.</param>
    /// <param name="options">Options to control the reader behavior during parsing.</param>
    /// <returns>The entity serialized from the given JSON.</returns>
    /// <exception cref="JsonException">json does not represent a valid single JSON object.</exception>
    /// <exception cref="ArgumentException">options contains unsupported options.</exception>
    public BaseAccountEntityInfo Deserialize(string json, JsonDocumentOptions options = default)
    {
        var obj = JsonObjectNode.Parse(json, options);
        return Deserialize(obj);
    }

    /// <summary>
    /// Deserializes a JSON to entity.
    /// </summary>
    /// <param name="source">The source entity to fill when matches.</param>
    /// <param name="json">The JSON to serialize.</param>
    /// <param name="options">Options to control the reader behavior during parsing.</param>
    /// <returns>The entity serialized from the given JSON.</returns>
    /// <exception cref="JsonException">json does not represent a valid single JSON object.</exception>
    /// <exception cref="ArgumentException">options contains unsupported options.</exception>
    public BaseAccountEntityInfo Deserialize(BaseAccountEntityInfo source, string json, JsonDocumentOptions options = default)
    {
        var obj = JsonObjectNode.Parse(json, options);
        return Deserialize(source, obj);
    }

    /// <summary>
    /// Deserializes a JSON to entity.
    /// </summary>
    /// <param name="json">The JSON to serialize.</param>
    /// <returns>The entity serialized from the given JSON; or null, if not supported.</returns>
    public BaseAccountEntityInfo Deserialize(JsonObjectNode json)
    {
        if (json is null) return null;
        var type = AccountEntityInfoConverter.GetAccountEntityType(json, AccountEntityTypes.Unknown);
        return type switch
        {
            AccountEntityTypes.User => ToUser(json),
            AccountEntityTypes.Group => ToGroup(json),
            AccountEntityTypes.Service => ToServiceAccount(json),
            AccountEntityTypes.Bot => ToBotAccount(json),
            AccountEntityTypes.Device => ToAuthDevice(json),
            AccountEntityTypes.Agent => ToAgentAccount(json),
            AccountEntityTypes.Organization => ToOrganization(json),
            _ => ToUnknown(json)
        };
    }

    /// <summary>
    /// Deserializes a JSON to entity.
    /// </summary>
    /// <param name="source">The source entity to fill when matches.</param>
    /// <param name="json">The JSON to serialize.</param>
    /// <returns>The entity serialized from the given JSON; or null, if not supported.</returns>
    public virtual BaseAccountEntityInfo Deserialize(BaseAccountEntityInfo source, JsonObjectNode json)
    {
        if (source == null) return Deserialize(json);
        if (json == null) return null;
        if (source.TryFill(json)) return source;
        return Deserialize(json);
    }

    /// <summary>
    /// Deserializes a set of JSON to entity collection.
    /// </summary>
    /// <param name="arr">The JSON collection to serialize.</param>
    /// <returns>The entity collection serialized.</returns>
    public IEnumerable<BaseAccountEntityInfo> Deserialize(IEnumerable<JsonObjectNode> arr)
    {
        if (arr == null) yield break;
        foreach (var json in arr)
        {
            yield return Deserialize(json);
        }
    }

    /// <summary>
    /// Deserializes a set of JSON to entity collection.
    /// </summary>
    /// <param name="resources">The resources.</param>
    /// <param name="arr">The JSON collection to serialize.</param>
    /// <returns>The entity collection serialized.</returns>
    public int Deserialize(IAccountEntityResources resources, IEnumerable<JsonObjectNode> arr)
    {
        var i = 0;
        if (arr == null || resources == null) return i;
        var col = resources.Accounts;
        if (col == null)
        {
            var prop = resources.GetType().GetProperty(nameof(resources.Accounts));
            if (!prop.CanWrite) return 0;
            if (prop.PropertyType.IsInterface) col = new ObservableCollection<BaseAccountEntityInfo>();
            else col = Activator.CreateInstance(prop.PropertyType) as IList<BaseAccountEntityInfo>;
            prop.SetValue(resources, col, null);
        }

        foreach (var json in arr)
        {
            var instance = Deserialize(json);
            if (instance == null) continue;
            col.Add(instance);
            i++;
        }

        return i;
    }

    /// <summary>
    /// Converts a JSON node to user entity.
    /// </summary>
    /// <param name="json">The JSON node with information of the entity.</param>
    /// <returns>The entity converted.</returns>
    protected virtual UserItemInfo ToUser(JsonObjectNode json)
        => new(json);

    /// <summary>
    /// Converts a JSON node to user group entity.
    /// </summary>
    /// <param name="json">The JSON node with information of the entity.</param>
    /// <returns>The entity converted.</returns>
    protected virtual BaseUserGroupItemInfo ToGroup(JsonObjectNode json)
        => new(json);

    /// <summary>
    /// Converts a JSON node to service account entity.
    /// </summary>
    /// <param name="json">The JSON node with information of the entity.</param>
    /// <returns>The entity converted.</returns>
    protected virtual ServiceAccountItemInfo ToServiceAccount(JsonObjectNode json)
        => new(json);

    /// <summary>
    /// Converts a JSON node to bot entity.
    /// </summary>
    /// <param name="json">The JSON node with information of the entity.</param>
    /// <returns>The entity converted.</returns>
    protected virtual BotAccountItemInfo ToBotAccount(JsonObjectNode json)
        => new(json);

    /// <summary>
    /// Converts a JSON node to device entity.
    /// </summary>
    /// <param name="json">The JSON node with information of the entity.</param>
    /// <returns>The entity converted.</returns>
    protected virtual AuthDeviceItemInfo ToAuthDevice(JsonObjectNode json)
        => new(json);

    /// <summary>
    /// Converts a JSON node to agent entity.
    /// </summary>
    /// <param name="json">The JSON node with information of the entity.</param>
    /// <returns>The entity converted.</returns>
    protected virtual UserItemInfo ToAgentAccount(JsonObjectNode json)
        => new(json);

    /// <summary>
    /// Converts a JSON node to organization entity.
    /// </summary>
    /// <param name="json">The JSON node with information of the entity.</param>
    /// <returns>The entity converted.</returns>
    protected virtual OrgAccountItemInfo ToOrganization(JsonObjectNode json)
        => new(json);

    /// <summary>
    /// Converts a JSON node to an entity with unknown type.
    /// </summary>
    /// <param name="json">The JSON node with information of the entity.</param>
    /// <returns>The entity converted; or null, if not supported.</returns>
    protected virtual BaseAccountEntityInfo ToUnknown(JsonObjectNode json)
        => new UnknownAccountEntityInfo(json);
}
