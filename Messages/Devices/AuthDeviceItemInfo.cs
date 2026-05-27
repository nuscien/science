using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Trivial.Data;
using Trivial.Reflection;
using Trivial.Tasks;
using Trivial.Text;
using Trivial.Users;
using Trivial.Web;

namespace Trivial.Devices;

/// <summary>
/// The entity of device which can be verified through identity authentication.
/// </summary>
[Guid("C01695B0-CA1C-4DED-8149-A149E55294DB")]
public class AuthDeviceItemInfo : BaseUserItemInfo
{
    /// <summary>
    /// Initializes a new instance of the AuthDeviceItemInfo class.
    /// </summary>
    public AuthDeviceItemInfo()
        : base(AccountEntityTypes.Device)
    {
    }

    /// <summary>
    /// Initializes a new instance of the AuthDeviceItemInfo class.
    /// </summary>
    /// <param name="args">The initialization arguments.</param>
    /// <param name="nickname">The nickname or display name.</param>
    /// <param name="avatar">The avatar URI.</param>
    public AuthDeviceItemInfo(ResourceEntityArgs args, string nickname = null, Uri avatar = null)
        : base(AccountEntityTypes.Device, args, nickname, Genders.Asexual, avatar)
    {
    }

    /// <summary>
    /// Initializes a new instance of the AuthDeviceItemInfo class.
    /// </summary>
    /// <param name="args">The initialization arguments.</param>
    public AuthDeviceItemInfo(AccountEntityArgs args)
        : base(AccountEntityTypes.Device, args, Genders.Asexual)
    {
    }

    /// <summary>
    /// Initializes a new instance of the AuthDeviceItemInfo class.
    /// </summary>
    /// <param name="id">The resource identifier.</param>
    /// <param name="nickname">The nickname or display name.</param>
    /// <param name="avatar">The avatar URI.</param>
    /// <param name="creation">The creation date time.</param>
    public AuthDeviceItemInfo(string id, string nickname, Uri avatar = null, DateTime? creation = null)
        : base(AccountEntityTypes.Device, id, nickname, Genders.Asexual, avatar, creation)
    {
    }

    /// <summary>
    /// Gets or sets the manufacturer of the device.
    /// </summary>
    [DataMember(Name = "manufacturer")]
    [JsonPropertyName("manufacturer")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [Description("The manufacturer of the device.")]
#if NETCOREAPP
    [Column("manufacturer")]
#endif
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
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [Description("The product name or model code of the device.")]
#if NETCOREAPP
    [Column("model")]
#endif
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
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [Description("The device form.")]
#if NETCOREAPP
    [Column("form")]
#endif
    public string DeviceForm
    {
        get => GetCurrentProperty<string>();
        set => SetCurrentProperty(value);
    }

    /// <summary>
    /// Gets or sets the thumbprint.
    /// </summary>
    [DataMember(Name = "thumbprint")]
    [JsonPropertyName("thumbprint")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [Description("The thumbprint of the device.")]
#if NETCOREAPP
    [Column("thumbprint")]
#endif
    public string Thumbprint
    {
        get => GetCurrentProperty<string>();
        set => SetCurrentProperty(value);
    }

    /// <summary>
    /// Generates claims of this entity.
    /// </summary>
    /// <param name="issuer">The optional claim issuer.</param>
    /// <returns>A collection of claim.</returns>
    public override IEnumerable<Claim> ToClaims(string issuer = null)
    {
        foreach (var claim in base.ToClaims(issuer))
        {
            yield return claim;
        }

        if (!string.IsNullOrWhiteSpace(Thumbprint)) yield return ResourceEntityUtils.ToClaim(ClaimTypes.Thumbprint, Thumbprint, issuer);
    }

    /// <inheritdoc />
    protected override void ToString(StringBuilder sb)
    {
        sb.AppendLine();
        sb.Append("Manufacturer = ");
        sb.Append(Manufacturer);
        sb.Append(" & Model = ");
        sb.Append(ModelName);
        sb.Append(" & Form = ");
        sb.Append(DeviceForm);
    }
}
