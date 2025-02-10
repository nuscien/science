using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Trivial.Data;
using Trivial.Reflection;
using Trivial.Tasks;
using Trivial.Text;
using Trivial.Web;
using Trivial.Users;

namespace Trivial.Devices;

/// <summary>
/// The device item information.
/// </summary>
[JsonConverter(typeof(AuthDeviceItemInfoConverter))]
public class AuthDeviceItemInfo : BaseUserItemInfo
{
    /// <summary>
    /// Initializes a new instance of the AuthDeviceItemInfo class.
    /// </summary>
    public AuthDeviceItemInfo()
        : base(PrincipalEntityTypes.Device)
    {
    }

    /// <summary>
    /// Initializes a new instance of the AuthDeviceItemInfo class.
    /// </summary>
    /// <param name="id">The resource identifier.</param>
    /// <param name="nickname">The nickname or display name.</param>
    /// <param name="avatar">The avatar URI.</param>
    public AuthDeviceItemInfo(string id, string nickname, Uri avatar = null)
        : base(PrincipalEntityTypes.Device, id, nickname, Genders.Asexual, avatar)
    {
    }

    /// <summary>
    /// Initializes a new instance of the AuthDeviceItemInfo class.
    /// </summary>
    /// <param name="json">The JSON object to parse.</param>
    public AuthDeviceItemInfo(JsonObjectNode json)
        : base(json, PrincipalEntityTypes.Device)
    {
        if (json == null) return;
        Manufacturer = json.TryGetStringTrimmedValue("manufacturer", true);
        ModelName = json.TryGetStringTrimmedValue("model", true);
        DeviceForm = json.TryGetStringTrimmedValue("form", true);
    }

    /// <summary>
    /// Gets or sets the manufacturer of the device.
    /// </summary>
    [DataMember(Name = "manufacturer")]
    [JsonPropertyName("manufacturer")]
    [Description("The manufacturer of the device.")]
    public string Manufacturer
    {
        get => GetCurrentProperty<string>();
        set => SetCurrentProperty(value);
    }

    /// <summary>
    /// Gets or sets the product name or model code of the device.
    /// </summary>
    [DataMember(Name = "model")]
    [JsonPropertyName("model")]
    [Description("The product name or model code of the device.")]
    public string ModelName
    {
        get => GetCurrentProperty<string>();
        set => SetCurrentProperty(value);
    }

    /// <summary>
    /// Gets or sets the device form.
    /// </summary>
    [DataMember(Name = "form")]
    [JsonPropertyName("form")]
    [Description("The device form.")]
    public string DeviceForm
    {
        get => GetCurrentProperty<string>();
        set => SetCurrentProperty(value);
    }

    /// <summary>
    /// Converts to JSON object.
    /// </summary>
    /// <returns>A JSON object.</returns>
    public override JsonObjectNode ToJson()
    {
        var json = base.ToJson();
        json.SetValue("manufacturer", Manufacturer);
        json.SetValue("model", ModelName);
        json.SetValue("form", DeviceForm);
        return json;
    }

    /// <summary>
    /// Converts the JSON raw back.
    /// </summary>
    /// <param name="value">The source value.</param>
    /// <returns>The request instance.</returns>
    public static implicit operator AuthDeviceItemInfo(JsonObjectNode value)
        => value is null ? null : new(value);

    /// <summary>
    /// Converts to JSON object.
    /// </summary>
    /// <param name="value">The JSON value.</param>
    /// <returns>A JSON object.</returns>
    public static explicit operator JsonObjectNode(AuthDeviceItemInfo value)
        => value?.ToJson();
}

/// <summary>
/// JSON value node converter.
/// </summary>
internal sealed class AuthDeviceItemInfoConverter : JsonObjectHostConverter<AuthDeviceItemInfo>
{
    /// <inheritdoc />
    protected override AuthDeviceItemInfo Create(JsonObjectNode json)
        => new(json);
}
