using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security.Claims;
using System.Security.Principal;
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
/// The base user item information.
/// </summary>
[JsonConverter(typeof(UserEntityInfoConverter))]
public abstract class BaseUserItemInfo : BaseAccountEntityInfo
{
    /// <summary>
    /// Initializes a new instance of the BaseUserItemInfo class.
    /// </summary>
    /// <param name="type">The account entity type.</param>
    internal BaseUserItemInfo(AccountEntityTypes type)
        : base(type)
    {
    }

    /// <summary>
    /// Initializes a new instance of the BaseUserItemInfo class.
    /// </summary>
    /// <param name="type">The account entity type.</param>
    /// <param name="args">The initialization arguments.</param>
    internal BaseUserItemInfo(AccountEntityTypes type, ResourceEntityArgs args)
        : base(type, args)
    {
    }

    /// <summary>
    /// Initializes a new instance of the BaseUserItemInfo class.
    /// </summary>
    /// <param name="type">The account entity type.</param>
    /// <param name="args">The initialization arguments.</param>
    /// <param name="gender">The gender.</param>
    internal BaseUserItemInfo(AccountEntityTypes type, AccountEntityArgs args, Genders gender)
        : base(type, args)
    {
        Gender = gender;
    }

    /// <summary>
    /// Initializes a new instance of the BaseUserItemInfo class.
    /// </summary>
    /// <param name="type">The account entity type.</param>
    /// <param name="args">The initialization arguments.</param>
    /// <param name="nickname">The nickname or display name.</param>
    /// <param name="gender">The gender.</param>
    /// <param name="avatar">The avatar URI.</param>
    internal BaseUserItemInfo(AccountEntityTypes type, ResourceEntityArgs args, string nickname, Genders gender, Uri avatar = null)
        : base(type, args, nickname, avatar)
    {
        Gender = gender;
    }

    /// <summary>
    /// Initializes a new instance of the BaseUserItemInfo class.
    /// </summary>
    /// <param name="type">The account entity type.</param>
    /// <param name="id">The resource identifier.</param>
    /// <param name="nickname">The nickname or display name.</param>
    /// <param name="gender">The gender.</param>
    /// <param name="avatar">The avatar URI.</param>
    /// <param name="creation">The creation date time.</param>
    internal BaseUserItemInfo(AccountEntityTypes type, string id, string nickname, Genders gender, Uri avatar = null, DateTime? creation = null)
        : base(type, id, nickname, avatar, creation)
    {
        Gender = gender;
    }

    /// <summary>
    /// Gets or sets the gender.
    /// </summary>
    [DataMember(Name = "gender")]
    [JsonPropertyName("gender")]
    [JsonConverter(typeof(JsonStringEnumCompatibleConverter))]
    [Description("The gender of the user.")]
#if NETCOREAPP
    [Column("gender")]
#endif
    public Genders Gender
    {
        get => GetCurrentProperty<Genders>();
        set => SetCurrentProperty(value);
    }

    /// <summary>
    /// Gets the claim of name identifier.
    /// </summary>
    /// <param name="issuer">The optional claim issuer.</param>
    /// <returns>The claim of name identifier of this entity.</returns>
    protected virtual Claim GetIdClaim(string issuer = null)
        => ResourceEntityUtils.ToClaim(ClaimTypes.NameIdentifier, Id, issuer);

    /// <summary>
    /// Generates claims of this entity.
    /// </summary>
    /// <param name="issuer">The optional claim issuer.</param>
    /// <returns>A collection of claim.</returns>
    public virtual IEnumerable<Claim> ToClaims(string issuer = null)
    {
        yield return GetIdClaim(issuer);
        yield return ResourceEntityUtils.ToClaim(ClaimTypes.Name, Nickname, issuer);
        yield return ResourceEntityUtils.ToClaim(ClaimTypes.Gender, Gender.ToString(), issuer);
    }
}

/// <summary>
/// The user account.
/// </summary>
[Guid("31309158-4064-4C32-A865-BED57EA88684")]
public class UserItemInfo : BaseUserItemInfo
{
    /// <summary>
    /// Initializes a new instance of the UserItemInfo class.
    /// </summary>
    public UserItemInfo()
        : base(AccountEntityTypes.User)
    {
    }

