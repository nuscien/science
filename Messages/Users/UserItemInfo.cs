using System;
using System.Collections.Generic;
using System.ComponentModel;
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
/// The user item information.
/// </summary>
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
    /// Initializes a new instance of the BaseUserItemInfo class.
    /// </summary>
    /// <param name="json">The JSON object to parse.</param>
    /// <param name="defaultType">The default account entity type.</param>
    internal BaseUserItemInfo(JsonObjectNode json, AccountEntityTypes defaultType)
        : base(defaultType, json, true)
    {
    }

    /// <summary>
    /// Gets or sets the gender.
    /// </summary>
    [DataMember(Name = "gender")]
    [JsonPropertyName("gender")]
    [JsonConverter(typeof(JsonStringEnumCompatibleConverter))]
    [Description("The gender of the user.")]
    public Genders Gender
    {
        get => GetCurrentProperty<Genders>();
        set => SetCurrentProperty(value);
    }

    /// <inheritdoc />
    protected override void Fill(JsonObjectNode json)
    {
        base.Fill(json);
        Gender = json.TryGetEnumValue<Genders>("gender") ?? Genders.Unknown;
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

    /// <summary>
    /// Converts to JSON object.
    /// </summary>
    /// <returns>A JSON object.</returns>
    public override JsonObjectNode ToJson()
    {
        var json = base.ToJson();
        json.SetValue("gender", Gender.ToString());
        return json;
    }
}

/// <summary>
/// The user item information.
/// </summary>
[JsonConverter(typeof(UserItemInfoConverter))]
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
    /// Initializes a new instance of the UserItemInfo class.
    /// </summary>
    /// <param name="json">The JSON object to parse.</param>
    protected internal UserItemInfo(JsonObjectNode json)
        : base(json, AccountEntityTypes.User)
    {
    }

    /// <summary>
    /// Gets or sets the primary account name or email address for login.
    /// </summary>
    [DataMember(Name = "logname")]
    [JsonPropertyName("logname")]
    [Description("The primary account name or email address for login.")]
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
    [Description("The email address.")]
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
    public DateTime? Birthday
    {
        get => GetCurrentProperty<DateTime?>();
        set => SetCurrentProperty(value);
    }

    /// <inheritdoc />
    protected override void Fill(JsonObjectNode json)
    {
        base.Fill(json);
        LoginName = json.TryGetStringTrimmedValue("logname");
        Birthday = json.TryGetDateTimeValue("birth");
        Email = json.TryGetStringTrimmedValue("email");
        PhoneNumber = json.TryGetStringTrimmedValue("phone");
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

    /// <summary>
    /// Converts to JSON object.
    /// </summary>
    /// <returns>A JSON object.</returns>
    public override JsonObjectNode ToJson()
    {
        var json = base.ToJson();
        json.SetValueIfNotEmpty("logname", LoginName);
        if (Birthday.HasValue) json.SetValue("birth", Birthday.Value);
        else json.Remove("birth");
        json.SetValue("email", Email);
        json.SetValue("phone", PhoneNumber);
        return json;
    }

    /// <summary>
    /// Converts the JSON raw back.
    /// </summary>
    /// <param name="value">The source value.</param>
    /// <returns>The request instance.</returns>
    public static implicit operator UserItemInfo(JsonObjectNode value)
        => value is null ? null : new(value);

    /// <summary>
    /// Converts to JSON object.
    /// </summary>
    /// <param name="value">The JSON value.</param>
    /// <returns>A JSON object.</returns>
    public static explicit operator JsonObjectNode(UserItemInfo value)
        => value?.ToJson();
}

/// <summary>
/// JSON value node converter.
/// </summary>
internal sealed class UserItemInfoConverter : JsonObjectHostConverter<UserItemInfo>
{
    /// <inheritdoc />
    protected override UserItemInfo Create(JsonObjectNode json)
        => new(json);
}
