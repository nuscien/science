using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Xml.Linq;
using Trivial.Data;
using Trivial.Text;
using Trivial.Users;

namespace Trivial.Security;

/// <summary>
/// The credential name types.
/// </summary>
public enum UserCredentialNameTypes : byte
{
    /// <summary>
    /// The login name.
    /// </summary>
    Logname = 0,

    /// <summary>
    /// The email address.
    /// </summary>
    Email = 1,

    /// <summary>
    /// The phone number.
    /// </summary>
    Phone = 2,

    /// <summary>
    /// A kind of ID number.
    /// </summary>
    Identifier = 3,

    /// <summary>
    /// Social network account name.
    /// </summary>
    Sns = 5,

    /// <summary>
    /// The other type.
    /// </summary>
    Other = 63,
}

/// <summary>
/// The credential key types.
/// </summary>
public enum UserCredentialKeyTypes : byte
{
    /// <summary>
    /// None.
    /// </summary>
    None = 0,

    /// <summary>
    /// The tranditional password.
    /// </summary>
    Password = 1,

    /// <summary>
    /// The access token, authentication code or app credential key.
    /// </summary>
    Token = 2,

    /// <summary>
    /// The public certificate of passkey.
    /// </summary>
    Passkey = 3,

    /// <summary>
    /// The one-time passcode or other random immediate passcode.
    /// </summary>
    Otp = 4,

    /// <summary>
    /// The seed of two-step authentication.
    /// </summary>
    Seed = 5,

    /// <summary>
    /// The private certificate of SSO or other kind of distribution authorized.
    /// </summary>
    Certificate = 6,

    /// <summary>
    /// The biometrics raw data.
    /// </summary>
    Biometrics = 7,

    /// <summary>
    /// The trusted client identifier, footprint or credential secret.
    /// </summary>
    Client = 15,

    /// <summary>
    /// The 3rd-party authentication service.
    /// </summary>
    Service = 16,

    /// <summary>
    /// The other type.
    /// </summary>
    Other = 63,
}

/// <summary>
/// The credential name of a user account item.
/// </summary>
[JsonConverter(typeof(UserItemCredentialNameInfoConverter))]
public class UserItemCredentialNameInfo : UserItemRelatedInfo
{
    /// <summary>
    /// Intializes a new instance of the UserItemCredentialNameInfo class.
    /// </summary>
    public UserItemCredentialNameInfo()
    {
    }

    /// <summary>
    /// Intializes a new instance of the UserItemCredentialNameInfo class.
    /// </summary>
    /// <param name="id">The entity identifer.</param>
    /// <param name="owner">The owner.</param>
    /// <param name="type">The type of credential name.</param>
    /// <param name="subType">The sub-type.</param>
    /// <param name="name">The value of name.</param>
    /// <param name="descripiton">The description.</param>
    /// <param name="creation">The creation date time.</param>
    public UserItemCredentialNameInfo(string id, BaseAccountEntityInfo owner, UserCredentialNameTypes type, string subType, string name, string descripiton = null, DateTime? creation = null)
        : base(id, owner, creation)
    {
        CredentialType = type;
        SubType = subType;
        Name = name;
        Description = descripiton;
    }

    /// <summary>
    /// Intializes a new instance of the UserItemCredentialNameInfo class.
    /// </summary>
    /// <param name="id">The entity identifer.</param>
    /// <param name="owner">The owner.</param>
    /// <param name="type">The type of credential name.</param>
    /// <param name="subType">The sub-type.</param>
    /// <param name="name">The value of name.</param>
    /// <param name="descripiton">The description.</param>
    /// <param name="creation">The creation date time.</param>
    public UserItemCredentialNameInfo(Guid id, BaseAccountEntityInfo owner, UserCredentialNameTypes type, string subType, string name, string descripiton = null, DateTime? creation = null)
        : base(id, owner, creation)
    {
        CredentialType = type;
        SubType = subType;
        Name = name;
        Description = descripiton;
    }

    /// <summary>
    /// Intializes a new instance of the UserItemCredentialNameInfo class.
    /// </summary>
    /// <param name="json">The JSON object to parse.</param>
    public UserItemCredentialNameInfo(JsonObjectNode json)
        : base(json)
    {
    }

    /// <summary>
    /// Gets or sets the credential name type.
    /// </summary>
    public UserCredentialNameTypes CredentialType
    {
        get => GetCurrentProperty<UserCredentialNameTypes>();
        set => SetCurrentProperty(value);
    }

    /// <summary>
    /// Gets or sets the sub-type.
    /// </summary>
    public string SubType
    {
        get => GetCurrentProperty<string>();
        set => SetCurrentProperty(value);
    }

    /// <summary>
    /// Gets or sets the value of the name.
    /// </summary>
    public string Name
    {
        get => GetCurrentProperty<string>();
        set => SetCurrentProperty(value);
    }

