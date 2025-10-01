using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Trivial.Data;
using Trivial.Devices;
using Trivial.Net;
using Trivial.Reflection;
using Trivial.Security;
using Trivial.Tasks;
using Trivial.Text;

namespace Trivial.Users;

/// <summary>
/// The account entity information.
/// </summary>
[JsonConverter(typeof(AccountEntityInfoConverter))]
public abstract class BaseAccountEntityInfo : BaseResourceEntityInfo
{
    /// <summary>
    /// Initializes a new instance of the BaseAccountEntityInfo class.
    /// </summary>
    /// <param name="type">The account entity type.</param>
    /// <param name="args">The initialization arguments.</param>
    internal BaseAccountEntityInfo(AccountEntityTypes type, ResourceEntityArgs args)
        : base(args)
    {
        AccountEntityType = type;
        if (args is not AccountEntityArgs a) return;
        Nickname = a.Nickname;
        AvatarUri = a.AvatarUri;
        Bio = a.Bio;
    }

    /// <summary>
    /// Initializes a new instance of the BaseAccountEntityInfo class.
    /// </summary>
    /// <param name="type">The account entity type.</param>
    /// <param name="args">The initialization arguments.</param>
    internal BaseAccountEntityInfo(AccountEntityTypes type, AccountEntityArgs args)
        : base(args)
    {
        AccountEntityType = type;
        if (args == null) return;
        Nickname = args.Nickname;
        AvatarUri = args.AvatarUri;
        Bio = args.Bio;
    }

    /// <summary>
    /// Initializes a new instance of the BaseAccountEntityInfo class.
    /// </summary>
    /// <param name="type">The account entity type.</param>
    /// <param name="creation">The creation date time.</param>
    internal BaseAccountEntityInfo(AccountEntityTypes type, DateTime? creation = null)
        : base(null, creation)
    {
        AccountEntityType = type;
    }

    /// <summary>
    /// Initializes a new instance of the BaseAccountEntityInfo class.
    /// </summary>
    /// <param name="type">The account entity type.</param>
    /// <param name="args">The initialization arguments.</param>
    /// <param name="nickname">The nickname or display name.</param>
    /// <param name="avatar">The avatar URI.</param>
    internal BaseAccountEntityInfo(AccountEntityTypes type, ResourceEntityArgs args, string nickname, Uri avatar = null)
        : base(args)
    {
        AccountEntityType = type;
        Nickname = nickname;
        AvatarUri = avatar;
    }

    /// <summary>
    /// Initializes a new instance of the BaseAccountEntityInfo class.
    /// </summary>
    /// <param name="type">The account entity type.</param>
    /// <param name="id">The resource identifier.</param>
    /// <param name="nickname">The nickname or display name.</param>
    /// <param name="avatar">The avatar URI.</param>
    /// <param name="creation">The creation date time.</param>
    internal BaseAccountEntityInfo(AccountEntityTypes type, string id, string nickname, Uri avatar = null, DateTime? creation = null)
        : this(type, creation)
    {
        Id = id;
        Nickname = nickname;
        AvatarUri = avatar;
    }

    /// <summary>
    /// Initializes a new instance of the BaseAccountEntityInfo class.
    /// </summary>
    /// <param name="type">The account entity type.</param>
    /// <param name="json">The JSON object to parse.</param>
    /// <param name="autoTypeSelect">true if use type from JSON; otherwise, false.</param>
    internal BaseAccountEntityInfo(AccountEntityTypes type, JsonObjectNode json, bool autoTypeSelect = false)
        : base(json)
    {
        AccountEntityType = json != null && autoTypeSelect ? AccountEntityInfoConverter.GetAccountEntityType(json, type) : type;
    }

    /// <summary>
    /// Gets the account entity type.
    /// </summary>
    [DataMember(Name = "type")]
    [JsonPropertyName("type")]
    [Description("The types of this security account entity, e.g. a user, a user group, a service agent, etc.")]
    public AccountEntityTypes AccountEntityType { get; }

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

    /// <inheritdoc />
    [JsonIgnore]
#if NETCOREAPP
    [NotMapped]
#endif
    protected override string Supertype => "account";

    /// <inheritdoc />
    [JsonIgnore]
#if NETCOREAPP
    [NotMapped]
#endif
    protected override string ResourceType => AccountEntityType.ToString();

    /// <summary>
    /// Gets the display name of this entity.
    /// </summary>
    /// <returns>The display name.</returns>
    protected override string GetDisplayName()
        => Nickname;