    /// <summary>
    /// Initializes a new instance of the UserItemInfo class.
    /// </summary>
    /// <param name="args">The initialization arguments.</param>
    /// <param name="nickname">The nickname or display name.</param>
    /// <param name="gender">The gender.</param>
    /// <param name="avatar">The avatar URI.</param>
    public UserItemInfo(ResourceEntityArgs args, string nickname = null, Genders gender = Genders.Unknown, Uri avatar = null)
        : base(gender == Genders.Asexual ? AccountEntityTypes.Bot : AccountEntityTypes.User, args, nickname, gender, avatar)
    {
    }

    /// <summary>
    /// Initializes a new instance of the UserItemInfo class.
    /// </summary>
    /// <param name="args">The initialization arguments.</param>
    /// <param name="gender">The gender.</param>
    public UserItemInfo(AccountEntityArgs args, Genders gender = Genders.Unknown)
        : base(gender == Genders.Asexual ? AccountEntityTypes.Bot : AccountEntityTypes.User, args, gender)
    {
    }

    /// <summary>
    /// Initializes a new instance of the UserItemInfo class.
    /// </summary>
    /// <param name="id">The resource identifier.</param>
    /// <param name="nickname">The nickname or display name.</param>
    /// <param name="gender">The gender.</param>
    /// <param name="avatar">The avatar URI.</param>
    /// <param name="creation">The creation date time.</param>
    public UserItemInfo(string id, string nickname, Genders gender = Genders.Unknown, Uri avatar = null, DateTime? creation = null)
        : base(gender == Genders.Asexual ? AccountEntityTypes.Bot : AccountEntityTypes.User, id, nickname, gender, avatar, creation)
    {
    }

    /// <summary>
    /// Gets or sets the primary account name or email address for login.
    /// </summary>
    [DataMember(Name = "logname")]
    [JsonPropertyName("logname")]
    [Description("The primary account name or email address for login.")]
#if NETCOREAPP
    [Column("logname")]
#endif
    public string LoginName
    {
        get => GetCurrentProperty<string>();
        set => SetCurrentProperty(value?.Trim());
    }

    /// <summary>
    /// Gets or sets the email address.
    /// </summary>
    [DataMember(Name = "email")]
    [JsonPropertyName("email")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [Description("The email address.")]
#if NETCOREAPP
    [Column("email")]
#endif
    public string Email
    {
        get => GetCurrentProperty<string>();
        set => SetCurrentProperty(value?.Trim());
    }

    /// <summary>
    /// Gets or sets the phone number.
    /// </summary>
    [DataMember(Name = "phone")]
    [JsonPropertyName("phone")]
    [Description("The phone number.")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#if NETCOREAPP
    [Column("phone")]
#endif
    public string PhoneNumber
    {
        get => GetCurrentProperty<string>();
        set => SetCurrentProperty(value?.Trim());
    }

    /// <summary>
    /// Gets or sets the birthday.
    /// </summary>
    [DataMember(Name = "birth")]
    [JsonPropertyName("birth")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [Description("The birthday.")]
#if NETCOREAPP
    [Column("birth")]
#endif
    public DateTime? Birthday
    {
        get => GetCurrentProperty<DateTime?>();
        set => SetCurrentProperty(value);
    }

    /// <inheritdoc />
    protected override void ToString(StringBuilder sb)
    {
        sb.AppendLine();
        sb.Append("Logname = ");
        sb.Append(LoginName);
        sb.Append(" & Gender = ");
        sb.Append(Gender);
        if (!string.IsNullOrWhiteSpace(Email) && Email != LoginName)
        {
            sb.Append(" & ");
            sb.Append("Email = ");
            sb.Append(Email);
        }

        if (!string.IsNullOrWhiteSpace(PhoneNumber) && PhoneNumber != LoginName)
        {
            sb.Append(" & ");
            sb.Append("Phone = ");
            sb.Append(PhoneNumber);
        }
    }

    /// <summary>
    /// Gets the claim of name identifier.
    /// </summary>
    /// <param name="issuer">The optional claim issuer.</param>
    /// <returns>The claim of name identifier of this entity.</returns>
    protected override Claim GetIdClaim(string issuer = null)
        => ResourceEntityUtils.ToClaim(ClaimTypes.NameIdentifier, LoginName ?? Id, issuer);

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

        if (!string.IsNullOrWhiteSpace(Email)) yield return ResourceEntityUtils.ToClaim(ClaimTypes.Email, Email, issuer);
        if (!string.IsNullOrWhiteSpace(PhoneNumber)) yield return ResourceEntityUtils.ToClaim(ClaimTypes.MobilePhone, PhoneNumber, issuer);
        if (Birthday.HasValue) yield return ResourceEntityUtils.ToClaim(ClaimTypes.DateOfBirth, Birthday.Value, true, issuer);
    }
}
