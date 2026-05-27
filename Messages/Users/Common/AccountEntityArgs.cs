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
using Trivial.Data;
using Trivial.Devices;
using Trivial.Net;
using Trivial.Reflection;
using Trivial.Security;
using Trivial.Tasks;
using Trivial.Text;

namespace Trivial.Users;

/// <summary>
/// The arguments to initializes an account resource entity.
/// </summary>
[Guid("DDB1CE37-021F-464D-8476-298977062400")]
public class AccountEntityArgs : ResourceEntityArgs
{
    /// <summary>
    /// Initializes a new instance of the AccountEntityArgs class.
    /// </summary>
    /// <param name="keepNullId">true if keep the identifier as null; otherwise, false.</param>
    /// <param name="nickname">The nickname or display name.</param>
    /// <param name="avatar">The avatar URI.</param>
    public AccountEntityArgs(bool keepNullId, string nickname = null, Uri avatar = null)
        : base(keepNullId)
    {
        Nickname = nickname;
        AvatarUri = avatar;
    }

    /// <summary>
    /// Initializes a new instance of the AccountEntityArgs class.
    /// </summary>
    public AccountEntityArgs()
        : base(false)
    {
    }

    /// <summary>
    /// Initializes a new instance of the AccountEntityArgs class.
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <param name="creation">The creation date time.</param>
    public AccountEntityArgs(Guid id, DateTime? creation = null)
        : base(id.ToString("N"), creation)
    {
    }

    /// <summary>
    /// Initializes a new instance of the AccountEntityArgs class.
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <param name="creation">The creation date time.</param>
    /// <param name="modification">The last modification date time.</param>
    public AccountEntityArgs(Guid id, DateTime creation, DateTime modification)
        : base(id.ToString("N"), creation, modification)
    {
    }

    /// <summary>
    /// Initializes a new instance of the AccountEntityArgs class.
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <param name="creation">The creation date time.</param>
    public AccountEntityArgs(string id, DateTime? creation = null)
        : base(id, creation)
    {
    }

    /// <summary>
    /// Initializes a new instance of the AccountEntityArgs class.
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <param name="creation">The creation date time.</param>
    /// <param name="modification">The last modification date time.</param>
    public AccountEntityArgs(string id, DateTime creation, DateTime modification)
        : base(id, creation, modification)
    {
    }

    /// <summary>
    /// Initializes a new instance of the AccountEntityArgs class.
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <param name="nickname">The nickname or display name.</param>
    /// <param name="avatar">The avatar URI.</param>
    /// <param name="creation">The creation date time.</param>
    public AccountEntityArgs(Guid id, string nickname, Uri avatar, DateTime? creation = null)
        : base(id.ToString("N"), creation)
    {
        Nickname = nickname;
        AvatarUri = avatar;
    }

    /// <summary>
    /// Initializes a new instance of the AccountEntityArgs class.
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <param name="nickname">The nickname or display name.</param>
    /// <param name="avatar">The avatar URI.</param>
    /// <param name="creation">The creation date time.</param>
    /// <param name="modification">The last modification date time.</param>
    public AccountEntityArgs(Guid id, string nickname, Uri avatar, DateTime creation, DateTime modification)
        : base(id.ToString("N"), creation, modification)
    {
        Nickname = nickname;
        AvatarUri = avatar;
    }

    /// <summary>
    /// Initializes a new instance of the AccountEntityArgs class.
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <param name="nickname">The nickname or display name.</param>
    /// <param name="avatar">The avatar URI.</param>
    /// <param name="creation">The creation date time.</param>
    public AccountEntityArgs(string id, string nickname, Uri avatar, DateTime? creation = null)
        : base(id, creation)
    {
        Nickname = nickname;
        AvatarUri = avatar;
    }

    /// <summary>
    /// Initializes a new instance of the AccountEntityArgs class.
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <param name="nickname">The nickname or display name.</param>
    /// <param name="avatar">The avatar URI.</param>
    /// <param name="creation">The creation date time.</param>
    /// <param name="modification">The last modification date time.</param>
    public AccountEntityArgs(string id, string nickname, Uri avatar, DateTime creation, DateTime modification)
        : base(id, creation, modification)
    {
        Nickname = nickname;
        AvatarUri = avatar;
    }

    /// <summary>
    /// Gets or sets the nickname.
    /// </summary>
    [DataMember(Name = "nickname")]
    [JsonPropertyName("nickname")]
    [Description("The nickname.")]
    public string Nickname { get; set; }

    /// <summary>
    /// Gets or sets the URI of avatar.
    /// </summary>
    [DataMember(Name = "avatar")]
    [JsonPropertyName("avatar")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    [Description("The URI of the avatar.")]
    public Uri AvatarUri { get; set; }

    /// <summary>
    /// Gets or sets the introduction.
    /// </summary>
    [DataMember(Name = "bio", EmitDefaultValue = false)]
    [JsonPropertyName("bio")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    [Description("The introduction.")]
    public string Bio { get; set; }
}