    /// <summary>
    /// Returns a string that represents this entity.
    /// </summary>
    /// <param name="sb">A string builder that represents this entity.</param>
    /// <returns>A string that represents this entity.</returns>
    protected virtual void ToString(StringBuilder sb)
    {
        sb.Append(Nickname ?? "?");
        sb.Append(" (");
        sb.Append(AccountEntityType.ToString());
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
        var type = json.TryGetEnumValue<AccountEntityTypes>("type", true);
        if (type != AccountEntityType) return false;
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
    public static explicit operator JsonObjectNode(BaseAccountEntityInfo value)
        => value?.ToJson();
}

/// <summary>
/// The arguments to initializes an account resource entity.
/// </summary>
[Guid("DDB1CE37-021F-464D-8476-298977062400")]
public class AccountEntityArgs : ResourceEntityArgs
{
    /// <summary>
    /// Initializes a new instance of the AccountEntityArgs class.
    /// </summary>
    /// <param name="keepNullId">true if keep the identifier as null; otherwise, false.</param>
    /// <param name="nickname">The nickname or display name.</param>
    /// <param name="avatar">The avatar URI.</param>
    public AccountEntityArgs(bool keepNullId, string nickname = null, Uri avatar = null)
        : base(keepNullId)
    {
        Nickname = nickname;
        AvatarUri = avatar;
    }

    /// <summary>
    /// Initializes a new instance of the AccountEntityArgs class.
    /// </summary>
    public AccountEntityArgs()
        : base(false)
    {
    }

    /// <summary>
    /// Initializes a new instance of the AccountEntityArgs class.
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <param name="creation">The creation date time.</param>
    public AccountEntityArgs(Guid id, DateTime? creation = null)
        : base(id.ToString("N"), creation)
    {
    }

    /// <summary>
    /// Initializes a new instance of the AccountEntityArgs class.
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <param name="creation">The creation date time.</param>
    /// <param name="modification">The last modification date time.</param>
    public AccountEntityArgs(Guid id, DateTime creation, DateTime modification)
        : base(id.ToString("N"), creation, modification)
    {
    }

    /// <summary>
    /// Initializes a new instance of the AccountEntityArgs class.
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <param name="creation">The creation date time.</param>
    public AccountEntityArgs(string id, DateTime? creation = null)
        : base(id, creation)
    {
    }

    /// <summary>
    /// Initializes a new instance of the AccountEntityArgs class.
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <param name="creation">The creation date time.</param>
    /// <param name="modification">The last modification date time.</param>
    public AccountEntityArgs(string id, DateTime creation, DateTime modification)
        : base(id, creation, modification)
    {
    }

    /// <summary>
    /// Initializes a new instance of the AccountEntityArgs class.
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <param name="nickname">The nickname or display name.</param>
    /// <param name="avatar">The avatar URI.</param>
    /// <param name="creation">The creation date time.</param>
    public AccountEntityArgs(Guid id, string nickname, Uri avatar, DateTime? creation = null)
        : base(id.ToString("N"), creation)
    {
        Nickname = nickname;
        AvatarUri = avatar;
    }

    /// <summary>
    /// Initializes a new instance of the AccountEntityArgs class.
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <param name="nickname">The nickname or display name.</param>
    /// <param name="avatar">The avatar URI.</param>
    /// <param name="creation">The creation date time.</param>
    /// <param name="modification">The last modification date time.</param>
    public AccountEntityArgs(Guid id, string nickname, Uri avatar, DateTime creation, DateTime modification)
        : base(id.ToString("N"), creation, modification)
    {
        Nickname = nickname;
        AvatarUri = avatar;
    }

    /// <summary>
    /// Initializes a new instance of the AccountEntityArgs class.
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <param name="nickname">The nickname or display name.</param>
    /// <param name="avatar">The avatar URI.</param>
    /// <param name="creation">The creation date time.</param>
    public AccountEntityArgs(string id, string nickname, Uri avatar, DateTime? creation = null)
        : base(id, creation)
    {
        Nickname = nickname;
        AvatarUri = avatar;
    }

    /// <summary>
    /// Initializes a new instance of the AccountEntityArgs class.
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <param name="nickname">The nickname or display name.</param>
    /// <param name="avatar">The avatar URI.</param>
    /// <param name="creation">The creation date time.</param>
    /// <param name="modification">The last modification date time.</param>
    public AccountEntityArgs(string id, string nickname, Uri avatar, DateTime creation, DateTime modification)
        : base(id, creation, modification)
    {
        Nickname = nickname;
        AvatarUri = avatar;
    }

