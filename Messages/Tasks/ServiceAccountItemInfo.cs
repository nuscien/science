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
using Trivial.Text;
using Trivial.Web;
using Trivial.Users;

namespace Trivial.Tasks;

/// <summary>
/// The service item information.
/// </summary>
[JsonConverter(typeof(ServiceAccountItemInfoConverter))]
public class ServiceAccountItemInfo : BaseUserItemInfo
{
    /// <summary>
    /// Initializes a new instance of the ServiceAccountItemInfo class.
    /// </summary>
    public ServiceAccountItemInfo()
        : base(PrincipalEntityTypes.Service)
    {
    }

    /// <summary>
    /// Initializes a new instance of the ServiceAccountItemInfo class.
    /// </summary>
    /// <param name="id">The resource identifier.</param>
    /// <param name="nickname">The nickname or display name.</param>
    /// <param name="avatar">The avatar URI.</param>
    /// <param name="publisher">The publisher info.</param>
    /// <param name="creation">The creation date time.</param>
    public ServiceAccountItemInfo(string id, string nickname, Uri avatar = null, IBasicPublisherInfo publisher = null, DateTime? creation = null)
        : base(PrincipalEntityTypes.Service, id, nickname, Genders.Asexual, avatar, creation)
    {
        Publisher = publisher;
    }

    /// <summary>
    /// Initializes a new instance of the ServiceAccountItemInfo class.
    /// </summary>
    /// <param name="json">The JSON object to parse.</param>
    protected internal ServiceAccountItemInfo(JsonObjectNode json)
        : base(json, PrincipalEntityTypes.Service)
    {
    }

    /// <summary>
    /// Gets or sets the basic information of publisher or developer.
    /// </summary>
    [DataMember(Name = "publisher")]
    [JsonPropertyName("publisher")]
    [Description("The basic information of publisher or developer.")]
    public IBasicPublisherInfo Publisher
    {
        get => GetCurrentProperty<IBasicPublisherInfo>();
        set => SetCurrentProperty(value);
    }

    /// <inheritdoc />
    protected override void Fill(JsonObjectNode json)
    {
        base.Fill(json);
        Publisher = TextHelper.ToPublisherInfo(json, "publisher");
    }

    /// <inheritdoc />
    protected override void ToString(StringBuilder sb)
    {
        var publisher = Publisher.DisplayName;
        if (string.IsNullOrWhiteSpace(publisher)) return;
        sb.AppendLine();
        sb.Append("Publisher = ");
        sb.Append(publisher);
        var pid = Publisher?.Id?.Trim();
        if (string.IsNullOrEmpty(pid)) return;
        sb.Append(" (");
        sb.Append(pid);
        sb.Append(')');
    }

    /// <summary>
    /// Converts to JSON object.
    /// </summary>
    /// <returns>A JSON object.</returns>
    public override JsonObjectNode ToJson()
    {
        var json = base.ToJson();
        var publisher = JsonObjectNode.ConvertFrom(Publisher);
        json.SetValue("publisher", publisher);
        return json;
    }

    /// <summary>
    /// Converts the JSON raw back.
    /// </summary>
    /// <param name="value">The source value.</param>
    /// <returns>The request instance.</returns>
    public static implicit operator ServiceAccountItemInfo(JsonObjectNode value)
        => value is null ? null : new(value);

    /// <summary>
    /// Converts to JSON object.
    /// </summary>
    /// <param name="value">The JSON value.</param>
    /// <returns>A JSON object.</returns>
    public static explicit operator JsonObjectNode(ServiceAccountItemInfo value)
        => value?.ToJson();
}

/// <summary>
/// JSON value node converter.
/// </summary>
internal sealed class ServiceAccountItemInfoConverter : JsonObjectHostConverter<ServiceAccountItemInfo>
{
    /// <inheritdoc />
    protected override ServiceAccountItemInfo Create(JsonObjectNode json)
        => new(json);
}
