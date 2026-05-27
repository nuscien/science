using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Trivial.AI;
using Trivial.Data;
using Trivial.Devices;
using Trivial.Security;
using Trivial.Tasks;
using Trivial.Text;
using Trivial.Users;

namespace Trivial.Users;

/// <summary>
/// The resource type info of account entity.
/// </summary>
public static class AccountEntityFactory
{
    /// <summary>
    /// The registry of account entity type information.
    /// </summary>
    private static readonly Dictionary<AccountEntityTypes, Type> registry = new();

    /// <summary>
    /// Registers a runtime type of account entity.
    /// </summary>
    /// <typeparam name="T">The runtime type to register.</typeparam>
    /// <returns>The type register, if registers succeeded; or null, if fails.</returns>
    public static Type Register<T>() where T : BaseAccountEntityInfo
    {
        var type = typeof(T);
        return Register(type) ? type : null;
    }

    /// <summary>
    /// Registers a runtime type of account entity.
    /// </summary>
    /// <param name="type">The runtime type to register.</param>
    /// <returns>true if registers succeeded; otherwise, false.</returns>
    public static bool Register(Type type)
    {
        if (type == null || !type.IsClass || !type.IsSubclassOf(typeof(BaseAccountEntityInfo))) return false;
        var constructor = type.GetConstructor(Type.EmptyTypes);
        if (constructor == null) return false;
        if (type.IsSubclassOf(typeof(UserItemInfo)) || type == typeof(UserItemInfo)) registry[AccountEntityTypes.User] = type;
        else if (type.IsSubclassOf(typeof(AuthDeviceItemInfo)) || type == typeof(AuthDeviceItemInfo)) registry[AccountEntityTypes.Device] = type;
        else if (type.IsSubclassOf(typeof(ServiceAccountItemInfo)) || type == typeof(ServiceAccountItemInfo)) registry[AccountEntityTypes.Service] = type;
        else if (type.IsSubclassOf(typeof(BotAccountItemInfo)) || type == typeof(BotAccountItemInfo)) registry[AccountEntityTypes.Bot] = type;
        else if (type.IsSubclassOf(typeof(OrgAccountItemInfo)) || type == typeof(OrgAccountItemInfo)) registry[AccountEntityTypes.Organization] = type;
        else if (type.IsSubclassOf(typeof(AgentAccountItemInfo)) || type == typeof(AgentAccountItemInfo)) registry[AccountEntityTypes.Agent] = type;
        else if (type.IsSubclassOf(typeof(BaseUserGroupItemInfo)) || type == typeof(BaseUserGroupItemInfo)) registry[AccountEntityTypes.Group] = type;
        else if (type.IsSubclassOf(typeof(ApplicationCustomizedAccountEntityInfo)) || type == typeof(ApplicationCustomizedAccountEntityInfo)) registry[AccountEntityTypes.Customized] = type;
        else return false;
        return true;
    }

    /// <summary>
    /// Gets the runtime type of account entity registered.
    /// </summary>
    /// <param name="type">The account entity type.</param>
    /// <returns>The runtime type used to manage the account information if found; or null, if does not exist or does not support.</returns>
    public static Type GetType(AccountEntityTypes type)
    {
        if (!registry.TryGetValue(type, out var result)) return null;
        return result ?? type switch
        {
            AccountEntityTypes.User => typeof(UserItemInfo),
            AccountEntityTypes.Device => typeof(AuthDeviceItemInfo),
            AccountEntityTypes.Service => typeof(ServiceAccountItemInfo),
            AccountEntityTypes.Bot => typeof(BotAccountItemInfo),
            AccountEntityTypes.Organization => typeof(OrgAccountItemInfo),
            AccountEntityTypes.Agent => typeof(AgentAccountItemInfo),
            AccountEntityTypes.Group => typeof(BaseUserGroupItemInfo),
            AccountEntityTypes.Customized => typeof(ApplicationCustomizedAccountEntityInfo),
            AccountEntityTypes.Unknown => typeof(UnknownAccountEntityInfo),
            _ => null
        };
    }

    /// <summary>
    /// Gets the runtime type of account entity registered.
    /// </summary>
    /// <param name="json">The JSON input.</param>
    /// <returns>The runtime type used to manage the account information if found; or null, if does not exist.</returns>
    public static Type GetType(JsonObjectNode json)
    {
        var kind = GetAccountEntityType(json, AccountEntityTypes.Other);
        return kind == AccountEntityTypes.Other ? null : GetType(kind);
    }

    /// <summary>
    /// Creates an instance.
    /// </summary>
    /// <param name="type">The account entity type.</param>
    /// <returns>An instance of account entity.</returns>
    public static BaseAccountEntityInfo CreateInstance(AccountEntityTypes type)
    {
        var t = GetType(type);
        if (t is null) return null;
        return Activator.CreateInstance(t) as BaseAccountEntityInfo;
    }

