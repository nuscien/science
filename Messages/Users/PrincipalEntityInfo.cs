using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Trivial.Data;
using Trivial.Reflection;
using Trivial.Tasks;
using Trivial.Text;

namespace Trivial.Users;

/// <summary>
/// The principal entity information.
/// </summary>
public abstract class BasePrincipalEntityInfo : BaseResourceEntityInfo
{
    /// <summary>
    /// Initializes a new instance of the BasePrincipalEntityInfo class.
    /// </summary>
    /// <param name="type">The security principal entity type.</param>
    internal BasePrincipalEntityInfo(PrincipalEntityTypes type)
    {
        PrincipalEntityType = type;
    }

    /// <summary>
    /// Initializes a new instance of the BasePrincipalEntityInfo class.
    /// </summary>
    /// <param name="type">The security principal entity type.</param>
    /// <param name="id">The resource identifier.</param>
    /// <param name="nickname">The nickname or display name.</param>
    /// <param name="avatar">The avatar URI.</param>
    internal BasePrincipalEntityInfo(PrincipalEntityTypes type, string id, string nickname, Uri avatar = null)
        : this(type)
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
    internal BasePrincipalEntityInfo(PrincipalEntityTypes type, JsonObjectNode json)
        : this(type)
    {
        if (json == null) return;
        Id = json.TryGetStringTrimmedValue("id", true) ?? json.Id;
        Nickname = json.TryGetStringTrimmedValue("nickname", true);
        AvatarUri = json.TryGetUriValue("avatar");
        Bio = json.TryGetStringValue("bio");
        if (json.TryGetBooleanValue("_raw") != false) SetProperty("_raw", json);
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
        set => SetCurrentProperty(value);
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