    /// <summary>
    /// Gets or sets the nickname.
    /// </summary>
    [DataMember(Name = "nickname")]
    [JsonPropertyName("nickname")]
    [Description("The nickname.")]
    public string Nickname { get; set; }

    /// <summary>
    /// Gets or sets the URI of avatar.
    /// </summary>
    [DataMember(Name = "avatar")]
    [JsonPropertyName("avatar")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    [Description("The URI of the avatar.")]
    public Uri AvatarUri { get; set; }

    /// <summary>
    /// Gets or sets the introduction.
    /// </summary>
    [DataMember(Name = "bio", EmitDefaultValue = false)]
    [JsonPropertyName("bio")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    [Description("The introduction.")]
    public string Bio { get; set; }
}

/// <summary>
/// Unknown account entity information.
/// </summary>
internal sealed class UnknownAccountEntityInfo : BaseAccountEntityInfo
{
    /// <summary>
    /// Initializes a new instance of the UnknownAccountEntityInfo class.
    /// </summary>
    /// <param name="id">The resource identifier.</param>
    /// <param name="creation">The creation date time.</param>
    public UnknownAccountEntityInfo(string id, DateTime? creation = null)
        : base(AccountEntityTypes.Unknown, creation)
    {
        Id = id;
        IsUnknownType = true;
    }

    /// <summary>
    /// Initializes a new instance of the UnknownAccountEntityInfo class.
    /// </summary>
    /// <param name="json">The JSON object to parse.</param>
    public UnknownAccountEntityInfo(JsonObjectNode json)
        : base(AccountEntityTypes.Unknown, json, true)
    {
        IsUnknownType = true;
    }

    /// <summary>
    /// Gets a value indicating whether the account entity type is unknown.
    /// </summary>
    [JsonIgnore]
    public bool IsUnknownType { get; }
}

/// <summary>
/// JSON value node converter.
/// </summary>
internal sealed class AccountEntityInfoConverter : JsonObjectHostConverter<BaseAccountEntityInfo>
{
    /// <inheritdoc />
    protected override BaseAccountEntityInfo Create(JsonObjectNode json)
        => Convert(json);

    /// <summary>
    /// Converts the JSON to the account entity.
    /// </summary>
    /// <param name="json">The JSON to parse.</param>
    /// <returns>The account entity.</returns>
    public static BaseAccountEntityInfo Convert(JsonObjectNode json)
    {
        var type = GetAccountEntityType(json, AccountEntityTypes.Unknown);
        return type switch
        {
            AccountEntityTypes.User => new UserItemInfo(json),
            AccountEntityTypes.Group => new BaseUserGroupItemInfo(json),
            AccountEntityTypes.Service => new ServiceAccountItemInfo(json),
            AccountEntityTypes.Bot => new BotAccountItemInfo(json),
            AccountEntityTypes.Device => new AuthDeviceItemInfo(json),
            AccountEntityTypes.Agent => new AgentAccountItemInfo(json),
            AccountEntityTypes.Organization => new OrgAccountItemInfo(json),
            _ => null
        };
    }

    /// <summary>
    /// Gets the account entity type from the given JSON.
    /// </summary>
    /// <param name="json">The JSON input.</param>
    /// <param name="defaultType">The default type.</param>
    /// <returns>The account entity type.</returns>
    public static AccountEntityTypes GetAccountEntityType(JsonObjectNode json, AccountEntityTypes defaultType)
    {
        var type = json?.TryGetStringTrimmedValue("gender", true)?.ToLowerInvariant();
        if (type == null) return defaultType;
        return type switch
        {
            "u" or "user" or "account" or "用户" or "1" => AccountEntityTypes.User,
            "g" or "group" or "role" or "container" or "list" or "组" or "角色" or "2" => AccountEntityTypes.Group,
            "app" or "service" or "服务" or "3" => AccountEntityTypes.Service,
            "bot" or "robot" or "ai" or "assistance" or "machine" or "机器人" or "4" => AccountEntityTypes.Bot,
            "d" or "device" or "iot" or "client" or "设备" or "5" => AccountEntityTypes.Device,
            "agent" or "代理" or "6" => AccountEntityTypes.Agent,
            "org" or "organization" or "company" or "tenant" or "组织" or "7" => AccountEntityTypes.Organization,
            "default" or "-" => defaultType,
            _ => AccountEntityTypes.Other
        };
    }
}
