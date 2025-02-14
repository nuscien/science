using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Trivial.Data;
using Trivial.Devices;
using Trivial.Reflection;
using Trivial.Security;
using Trivial.Tasks;
using Trivial.Text;

namespace Trivial.Users;

/// <summary>
/// The principal entity information.
/// </summary>
[JsonConverter(typeof(PrincipalEntityInfoConverter))]
public abstract class BasePrincipalEntityInfo : BaseResourceEntityInfo
{
    /// <summary>
    /// Initializes a new instance of the BasePrincipalEntityInfo class.
    /// </summary>
    /// <param name="type">The security principal entity type.</param>
    /// <param name="creation">The creation date time.</param>
    internal BasePrincipalEntityInfo(PrincipalEntityTypes type, DateTime? creation = null)
        : base(null, creation)
    {
        PrincipalEntityType = type;
        Supertype = "principal";
    }

    /// <summary>
    /// Initializes a new instance of the BasePrincipalEntityInfo class.
    /// </summary>
    /// <param name="type">The security principal entity type.</param>
    /// <param name="id">The resource identifier.</param>
    /// <param name="nickname">The nickname or display name.</param>
    /// <param name="avatar">The avatar URI.</param>
    /// <param name="creation">The creation date time.</param>
    internal BasePrincipalEntityInfo(PrincipalEntityTypes type, string id, string nickname, Uri avatar = null, DateTime? creation = null)
        : this(type, creation)
    {
        Id = id;
        Nickname = nickname;
        AvatarUri = avatar;
    }

    /// <summary>
    /// Initializes a new instance of the BasePrincipalEntityInfo class.
    /// </summary>
    /// <param name="type">The security principal entity type.</param>
    /// <param name="json">The JSON object to parse.</param>
    /// <param name="autoTypeSelect">true if use type from JSON; otherwise, false.</param>
    internal BasePrincipalEntityInfo(PrincipalEntityTypes type, JsonObjectNode json, bool autoTypeSelect = false)
        : base(json)
    {
        Supertype = "principal";
        PrincipalEntityType = json != null && autoTypeSelect ? PrincipalEntityInfoConverter.GetPrincipalEntityType(json, type) : type;
    }

    /// <summary>
    /// Gets the security principal entity type.
    /// </summary>
    [DataMember(Name = "type")]
    [JsonPropertyName("type")]
    [Description("This kind of entity can be used as an owner of the resource. This property is to define the type of the owner, e.g. a user, a user group, a service agent, etc.")]
    public PrincipalEntityTypes PrincipalEntityType { get; }

    /// <summary>
    /// Gets or sets the nickname.
    /// </summary>
    [DataMember(Name = "nickname")]
    [JsonPropertyName("nickname")]
    [Description("The nickname.")]
    public string Nickname
    {
        get => GetCurrentProperty<string>();
        set => SetCurrentProperty(value?.Trim());
    }

