using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
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
using Trivial.Web;

namespace Trivial.Users;

/// <summary>
/// The bot item information.
/// </summary>
[JsonConverter(typeof(BotAccountItemInfoConverter))]
[Guid("50381EBC-FAA7-41C9-B56C-32B0F82DF482")]
public class BotAccountItemInfo : BaseUserItemInfo
{
    /// <summary>
    /// Initializes a new instance of the BotAccountItemInfo class.
    /// </summary>
    public BotAccountItemInfo()
        : base(AccountEntityTypes.Bot)
    {
    }

    /// <summary>
    /// Initializes a new instance of the BotAccountItemInfo class.
    /// </summary>
    /// <param name="id">The resource identifier.</param>
    /// <param name="nickname">The nickname or display name.</param>
    /// <param name="gender">The gender.</param>
    /// <param name="avatar">The avatar URI.</param>
    /// <param name="publisher">The publisher info.</param>
    /// <param name="creation">The creation date time.</param>
    public BotAccountItemInfo(string id, string nickname, Genders gender, Uri avatar = null, IBasicPublisherInfo publisher = null, DateTime? creation = null)
        : base(AccountEntityTypes.Bot, id, nickname, gender, avatar, creation)
    {
        Publisher = publisher;
    }

    /// <summary>
    /// Initializes a new instance of the BotAccountItemInfo class.
    /// </summary>
    /// <param name="id">The resource identifier.</param>
    /// <param name="nickname">The nickname or display name.</param>
    /// <param name="avatar">The avatar URI.</param>
    /// <param name="creation">The creation date time.</param>
    public BotAccountItemInfo(string id, string nickname, Uri avatar = null, DateTime? creation = null)
        : base(AccountEntityTypes.Bot, id, nickname, Genders.Asexual, avatar, creation)
    {
    }

    /// <summary>
    /// Initializes a new instance of the BotAccountItemInfo class.
    /// </summary>
    /// <param name="json">The JSON object to parse.</param>
    protected internal BotAccountItemInfo(JsonObjectNode json)
        : base(json, AccountEntityTypes.Bot)
    {
    }

    /// <summary>
    /// Gets or sets the basic information of publisher, manufacturer or developer.
    /// </summary>
    [DataMember(Name = "publisher")]
    [JsonPropertyName("publisher")]
    [Description("The basic information of publisher, manufacturer or developer.")]
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
    public static implicit operator BotAccountItemInfo(JsonObjectNode value)
        => value is null ? null : new(value);

    /// <summary>
    /// Converts to JSON object.
    /// </summary>
    /// <param name="value">The JSON value.</param>
    /// <returns>A JSON object.</returns>
    public static explicit operator JsonObjectNode(BotAccountItemInfo value)
        => value?.ToJson();
}

/// <summary>
/// JSON value node converter.
/// </summary>
internal sealed class BotAccountItemInfoConverter : JsonObjectHostConverter<BotAccountItemInfo>
{
    /// <inheritdoc />
    protected override BotAccountItemInfo Create(JsonObjectNode json)
        => new(json);
}
