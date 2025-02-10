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

namespace Trivial.Users;

/// <summary>
/// The bot item information.
/// </summary>
[JsonConverter(typeof(BotAccountItemInfoConverter))]
public class BotAccountItemInfo : BaseUserItemInfo
{
    /// <summary>
    /// Initializes a new instance of the BotAccountItemInfo class.
    /// </summary>
    public BotAccountItemInfo()
        : base(PrincipalEntityTypes.Bot)
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
    public BotAccountItemInfo(string id, string nickname, Genders gender, Uri avatar = null, IBasicPublisherInfo publisher = null)
        : base(PrincipalEntityTypes.Bot, id, nickname, gender, avatar)
    {
        Publisher = publisher;
    }

    /// <summary>
    /// Initializes a new instance of the BotAccountItemInfo class.
    /// </summary>
    /// <param name="id">The resource identifier.</param>
    /// <param name="nickname">The nickname or display name.</param>
    /// <param name="avatar">The avatar URI.</param>
    public BotAccountItemInfo(string id, string nickname, Uri avatar = null)
        : base(PrincipalEntityTypes.Bot, id, nickname, Genders.Asexual, avatar)
    {
    }

    /// <summary>
    /// Initializes a new instance of the BotAccountItemInfo class.
    /// </summary>
    /// <param name="json">The JSON object to parse.</param>
    public BotAccountItemInfo(JsonObjectNode json)
        : base(json, PrincipalEntityTypes.Bot)
    {
        if (json == null) return;
        Publisher = TextHelper.ToPublisherInfo(json, "publisher");
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