    /// <summary>
    /// Gets or sets the URI of avatar.
    /// </summary>
    [DataMember(Name = "avatar")]
    [JsonPropertyName("avatar")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    [Description("The URI of the avatar.")]
    public Uri AvatarUri
    {
        get => GetCurrentProperty<Uri>();
        set => SetCurrentProperty(value);
    }

    /// <summary>
    /// Gets or sets the introduction.
    /// </summary>
    [DataMember(Name = "bio", EmitDefaultValue = false)]
    [JsonPropertyName("bio")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    [Description("The introduction.")]
    public string Bio
    {
        get => GetCurrentProperty<string>();
        set => SetCurrentProperty(value);
    }

    /// <summary>
    /// Gets the raw JSON object for reference.
    /// The value is null if no such data.
    /// </summary>
    protected JsonObjectNode RawJson => base.GetProperty<JsonObjectNode>("_raw");

    /// <summary>
    /// Returns a string that represents this entity.
    /// </summary>
    /// <param name="sb">A string builder that represents this entity.</param>
    /// <returns>A string that represents this entity.</returns>
    protected virtual void ToString(StringBuilder sb)
    {
        sb.Append(Nickname ?? "?");
        sb.Append(" (");
        sb.Append(PrincipalEntityType.ToString());
        sb.Append(' ');
        sb.Append(Id ?? "-");
        sb.AppendLine(")");
    }

    /// <inheritdoc />
    protected override void Fill(JsonObjectNode json)
    {
        base.Fill(json);
        Nickname = json.TryGetStringTrimmedValue("nickname", true);
        AvatarUri = json.TryGetUriValue("avatar");
        Bio = json.TryGetStringValue("bio");
        if (json.TryGetBooleanValue("_raw") != false) SetProperty("_raw", json);
        else RemoveProperty("_raw");
    }

    /// <summary>
    /// Tries to fill the JSON object node.
    /// </summary>
    /// <param name="json">The JSON object node to read.</param>
    /// <returns>true if fill succeeded; otherwise, false.</returns>
    internal bool TryFill(JsonObjectNode json)
    {
        var id = json.TryGetId(out _);
        if (id != null && !string.IsNullOrWhiteSpace(Id) && id != Id) return false;
        var supertype = json.TryGetStringTrimmedValue("supertype", true);
        if (supertype != Supertype) return false;
        var type = json.TryGetEnumValue<PrincipalEntityTypes>("type", true);
        if (type != PrincipalEntityType) return false;
        Fill(json);
        return true;
    }

    /// <summary>
    /// Converts to JSON object.
    /// </summary>
    /// <returns>A JSON object.</returns>
    public override JsonObjectNode ToJson()
    {
        var json = base.ToJson();
        json.SetValue("type", PrincipalEntityType.ToString());
        json.SetValue("nickname", Nickname);
        if (AvatarUri != null) json.SetValue("avatar", AvatarUri);
        if (!string.IsNullOrEmpty(Bio)) json.SetValue("bio", Bio);
        var raw = RawJson;
        if (raw != null) json.SetValue("_raw", raw);
        return json;
    }

    /// <summary>
    /// Returns a string that represents this entity.
    /// </summary>
    /// <returns>A string that represents this entity.</returns>
    public override string ToString()
    {
        var sb = new StringBuilder();
        ToString(sb);
        return sb.ToString();
    }

    /// <summary>
    /// Gets a property value.
    /// </summary>
    /// <typeparam name="T">The type of the property value.</typeparam>
    /// <param name="key">The key.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <returns>A property value.</returns>
    public new T GetProperty<T>(string key, T defaultValue = default)
        => base.GetProperty(key, defaultValue);

    /// <summary>
    /// Gets a property value.
    /// </summary>
    /// <typeparam name="T">The type of the property value.</typeparam>
    /// <param name="key">The key.</param>
    /// <param name="result">The property value.</param>
    /// <returns>true if contains; otherwise, false.</returns>
    public new bool GetProperty<T>(string key, out T result)
        => base.GetProperty(key, out result);

    /// <summary>
    /// Converts to JSON object.
    /// </summary>
    /// <param name="value">The JSON value.</param>
    /// <returns>A JSON object.</returns>
    public static explicit operator JsonObjectNode(BasePrincipalEntityInfo value)
        => value?.ToJson();
}

/// <summary>
/// The JSON serializer of principal entity info.
/// </summary>
public abstract class BasePrincipalEntityInfoSerializer
{
    /// <summary>
    /// Serializes a JSON to entity.
    /// </summary>
    /// <param name="utf8Json">The JSON in UTF8 stream.</param>
    /// <param name="options">Options to control the reader behavior during parsing.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>Then entity serialized from the given JSON.</returns>
    /// <exception cref="TaskCanceledException">The task is cancelled.</exception>
    /// <exception cref="JsonException">json does not represent a valid single JSON object.</exception>
    /// <exception cref="ArgumentException">options contains unsupported options.</exception>
    public async Task<BasePrincipalEntityInfo> SerializeAsync(Stream utf8Json, JsonDocumentOptions options, CancellationToken cancellationToken = default)
    {
        var obj = await JsonObjectNode.ParseAsync(utf8Json, cancellationToken);
        return Serialize(obj);
    }

    /// <summary>
    /// Serializes a JSON to entity.
    /// </summary>
    /// <param name="utf8Json">The JSON in UTF8 stream.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>The entity serialized from the given JSON.</returns>
    /// <exception cref="TaskCanceledException">The task is cancelled.</exception>
    /// <exception cref="JsonException">json does not represent a valid single JSON object.</exception>
    /// <exception cref="ArgumentException">options contains unsupported options.</exception>
    public async Task<BasePrincipalEntityInfo> SerializeAsync(Stream utf8Json, CancellationToken cancellationToken = default)
    {
        var obj = await JsonObjectNode.ParseAsync(utf8Json, cancellationToken);
        return Serialize(obj);
    }

    /// <summary>
    /// Serializes a JSON to entity.
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
    public async Task<BasePrincipalEntityInfo> SerializeAsync(FileInfo file, JsonDocumentOptions options = default, CancellationToken cancellationToken = default)
    {
        var obj = await JsonObjectNode.ParseAsync(file, options, cancellationToken);
        return Serialize(obj);
    }

    /// <summary>
    /// Serializes a JSON to entity.
    /// </summary>
    /// <param name="source">The source entity to fill when matches.</param>
    /// <param name="utf8Json">The JSON in UTF8 stream.</param>
    /// <param name="options">Options to control the reader behavior during parsing.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>Then entity serialized from the given JSON.</returns>
    /// <exception cref="TaskCanceledException">The task is cancelled.</exception>
    /// <exception cref="JsonException">json does not represent a valid single JSON object.</exception>
    /// <exception cref="ArgumentException">options contains unsupported options.</exception>
    public async Task<BasePrincipalEntityInfo> SerializeAsync(BasePrincipalEntityInfo source, Stream utf8Json, JsonDocumentOptions options, CancellationToken cancellationToken = default)
    {
        var obj = await JsonObjectNode.ParseAsync(utf8Json, cancellationToken);
        return Serialize(source, obj);
    }

    /// <summary>
    /// Serializes a JSON to entity.
    /// </summary>
    /// <param name="source">The source entity to fill when matches.</param>
    /// <param name="utf8Json">The JSON in UTF8 stream.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>The entity serialized from the given JSON.</returns>
    /// <exception cref="TaskCanceledException">The task is cancelled.</exception>
    /// <exception cref="JsonException">json does not represent a valid single JSON object.</exception>
    /// <exception cref="ArgumentException">options contains unsupported options.</exception>
    public async Task<BasePrincipalEntityInfo> SerializeAsync(BasePrincipalEntityInfo source, Stream utf8Json, CancellationToken cancellationToken = default)
    {
        var obj = await JsonObjectNode.ParseAsync(utf8Json, cancellationToken);
        return Serialize(source, obj);
    }

    /// <summary>
    /// Serializes a JSON to entity.
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
    public async Task<BasePrincipalEntityInfo> SerializeAsync(BasePrincipalEntityInfo source, FileInfo file, JsonDocumentOptions options = default, CancellationToken cancellationToken = default)
    {
        var obj = await JsonObjectNode.ParseAsync(file, options, cancellationToken);
        return Serialize(source, obj);
    }

    /// <summary>
    /// Serializes a JSON to entity.
    /// </summary>
    /// <param name="json">The JSON to serialize.</param>
    /// <param name="options">Options to control the reader behavior during parsing.</param>
    /// <returns>The entity serialized from the given JSON.</returns>
    /// <exception cref="JsonException">json does not represent a valid single JSON object.</exception>
    /// <exception cref="ArgumentException">options contains unsupported options.</exception>
    public BasePrincipalEntityInfo Serialize(string json, JsonDocumentOptions options = default)
    {
        var obj = JsonObjectNode.Parse(json, options);
        return Serialize(obj);
    }

    /// <summary>
    /// Serializes a JSON to entity.
    /// </summary>
    /// <param name="source">The source entity to fill when matches.</param>
    /// <param name="json">The JSON to serialize.</param>
    /// <param name="options">Options to control the reader behavior during parsing.</param>
    /// <returns>The entity serialized from the given JSON.</returns>
    /// <exception cref="JsonException">json does not represent a valid single JSON object.</exception>
    /// <exception cref="ArgumentException">options contains unsupported options.</exception>
    public BasePrincipalEntityInfo Serialize(BasePrincipalEntityInfo source, string json, JsonDocumentOptions options = default)
    {
        var obj = JsonObjectNode.Parse(json, options);
        return Serialize(source, obj);
    }

    /// <summary>
    /// Serializes a JSON to entity.
    /// </summary>
    /// <param name="json">The JSON to serialize.</param>
    /// <returns>The entity serialized from the given JSON; or null, if not supported.</returns>
    public BasePrincipalEntityInfo Serialize(JsonObjectNode json)
    {
        if (json is null) return null;
        var type = PrincipalEntityInfoConverter.GetPrincipalEntityType(json, PrincipalEntityTypes.Unknown);
        return type switch
        {
            PrincipalEntityTypes.User => ToUser(json),
            PrincipalEntityTypes.Group => ToGroup(json),
            PrincipalEntityTypes.Service => ToServiceAccount(json),
            PrincipalEntityTypes.Bot => ToBotAccount(json),
            PrincipalEntityTypes.Device => ToAuthDevice(json),
            PrincipalEntityTypes.Agent => ToAgentAccount(json),
            PrincipalEntityTypes.Organization => ToOrganization(json),
            _ => ToUnknown(json)
        };
    }

    /// <summary>
    /// Serializes a JSON to entity.
    /// </summary>
    /// <param name="source">The source entity to fill when matches.</param>
    /// <param name="json">The JSON to serialize.</param>
    /// <returns>The entity serialized from the given JSON; or null, if not supported.</returns>
    public virtual BasePrincipalEntityInfo Serialize(BasePrincipalEntityInfo source, JsonObjectNode json)
    {
        if (source == null) return Serialize(json);
        if (json == null) return null;
        if (source.TryFill(json)) return source;
        return Serialize(json);
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
    protected virtual BasePrincipalEntityInfo ToUnknown(JsonObjectNode json)
        => new UnknownPrincipalEntityInfo(json);
}

/// <summary>
/// Unknown principal entity information.
/// </summary>
internal sealed class UnknownPrincipalEntityInfo : BasePrincipalEntityInfo
{
    /// <summary>
    /// Initializes a new instance of the UnknownPrincipalEntityInfo class.
    /// </summary>
    /// <param name="id">The resource identifier.</param>
    /// <param name="creation">The creation date time.</param>
    public UnknownPrincipalEntityInfo(string id, DateTime? creation = null)
        : base(PrincipalEntityTypes.Unknown, creation)
    {
        Id = id;
        IsUnknownType = true;
    }

    /// <summary>
    /// Initializes a new instance of the UnknownPrincipalEntityInfo class.
    /// </summary>
    /// <param name="json">The JSON object to parse.</param>
    public UnknownPrincipalEntityInfo(JsonObjectNode json)
        : base(PrincipalEntityTypes.Unknown, json, true)
    {
        IsUnknownType = true;
    }

    /// <summary>
    /// Gets a value indicating whether the principal entity type is unknown.
    /// </summary>
    [JsonIgnore]
    public bool IsUnknownType { get; }
}

/// <summary>
/// JSON value node converter.
/// </summary>
internal sealed class PrincipalEntityInfoConverter : JsonObjectHostConverter<BasePrincipalEntityInfo>
{
    /// <inheritdoc />
    protected override BasePrincipalEntityInfo Create(JsonObjectNode json)
        => Convert(json);

    /// <summary>
    /// Converts the JSON to the principal entity.
    /// </summary>
    /// <param name="json">The JSON to parse.</param>
    /// <returns>The principal entity.</returns>
    public static BasePrincipalEntityInfo Convert(JsonObjectNode json)
    {
        var type = GetPrincipalEntityType(json, PrincipalEntityTypes.Unknown);
        return type switch
        {
            PrincipalEntityTypes.User => new UserItemInfo(json),
            PrincipalEntityTypes.Group => new BaseUserGroupItemInfo(json),
            PrincipalEntityTypes.Service => new ServiceAccountItemInfo(json),
            PrincipalEntityTypes.Bot => new BotAccountItemInfo(json),
            PrincipalEntityTypes.Device => new AuthDeviceItemInfo(json),
            PrincipalEntityTypes.Agent => new AgentAccountItemInfo(json),
            PrincipalEntityTypes.Organization => new OrgAccountItemInfo(json),
            _ => null
        };
    }

    /// <summary>
    /// Gets the principal entity type from the given JSON.
    /// </summary>
    /// <param name="json">The JSON input.</param>
    /// <param name="defaultType">The default type.</param>
    /// <returns>The principal entity type.</returns>
    public static PrincipalEntityTypes GetPrincipalEntityType(JsonObjectNode json, PrincipalEntityTypes defaultType)
    {
        var type = json?.TryGetStringTrimmedValue("gender", true)?.ToLowerInvariant();
        if (type == null) return defaultType;
        return type switch
        {
            "u" or "user" or "account" or "用户" or "1" => PrincipalEntityTypes.User,
            "g" or "group" or "role" or "container" or "list" or "组" or "角色" or "2" => PrincipalEntityTypes.Group,
            "app" or "service" or "服务" or "3" => PrincipalEntityTypes.Service,
            "bot" or "robot" or "ai" or "assistance" or "machine" or "机器人" or "4" => PrincipalEntityTypes.Bot,
            "d" or "device" or "iot" or "client" or "设备" or "5" => PrincipalEntityTypes.Device,
            "agent" or "代理" or "6" => PrincipalEntityTypes.Agent,
            "org" or "organization" or "company" or "tenant" or "组织" or "7" => PrincipalEntityTypes.Organization,
            "default" or "-" => defaultType,
            _ => PrincipalEntityTypes.Other
        };
    }
}
