using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Trivial.AI;
using Trivial.Data;
using Trivial.Devices;
using Trivial.Net;
using Trivial.Reflection;
using Trivial.Security;
using Trivial.Tasks;
using Trivial.Text;

namespace Trivial.Users;

/// <summary>
/// The base entity of users, groups and other kind of authorized entities.
/// </summary>
[JsonConverter(typeof(AccountEntityInfoConverter))]
public abstract class BaseAccountEntityInfo : BaseResourceEntityInfo
{
    /// <summary>
    /// Initializes a new instance of the BaseAccountEntityInfo class.
    /// </summary>
    /// <param name="type">The account entity type.</param>
    /// <param name="args">The initialization arguments.</param>
    internal BaseAccountEntityInfo(AccountEntityTypes type, ResourceEntityArgs args)
        : base(args)
    {
        AccountEntityType = type;
        if (args is not AccountEntityArgs a) return;
        Nickname = a.Nickname;
        AvatarUri = a.AvatarUri;
        Bio = a.Bio;
    }

    /// <summary>
    /// Initializes a new instance of the BaseAccountEntityInfo class.
    /// </summary>
    /// <param name="type">The account entity type.</param>
    /// <param name="args">The initialization arguments.</param>
    internal BaseAccountEntityInfo(AccountEntityTypes type, AccountEntityArgs args)
        : base(args)
    {
        AccountEntityType = type;
        if (args == null) return;
        Nickname = args.Nickname;
        AvatarUri = args.AvatarUri;
        Bio = args.Bio;
    }

    /// <summary>
    /// Initializes a new instance of the BaseAccountEntityInfo class.
    /// </summary>
    /// <param name="type">The account entity type.</param>
    /// <param name="creation">The creation date time.</param>
    internal BaseAccountEntityInfo(AccountEntityTypes type, DateTime? creation = null)
        : base(null, creation)
    {
        AccountEntityType = type;
    }

    /// <summary>
    /// Initializes a new instance of the BaseAccountEntityInfo class.
    /// </summary>
    /// <param name="type">The account entity type.</param>
    /// <param name="args">The initialization arguments.</param>
    /// <param name="nickname">The nickname or display name.</param>
    /// <param name="avatar">The avatar URI.</param>
    internal BaseAccountEntityInfo(AccountEntityTypes type, ResourceEntityArgs args, string nickname, Uri avatar = null)
        : base(args)
    {
        AccountEntityType = type;
        Nickname = nickname;
        AvatarUri = avatar;
    }

    /// <summary>
    /// Initializes a new instance of the BaseAccountEntityInfo class.
    /// </summary>
    /// <param name="type">The account entity type.</param>
    /// <param name="id">The resource identifier.</param>
    /// <param name="nickname">The nickname or display name.</param>
    /// <param name="avatar">The avatar URI.</param>
    /// <param name="creation">The creation date time.</param>
    internal BaseAccountEntityInfo(AccountEntityTypes type, string id, string nickname, Uri avatar = null, DateTime? creation = null)
        : base(null, creation)
    {
        Id = id;
        AccountEntityType = type;
        Nickname = nickname;
        AvatarUri = avatar;
    }

    /// <summary>
    /// Initializes a new instance of the BaseAccountEntityInfo class.
    /// </summary>
    /// <param name="type">The account entity type.</param>
    /// <param name="id">The resource identifier.</param>
    /// <param name="nickname">The nickname or display name.</param>
    /// <param name="creation">The creation date time.</param>
    /// <param name="modification">The last modification date time.</param>
    /// <param name="avatar">The avatar URI.</param>
    internal BaseAccountEntityInfo(AccountEntityTypes type, string id, string nickname, DateTime creation, DateTime modification, Uri avatar = null)
        : base(null, creation, modification)
    {
        Id = id;
        AccountEntityType = type;
        Nickname = nickname;
        AvatarUri = avatar;
    }

    /// <summary>
    /// Gets the account entity type.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenReading)]
    [Description("The types of this security account entity, e.g. a user, a user group, a service agent, etc.")]
#if NETCOREAPP
    [NotMapped]
#endif
    public AccountEntityTypes AccountEntityType { get; }

    /// <summary>
    /// Gets or sets the nickname.
    /// </summary>
    [DataMember(Name = "nickname")]
    [JsonPropertyName("nickname")]
    [Description("The nickname.")]
#if NETCOREAPP
    [Column("nickname")]
    [MaxLength(80)]
#endif
    public string Nickname
    {
        get => GetCurrentProperty<string>();
        set => SetCurrentProperty(value?.Trim());
    }

    /// <summary>
    /// Gets or sets the URI of avatar.
    /// </summary>
    [DataMember(Name = "avatar")]
    [JsonPropertyName("avatar")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    [Description("The URI of the avatar.")]
#if NETCOREAPP
    [Column("avatar")]
    [MaxLength(255)]
#endif
    public Uri AvatarUri
    {
        get => GetCurrentProperty<Uri>();
        set => SetCurrentProperty(value);
    }

    /// <summary>
    /// Gets or sets the introduction.
    /// </summary>
    [DataMember(Name = "bio", EmitDefaultValue = false)]
    [JsonPropertyName("bio")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    [Description("The introduction.")]
#if NETCOREAPP
    [Column("bio")]
#endif
    public string Bio
    {
        get => GetCurrentProperty<string>();
        set => SetCurrentProperty(value);
    }

    /// <summary>
    /// Gets the raw JSON object for reference.
    /// The value is null if no such data.
    /// </summary>
    [JsonInclude]
    [JsonPropertyName("raw")]
#if NETCOREAPP
    [NotMapped]
#endif
    protected JsonObjectNode RawJson
    {
        get => GetProperty<JsonObjectNode>("RawJson");
        private set => SetProperty("RawJson", value);
    }

    /// <summary>
    /// Gets the display name of this entity.
    /// </summary>
    /// <returns>The display name.</returns>
    public override string DisplayName => Nickname;

    /// <inheritdoc />
    protected override string Supertype => "account";

    /// <inheritdoc />
    protected override string ResourceType => AccountEntityType.ToString();

    /// <summary>
    /// Returns a string that represents this entity.
    /// </summary>
    /// <param name="sb">A string builder that represents this entity.</param>
    /// <returns>A string that represents this entity.</returns>
    protected virtual void ToString(StringBuilder sb)
    {
        sb.Append(Nickname ?? "?");
        sb.Append(" (");
        sb.Append(AccountEntityType.ToString());
        sb.Append(' ');
        sb.Append(Id ?? "-");
        sb.AppendLine(")");
    }

    /// <summary>
    /// Returns a string that represents this entity.
    /// </summary>
    /// <returns>A string that represents this entity.</returns>
    public override string ToString()
    {
        var sb = new StringBuilder();
        ToString(sb);
        return sb.ToString();
    }
}