    /// <summary>
    /// Gets or sets the description.
    /// </summary>
    public string Description
    {
        get => GetCurrentProperty<string>();
        set => SetCurrentProperty(value);
    }

    /// <inheritdoc />
    [JsonIgnore]
#if NETCOREAPP
    [NotMapped]
#endif
    protected override string Supertype => "logname";

    /// <inheritdoc />
    [JsonIgnore]
#if NETCOREAPP
    [NotMapped]
#endif
    protected override string ResourceType => CredentialType.ToString();

    /// <inheritdoc />
    protected override void Fill(JsonObjectNode json)
    {
        base.Fill(json);
        CredentialType = json.TryGetEnumValue<UserCredentialNameTypes>("type") ?? UserCredentialNameTypes.Logname;
        SubType = json.TryGetStringTrimmedValue("subtype", true);
        Name = json.TryGetStringTrimmedValue("name", true);
        Description = json.TryGetStringTrimmedValue("desc");
    }

    /// <summary>
    /// Converts to JSON object.
    /// </summary>
    /// <returns>A JSON object.</returns>
    public override JsonObjectNode ToJson()
    {
        var json = base.ToJson();
        json.SetValue("subtype", SubType);
        json.SetValue("name", Name);
        json.SetValue("desc", Description);
        return json;
    }
}

/// <summary>
/// The credential key of a user account item.
/// </summary>
[JsonConverter(typeof(UserItemCredentialKeyInfoConverter))]
public class UserItemCredentialKeyInfo : UserItemRelatedInfo
{
    /// <summary>
    /// Intializes a new instance of the UserItemCredentialKeyInfo class.
    /// </summary>
    public UserItemCredentialKeyInfo()
    {
    }

    /// <summary>
    /// Intializes a new instance of the UserItemCredentialKeyInfo class.
    /// </summary>
    /// <param name="id">The entity identifer.</param>
    /// <param name="owner">The owner.</param>
    /// <param name="type">The type of credential name.</param>
    /// <param name="subType">The sub-type.</param>
    /// <param name="expiration">The expiration of the key.</param>
    /// <param name="protectedValue">The protected value.</param>
    /// <param name="algName">The algrithm name.</param>
    /// <param name="parameter">The additional parameter</param>
    /// <param name="descripiton">The description.</param>
    /// <param name="creation">The creation date time.</param>
    public UserItemCredentialKeyInfo(string id, BaseAccountEntityInfo owner, UserCredentialKeyTypes type, string subType, DateTime expiration, string protectedValue, string algName, string parameter = null, string descripiton = null, DateTime? creation = null)
        : this(id, owner, type, subType, protectedValue, algName, parameter, descripiton, creation)
    {
        Expiration = expiration;
    }

    /// <summary>
    /// Intializes a new instance of the UserItemCredentialKeyInfo class.
    /// </summary>
    /// <param name="id">The entity identifer.</param>
    /// <param name="owner">The owner.</param>
    /// <param name="type">The type of credential name.</param>
    /// <param name="subType">The sub-type.</param>
    /// <param name="protectedValue">The protected value.</param>
    /// <param name="algName">The algrithm name.</param>
    /// <param name="parameter">The additional parameter</param>
    /// <param name="descripiton">The description.</param>
    /// <param name="creation">The creation date time.</param>
    public UserItemCredentialKeyInfo(string id, BaseAccountEntityInfo owner, UserCredentialKeyTypes type, string subType, string protectedValue, string algName, string parameter = null, string descripiton = null, DateTime? creation = null)
        : base(id, owner, creation)
    {
        CredentialType = type;
        SubType = subType;
        ProtectedValue = protectedValue;
        AlgName = algName;
        Parameter = parameter;
        Description = descripiton;
    }

    /// <summary>
    /// Intializes a new instance of the UserItemCredentialKeyInfo class.
    /// </summary>
    /// <param name="id">The entity identifer.</param>
    /// <param name="owner">The owner.</param>
    /// <param name="type">The type of credential name.</param>
    /// <param name="subType">The sub-type.</param>
    /// <param name="expiration">The expiration of the key.</param>
    /// <param name="protectedValue">The protected value.</param>
    /// <param name="algName">The algrithm name.</param>
    /// <param name="parameter">The additional parameter</param>
    /// <param name="descripiton">The description.</param>
    /// <param name="creation">The creation date time.</param>
    public UserItemCredentialKeyInfo(Guid id, BaseAccountEntityInfo owner, UserCredentialKeyTypes type, string subType, DateTime expiration, string protectedValue, string algName, string parameter = null, string descripiton = null, DateTime? creation = null)
        : this(id, owner, type, subType, protectedValue, algName, parameter, descripiton, creation)
    {
        Expiration = expiration;
    }

