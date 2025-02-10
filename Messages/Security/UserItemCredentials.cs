using System;
using System.Collections.Generic;
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
public class UserItemCredentialNameInfo : BaseResourceEntityInfo
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
    public UserItemCredentialNameInfo(string id, BasePrincipalEntityInfo owner, UserCredentialNameTypes type, string subType, string name, string descripiton = null)
        : base(id)
    {
        OwnerId = owner?.Id;
        OwnerType = owner?.PrincipalEntityType ?? PrincipalEntityTypes.Unknown;
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
    public UserItemCredentialNameInfo(Guid id, BasePrincipalEntityInfo owner, UserCredentialNameTypes type, string subType, string name, string descripiton = null)
        : base(id)
    {
        OwnerId = owner?.Id;
        OwnerType = owner?.PrincipalEntityType ?? PrincipalEntityTypes.Unknown;
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
        : base()
    {
        Id = json.TryGetStringTrimmedValue("id", true) ?? json.Id;
        OwnerId = json.TryGetStringTrimmedValue("target", true);
        OwnerType = json.TryGetEnumValue<PrincipalEntityTypes>("target") ?? PrincipalEntityTypes.Unknown;
        CredentialType = json.TryGetEnumValue<UserCredentialNameTypes>("kind") ?? UserCredentialNameTypes.Logname;
        SubType = json.TryGetStringTrimmedValue("subkind", true);
        Name = json.TryGetStringTrimmedValue("name", true);
        Description = json.TryGetStringTrimmedValue("desc");
    }

    /// <summary>
    /// Gets or sets the principal entity type of target or owner.
    /// </summary>
    public PrincipalEntityTypes OwnerType
    {
        get => GetCurrentProperty<PrincipalEntityTypes>();
        set => SetCurrentProperty(value);
    }

    /// <summary>
    /// Gets or sets the identifier of target or owner.
    /// </summary>
    public string OwnerId
    {
        get => GetCurrentProperty<string>();
        set => SetCurrentProperty(value);
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

    /// <summary>
    /// Converts to JSON object.
    /// </summary>
    /// <returns>A JSON object.</returns>
    public override JsonObjectNode ToJson()
    {
        var json = base.ToJson();
        json.SetValue("type", "logname");
        json.SetValue("owner", OwnerId);
        json.SetValue("target", OwnerType.ToString());
        json.SetValue("kind", CredentialType.ToString());
        json.SetValue("subkind", SubType);
        json.SetValue("name", Name);
        json.SetValue("desc", Description);
        return json;
    }
}

/// <summary>
/// The credential key of a user account item.
/// </summary>
[JsonConverter(typeof(UserItemCredentialKeyInfoConverter))]
public class UserItemCredentialKeyInfo : BaseResourceEntityInfo
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
    public UserItemCredentialKeyInfo(string id, BasePrincipalEntityInfo owner, UserCredentialKeyTypes type, string subType, DateTime expiration, string protectedValue, string algName, string parameter = null, string descripiton = null)
        : this(id, owner, type, subType, protectedValue, algName, parameter, descripiton)
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
    public UserItemCredentialKeyInfo(string id, BasePrincipalEntityInfo owner, UserCredentialKeyTypes type, string subType, string protectedValue, string algName, string parameter = null, string descripiton = null)
        : base(id)
    {
        OwnerId = owner?.Id;
        OwnerType = owner?.PrincipalEntityType ?? PrincipalEntityTypes.Unknown;
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
    public UserItemCredentialKeyInfo(Guid id, BasePrincipalEntityInfo owner, UserCredentialKeyTypes type, string subType, DateTime expiration, string protectedValue, string algName, string parameter = null, string descripiton = null)
        : this(id, owner, type, subType, protectedValue, algName, parameter, descripiton)
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
    public UserItemCredentialKeyInfo(Guid id, BasePrincipalEntityInfo owner, UserCredentialKeyTypes type, string subType, string protectedValue, string algName, string parameter = null, string descripiton = null)
        : base(id)
    {
        OwnerId = owner?.Id;
        OwnerType = owner?.PrincipalEntityType ?? PrincipalEntityTypes.Unknown;
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
        : base()
    {
        Id = json.TryGetStringTrimmedValue("id", true) ?? json.Id;
        OwnerId = json.TryGetStringTrimmedValue("owner", true);
        OwnerType = json.TryGetEnumValue<PrincipalEntityTypes>("target") ?? PrincipalEntityTypes.Unknown;
        CredentialType = json.TryGetEnumValue<UserCredentialKeyTypes>("kind") ?? UserCredentialKeyTypes.None;
        SubType = json.TryGetStringTrimmedValue("subkind", true);
        Expiration = json.TryGetDateTimeValue("exp");
        ProtectedValue = json.TryGetStringTrimmedValue("value", true);
        AlgName = json.TryGetStringTrimmedValue("alg", true);
        Parameter = json.TryGetStringValue("param");
        Description = json.TryGetStringTrimmedValue("desc");
    }

    /// <summary>
    /// Gets or sets the principal entity type of target or owner.
    /// </summary>
    public PrincipalEntityTypes OwnerType
    {
        get => GetCurrentProperty<PrincipalEntityTypes>();
        set => SetCurrentProperty(value);
    }

    /// <summary>
    /// Gets or sets the identifier of target or owner.
    /// </summary>
    public string OwnerId
    {
        get => GetCurrentProperty<string>();
        set => SetCurrentProperty(value);
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

    /// <summary>
    /// Converts to JSON object.
    /// </summary>
    /// <returns>A JSON object.</returns>
    public override JsonObjectNode ToJson()
    {
        var json = base.ToJson();
        json.SetValue("type", "secret");
        json.SetValue("owner", OwnerId);
        json.SetValue("target", OwnerType.ToString());
        json.SetValue("kind", CredentialType.ToString());
        json.SetValue("subkind", SubType);
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
