using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
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
    protected BasePrincipalEntityInfo(PrincipalEntityTypes type)
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
    protected BasePrincipalEntityInfo(PrincipalEntityTypes type, string id, string nickname, Uri avatar = null)
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
    protected BasePrincipalEntityInfo(PrincipalEntityTypes type, JsonObjectNode json)
        : this(type)
    {
        if (json == null) return;
        Id = json.TryGetStringTrimmedValue("id", true) ?? json.Id;
        Nickname = json.TryGetStringTrimmedValue("nickname", true);
        AvatarUri = json.TryGetUriValue("avatar");
        if (json.TryGetBooleanValue("_raw") != false) SetProperty("_raw", json);
    }

    /// <summary>
    /// Initializes a new instance of the BasePrincipalEntityInfo class.
    /// </summary>
    /// <param name="json">The JSON object to parse.</param>
    /// <param name="typeConverter">The security principal entity type converter.</param>
    /// <param name="defaultType">The default security principal entity type.</param>
    protected BasePrincipalEntityInfo(JsonObjectNode json, Func<JsonObjectNode, PrincipalEntityTypes> typeConverter, PrincipalEntityTypes defaultType = PrincipalEntityTypes.Unknown)
    {
        if (json == null)
        {
            PrincipalEntityType = defaultType;
            return;
        }

        PrincipalEntityType = typeConverter?.Invoke(json) ?? defaultType;
        Id = json.TryGetStringTrimmedValue("id", true) ?? json.Id;
        Nickname = json.TryGetStringTrimmedValue("nickname", true) ?? json.TryGetStringTrimmedValue("name", true);
        AvatarUri = json.TryGetUriValue("avatar") ?? json.TryGetUriValue("icon");
        if (json.TryGetBooleanValue("_raw") != false) SetProperty("_raw", json);
    }

    /// <summary>
    /// Gets the security principal entity type.
    /// </summary>
    [Description("This kind of entity can be used as an owner of the resource. This property is to define the type of the owner, e.g. a user, a user group, a service agent, etc.")]
    public PrincipalEntityTypes PrincipalEntityType { get; }

    /// <summary>
    /// Gets or sets the nickname.
    /// </summary>
    [Description("The nickname.")]
    public string Nickname
    {
        get => GetCurrentProperty<string>();
        set => SetCurrentProperty(value);
    }

    /// <summary>
    /// Gets or sets the URI of avatar.
    /// </summary>
    [Description("The URI of the avatar.")]
    public Uri AvatarUri
    {
        get => GetCurrentProperty<Uri>();
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
        json.SetValue("avatar", AvatarUri);
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
