using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Trivial.Data;
using Trivial.Reflection;
using Trivial.Tasks;
using Trivial.Text;

namespace Trivial.Users;

/// <summary>
/// The user item information.
/// </summary>
[JsonConverter(typeof(UserItemInfoConverter))]
public class UserItemInfo : BasePrincipalEntityInfo
{
    /// <summary>
    /// Initializes a new instance of the UserItemInfo class.
    /// </summary>
    public UserItemInfo()
        : base(PrincipalEntityTypes.User)
    {
    }

    /// <summary>
    /// Initializes a new instance of the UserItemInfo class.
    /// </summary>
    public UserItemInfo(string id, string nickname, Genders gender = Genders.Unknown, Uri avatar = null)
        : base(gender == Genders.Machine ? PrincipalEntityTypes.Bot : PrincipalEntityTypes.User, id, nickname, avatar)
    {
        Gender = gender;
    }

    /// <summary>
    /// Initializes a new instance of the UserItemInfo class.
    /// </summary>
    /// <param name="json">The JSON object to parse.</param>
    public UserItemInfo(JsonObjectNode json)
        : base(json, GetPrincipalEntityType, PrincipalEntityTypes.User)
    {
        if (json == null) return;
        Gender = json.TryGetEnumValue<Genders>("gender") ?? Genders.Unknown;
    }

    /// <summary>
    /// Gets or sets the gender.
    /// </summary>
    [Description("The gender of the user.")]
    public Genders Gender
    {
        get => GetCurrentProperty<Genders>();
        set => SetCurrentProperty(value);
    }

    /// <summary>
    /// Converts to JSON object.
    /// </summary>
    /// <returns>A JSON object.</returns>
    public override JsonObjectNode ToJson()
    {
        var json = base.ToJson();
        json.SetValue("gender", Gender.ToString());
        return json;
    }

    /// <summary>
    /// Converts the JSON raw back.
    /// </summary>
    /// <param name="value">The source value.</param>
    /// <returns>The request instance.</returns>
    public static implicit operator UserItemInfo(JsonObjectNode value)
        => value is null ? null : new(value);

    /// <summary>
    /// Converts to JSON object.
    /// </summary>
    /// <param name="value">The JSON value.</param>
    /// <returns>A JSON object.</returns>
    public static explicit operator JsonObjectNode(UserItemInfo value)
        => value?.ToJson();

    private static PrincipalEntityTypes GetPrincipalEntityType(JsonObjectNode json)
    {
        var type = json?.TryGetStringTrimmedValue("gender", true)?.ToLowerInvariant();
        if (type == null) return PrincipalEntityTypes.User;
        return type switch
        {
            "u" or "user" or "account" or "用户" or "1" => PrincipalEntityTypes.User,
            "g" or "group" or "role" or "container" or "list" or "组" or "角色" or "2" => PrincipalEntityTypes.Group,
            "app" or "service" or "服务" or "3" => PrincipalEntityTypes.Service,
            "b" or "bot" or "robot" or "ai" or "assistance" or "machine" or "机器人" or "4" => PrincipalEntityTypes.Bot,
            "d" or "device" or "iot" or "client" or "设备" or "5" => PrincipalEntityTypes.Device,
            "agent" or "代理" or "6" => PrincipalEntityTypes.Agent,
            _ => PrincipalEntityTypes.Other
        };
    }
}

/// <summary>
/// JSON value node converter.
/// </summary>
internal sealed class UserItemInfoConverter : JsonObjectHostConverter<UserItemInfo>
{
    /// <inheritdoc />
    protected override UserItemInfo Create(JsonObjectNode json)
        => new(json);
}
