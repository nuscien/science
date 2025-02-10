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
using Trivial.Web;

namespace Trivial.Users;

/// <summary>
/// The organization item information.
/// </summary>
[JsonConverter(typeof(OrgAccountItemInfoConverter))]
public class OrgAccountItemInfo : BaseUserItemInfo, IBasicPublisherInfo
{
    /// <summary>
    /// Initializes a new instance of the OrgAccountItemInfo class.
    /// </summary>
    public OrgAccountItemInfo()
        : base(PrincipalEntityTypes.Organization)
    {
    }

    /// <summary>
    /// Initializes a new instance of the OrgAccountItemInfo class.
    /// </summary>
    /// <param name="id">The resource identifier.</param>
    /// <param name="nickname">The nickname or display name.</param>
    /// <param name="avatar">The avatar URI.</param>
    public OrgAccountItemInfo(string id, string nickname, Uri avatar = null)
        : base(PrincipalEntityTypes.Organization, id, nickname, Genders.Asexual, avatar)
    {
    }

    /// <summary>
    /// Initializes a new instance of the OrgAccountItemInfo class.
    /// </summary>
    /// <param name="json">The JSON object to parse.</param>
    public OrgAccountItemInfo(JsonObjectNode json)
        : base(json, PrincipalEntityTypes.Organization)
    {
        if (json == null) return;
        Website = json.TryGetUriValue("website");
    }

    /// <summary>
    /// Gets the publisher identifier.
    /// </summary>
    string IBasicPublisherInfo.Id => Id;

    /// <summary>
    /// Gets the publisher display name.
    /// </summary>
    string IBasicPublisherInfo.DisplayName => Nickname;

    /// <summary>
    /// Gets or sets the official website URI.
    /// </summary>
    [Description("The official website of the organization.")]
    public Uri Website
    {
        get => GetCurrentProperty<Uri>();
        set => SetCurrentProperty(value);
    }

    /// <summary>
    /// Converts to JSON object.
    /// </summary>
    /// <returns>A JSON object.</returns>
    public override JsonObjectNode ToJson()
    {
        var json = base.ToJson();
        json.SetValue("website", Website);
        return json;
    }

    /// <summary>
    /// Converts the JSON raw back.
    /// </summary>
    /// <param name="value">The source value.</param>
    /// <returns>The request instance.</returns>
    public static implicit operator OrgAccountItemInfo(JsonObjectNode value)
        => value is null ? null : new(value);

    /// <summary>
    /// Converts to JSON object.
    /// </summary>
    /// <param name="value">The JSON value.</param>
    /// <returns>A JSON object.</returns>
    public static explicit operator JsonObjectNode(OrgAccountItemInfo value)
        => value?.ToJson();
}

/// <summary>
/// JSON value node converter.
/// </summary>
internal sealed class OrgAccountItemInfoConverter : JsonObjectHostConverter<OrgAccountItemInfo>
{
    /// <inheritdoc />
    protected override OrgAccountItemInfo Create(JsonObjectNode json)
        => new(json);
}
