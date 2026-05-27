using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
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
using Trivial.Users;
using System.Net.Http;

namespace Trivial.AI;

/// <summary>
/// The bot account, including AI agent.
/// </summary>
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
    /// <param name="args">The initialization arguments.</param>
    /// <param name="nickname">The nickname or display name.</param>
    /// <param name="gender">The gender.</param>
    /// <param name="avatar">The avatar URI.</param>
    /// <param name="publisher">The publisher info.</param>
    public BotAccountItemInfo(ResourceEntityArgs args, string nickname, Genders gender, Uri avatar = null, IBasicPublisherInfo publisher = null)
        : base(AccountEntityTypes.Bot, args, nickname, gender, avatar)
    {
        Publisher = publisher;
    }

    /// <summary>
    /// Initializes a new instance of the BotAccountItemInfo class.
    /// </summary>
    /// <param name="args">The initialization arguments.</param>
    /// <param name="gender">The gender.</param>
    /// <param name="publisher">The publisher info.</param>
    public BotAccountItemInfo(AccountEntityArgs args, Genders gender, IBasicPublisherInfo publisher = null)
        : base(AccountEntityTypes.Bot, args, gender)
    {
        Publisher = publisher;
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
    /// Gets or sets the basic information of publisher, manufacturer or developer.
    /// </summary>
    [DataMember(Name = "publisher")]
    [JsonPropertyName("publisher")]
    [JsonConverter(typeof(GenericPublisherInfoConverter))]
    [Description("The basic information of publisher, manufacturer or developer.")]
#if NETCOREAPP
    [NotMapped]
#endif
    public IBasicPublisherInfo Publisher // ToDo: DB column
    {
        get => GetCurrentProperty<IBasicPublisherInfo>();
        set => SetCurrentProperty(value);
    }

    /// <summary>
    /// Gets or sets the optional argument of the bot settings.
    /// </summary>
    [DataMember(Name = "arg")]
    [JsonPropertyName("arg")]
    [JsonConverter(typeof(GenericPublisherInfoConverter))]
    [Description("The argument of the bot settings.")]
#if NETCOREAPP
    [NotMapped]
#endif
    public string Argument
    {
        get => GetCurrentProperty<string>();
        set => SetCurrentProperty(value);
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
}

public class BotAccountItemSettings
{
    public Uri Endpoint { get; set; }

    public string Argument { get; set; }

    public HttpMethod HttpMethod { get; set; }
}