    /// <summary>
    /// Intializes a new instance of the UserItemCredentialKeyInfo class.
    /// </summary>
    /// <param name="id">The entity identifer.</param>
    /// <param name="owner">The owner.</param>
    /// <param name="type">The type of credential name.</param>
    /// <param name="subType">The sub-type.</param>
    /// <param name="protectedValue">The protected value.</param>
    /// <param name="algName">The algrithm name.</param>
    /// <param name="parameter">The additional parameter</param>
    /// <param name="descripiton">The description.</param>
    /// <param name="creation">The creation date time.</param>
    public UserItemCredentialKeyInfo(Guid id, BaseAccountEntityInfo owner, UserCredentialKeyTypes type, string subType, string protectedValue, string algName, string parameter = null, string descripiton = null, DateTime? creation = null)
        : base(id, owner, creation)
    {
        CredentialType = type;
        SubType = subType;
        ProtectedValue = protectedValue;
        AlgName = algName;
        Parameter = parameter;
        Description = descripiton;
    }

    /// <summary>
    /// Intializes a new instance of the UserItemCredentialKeyInfo class.
    /// </summary>
    /// <param name="json">The JSON object to parse.</param>
    public UserItemCredentialKeyInfo(JsonObjectNode json)
        : base(json)
    {
    }

    /// <summary>
    /// Gets or sets the credential key type.
    /// </summary>
    public UserCredentialKeyTypes CredentialType
    {
        get => GetCurrentProperty<UserCredentialKeyTypes>();
        set => SetCurrentProperty(value);
    }

    /// <summary>
    /// Gets or sets the sub-type.
    /// </summary>
    public string SubType
    {
        get => GetCurrentProperty<string>();
        set => SetCurrentProperty(value);
    }

    /// <summary>
    /// Gets or sets the expiration of the key.
    /// </summary>
    public DateTime? Expiration
    {
        get => GetCurrentProperty<DateTime?>();
        set => SetCurrentProperty(value);
    }

    /// <summary>
    /// Gets or sets the key value hashed or encrypted.
    /// </summary>
    public string ProtectedValue
    {
        get => GetCurrentProperty<string>();
        set => SetCurrentProperty(value);
    }

    /// <summary>
    /// Gets or sets the algorithm name of hash or encryption.
    /// </summary>
    public string AlgName
    {
        get => GetCurrentProperty<string>();
        set => SetCurrentProperty(value);
    }

    /// <summary>
    /// Gets or sets the additional parameter of the key.
    /// </summary>
    public string Parameter
    {
        get => GetCurrentProperty<string>();
        set => SetCurrentProperty(value);
    }

    /// <summary>
    /// Gets or sets the description.
    /// </summary>
    public string Description
    {
        get => GetCurrentProperty<string>();
        set => SetCurrentProperty(value);
    }

    /// <inheritdoc />
    [JsonIgnore]
#if NETCOREAPP
    [NotMapped]
#endif
    protected override string Supertype => "secret";

    /// <inheritdoc />
    [JsonIgnore]
#if NETCOREAPP
    [NotMapped]
#endif
    protected override string ResourceType => CredentialType.ToString();

    /// <inheritdoc />
    protected override void Fill(JsonObjectNode json)
    {
        base.Fill(json);
        CredentialType = json.TryGetEnumValue<UserCredentialKeyTypes>("type") ?? UserCredentialKeyTypes.None;
        SubType = json.TryGetStringTrimmedValue("subtype", true);
        Expiration = json.TryGetDateTimeValue("exp");
        ProtectedValue = json.TryGetStringTrimmedValue("value", true);
        AlgName = json.TryGetStringTrimmedValue("alg", true);
        Parameter = json.TryGetStringValue("param");
        Description = json.TryGetStringTrimmedValue("desc");
    }

    /// <summary>
    /// Converts to JSON object.
    /// </summary>
    /// <returns>A JSON object.</returns>
    public override JsonObjectNode ToJson()
    {
        var json = base.ToJson();
        json.SetValue("subtype", SubType);
        json.SetValue("exp", Expiration);
        json.SetValue("value", ProtectedValue);
        json.SetValue("alg", AlgName);
        json.SetValue("param", Parameter);
        json.SetValue("desc", Description);
        return json;
    }
}

/// <summary>
/// JSON value node converter.
/// </summary>
internal sealed class UserItemCredentialNameInfoConverter : JsonObjectHostConverter<UserItemCredentialNameInfo>
{
    /// <inheritdoc />
    protected override UserItemCredentialNameInfo Create(JsonObjectNode json)
        => new(json);
}

/// <summary>
/// JSON value node converter.
/// </summary>
internal sealed class UserItemCredentialKeyInfoConverter : JsonObjectHostConverter<UserItemCredentialKeyInfo>
{
    /// <inheritdoc />
    protected override UserItemCredentialKeyInfo Create(JsonObjectNode json)
        => new(json);
}