    /// <summary>
    /// Gets the account entity type from the given JSON.
    /// </summary>
    /// <param name="json">The JSON input.</param>
    /// <param name="defaultType">The default type.</param>
    /// <returns>The account entity type.</returns>
    public static AccountEntityTypes GetAccountEntityType(JsonObjectNode json, AccountEntityTypes defaultType = AccountEntityTypes.Unknown)
    {
        if (json is null) return defaultType;
        var type = json.TryGetStringTrimmedValue("$type", true)?.ToLowerInvariant() ?? json.TryGetStringTrimmedValue("gender", true)?.ToLowerInvariant();
        if (type == null) return defaultType;
        return type switch
        {
            "u" or "user" or "account" or "male" or "female" or "用户" or "男" or "女" or "1" => AccountEntityTypes.User,
            "g" or "group" or "role" or "container" or "list" or "组" or "群" or "角色" or "2" => AccountEntityTypes.Group,
            "app" or "service" or "服务" or "3" => AccountEntityTypes.Service,
            "bot" or "robot" or "ai" or "assistance" or "machine" or "机器人" or "4" => AccountEntityTypes.Bot,
            "d" or "device" or "authdevice" or "iot" or "client" or "设备" or "5" => AccountEntityTypes.Device,
            "agent" or "代理" or "6" => AccountEntityTypes.Agent,
            "org" or "organization" or "company" or "school" or "goverment" or "institude" or "tenant" or "组织" or "7" => AccountEntityTypes.Organization,
            "customize" or "customization" or "自定义" or "62" => AccountEntityTypes.Customized,
            "unknown" or "未知" or "?" or "0" => AccountEntityTypes.Unknown,
            "default" or "-" => defaultType,
            _ => AccountEntityTypes.Other
        };
    }

    /// <summary>
    /// Parses a JSON string to account entity instance.
    /// </summary>
    /// <param name="s">The string in JSON format.</param>
    /// <param name="options">Options to control the reader behavior during parsing.</param>
    /// <returns>An instance of the account entity.</returns>
    public static BaseAccountEntityInfo Deserialize(string s, JsonDocumentOptions options = default)
    {
        var json = JsonObjectNode.Parse(s, options);
        if (json is null) return null;
        var type = GetType(json) ?? typeof(UnknownAccountEntityInfo);
        return json.Deserialize(type) as BaseAccountEntityInfo;
    }

    /// <summary>
    /// Parses a JSON string to account entity instance.
    /// </summary>
    /// <param name="s">The string in JSON format.</param>
    /// <param name="options">An object that specifies serialization options to use.</param>
    /// <returns>An instance of the account entity.</returns>
    public static BaseAccountEntityInfo Deserialize(string s, JsonSerializerOptions options)
    {
        var json = JsonObjectNode.Parse(s);
        if (json is null) return null;
        var type = GetType(json) ?? typeof(UnknownAccountEntityInfo);
        return json.Deserialize(type, options) as BaseAccountEntityInfo;
    }

    /// <summary>
    /// Parses a JSON string to account entity instance.
    /// </summary>
    /// <param name="s">The string in JSON format.</param>
    /// <param name="options">An object that specifies serialization options to use.</param>
    /// <returns>An instance of the account entity.</returns>
    public static T Deserialize<T>(string s, JsonSerializerOptions options)
        where T : BaseAccountEntityInfo, new()
        => string.IsNullOrWhiteSpace(s) ? null : JsonSerializer.Deserialize<T>(s, options);

    /// <summary>
    /// Parses a JSON string to account entity instance.
    /// </summary>
    /// <param name="reader">The reader.</param>
    /// <param name="options">An object that specifies serialization options to use.</param>
    /// <returns>An instance of the account entity.</returns>
    public static BaseAccountEntityInfo Deserialize(ref Utf8JsonReader reader, JsonSerializerOptions options = null)
    {
        var json = JsonObjectNode.ParseValue(ref reader);
        if (json is null) return null;
        var type = GetType(json) ?? typeof(UnknownAccountEntityInfo);
        return json.Deserialize(type, options) as BaseAccountEntityInfo;
    }

    /// <summary>
    /// Converts the JSON to the account entity.
    /// </summary>
    /// <param name="json">The JSON to parse.</param>
    /// <returns>The account entity.</returns>
    public static BaseAccountEntityInfo Convert(JsonObjectNode json)
    {
        if (json is null) return null;
        var type = GetType(json) ?? typeof(UnknownAccountEntityInfo);
        return json.Deserialize(type) as BaseAccountEntityInfo;
    }

    /// <summary>
    /// Converts an account entity to a JSON object node.
    /// </summary>
    /// <param name="obj">The object to convert to JSON.</param>
    /// <returns>A JSON object node instance.</returns>
    public static JsonObjectNode ToJson<T>(T obj)
        where T : BaseAccountEntityInfo
        => JsonObjectNode.ConvertFrom(obj);
}

/// <summary>
/// The JSON converter of account entity.
/// </summary>
internal sealed class AccountEntityInfoConverter : JsonConverter<BaseAccountEntityInfo>
{
    /// <inheritdoc />
    public override BaseAccountEntityInfo Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => AccountEntityFactory.Deserialize(ref reader, options);

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, BaseAccountEntityInfo value, JsonSerializerOptions options)
    {
        var json = JsonObjectNode.ConvertFrom(value);
        if (json is null) writer.WriteNullValue();
        else json.WriteTo(writer);
    }
}

/// <summary>
/// The JSON converter of user entity.
/// </summary>
internal sealed class UserEntityInfoConverter : JsonConverter<BaseUserItemInfo>
{
    /// <inheritdoc />
    public override BaseUserItemInfo Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var instance = AccountEntityFactory.Deserialize(ref reader, options);
        if (instance is BaseUserItemInfo user) return user;
        throw new JsonException($"The resource type is not allowed. The current is {instance.GetProperty(ResourceEntitySpecialProperties.ResourceType)}.");
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, BaseUserItemInfo value, JsonSerializerOptions options)
    {
        var json = JsonObjectNode.ConvertFrom(value);
        if (json is null) writer.WriteNullValue();
        else json.WriteTo(writer);
    }
}
