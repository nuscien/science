using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Trivial.Security;
using Trivial.Text;

namespace Trivial.Data;

/// <summary>
/// The base entity of property associated to a resource entity.
/// </summary>
public abstract class BasePropertyResourceEntityInfo<TOwner> : RelatedResourceEntityInfo<TOwner>
    where TOwner : BaseResourceEntityInfo
{
    /// <summary>
    /// Intializes a new instance of the BasePropertyResourceEntityInfo class.
    /// </summary>
    public BasePropertyResourceEntityInfo()
    {
    }

    /// <summary>
    /// Initializes a new instance of the BasePropertyResourceEntityInfo class.
    /// </summary>
    /// <param name="args">The initialization arguments.</param>
    public BasePropertyResourceEntityInfo(ResourceEntityArgs args)
        : base(args)
    {
        if (args is RelatedResourceEntityArgs a) OwnerId = a.OwnerId;
    }

    /// <summary>
    /// Initializes a new instance of the BasePropertyResourceEntityInfo class.
    /// </summary>
    /// <param name="args">The initialization arguments.</param>
    public BasePropertyResourceEntityInfo(RelatedResourceEntityArgs args)
        : base(args)
    {
        OwnerId = args?.OwnerId;
    }

    /// <summary>
    /// Intializes a new instance of the BasePropertyResourceEntityInfo class.
    /// </summary>
    /// <param name="id">The entity identifer.</param>
    /// <param name="owner">The owner.</param>
    /// <param name="creation">The creation date time.</param>
    public BasePropertyResourceEntityInfo(string id, TOwner owner, DateTime? creation = null)
        : base(id, owner, creation)
    {
    }

    /// <summary>
    /// Intializes a new instance of the BasePropertyResourceEntityInfo class.
    /// </summary>
    /// <param name="id">The entity identifer.</param>
    /// <param name="owner">The owner.</param>
    /// <param name="creation">The creation date time.</param>
    public BasePropertyResourceEntityInfo(Guid id, TOwner owner, DateTime? creation = null)
        : base(id, owner, creation)
    {
    }

    /// <summary>
    /// Intializes a new instance of the BasePropertyResourceEntityInfo class.
    /// </summary>
    /// <param name="id">The entity identifer.</param>
    /// <param name="owner">The owner.</param>
    /// <param name="name">The property key.</param>
    /// <param name="creation">The creation date time.</param>
    public BasePropertyResourceEntityInfo(string id, TOwner owner, string name, DateTime? creation = null)
        : base(id, owner, creation)
    {
        Name = name;
    }

    /// <summary>
    /// Intializes a new instance of the BasePropertyResourceEntityInfo class.
    /// </summary>
    /// <param name="id">The entity identifer.</param>
    /// <param name="owner">The owner.</param>
    /// <param name="name">The property key.</param>
    /// <param name="creation">The creation date time.</param>
    public BasePropertyResourceEntityInfo(Guid id, TOwner owner, string name, DateTime? creation = null)
        : base(id, owner, creation)
    {
        Name = name;
    }

    /// <summary>
    /// The property key. It should contains the namespace of business.
    /// </summary>
    [DataMember(Name = "name")]
    [JsonPropertyName("name")]
    [Description("The property key.")]
#if NETCOREAPP
    [Column("name")]
    [MaxLength(200)]
#endif
    public string Name
    {
        get => GetCurrentProperty<string>();
        set => SetCurrentProperty(value?.Trim());
    }

    /// <inheritdoc />
    protected override string ResourceType => OwnerSupertype;

    /// <summary>
    /// Gets the sub-type of the resource. It equals to the resource type of the owner.
    /// </summary>
    [DataMember(Name = "subtype")]
    [JsonPropertyName("subtype")]
    [Description("The sub-type of the resource. It equals to the resource type of the owner.")]
#if NETCOREAPP
    [Column("name")]
    [MaxLength(ResourceEntityUtils.MAX_ID_LENGTH)]
#endif
    public string Subtype => OwnerType;

    /// <inheritdoc />
    public override string DisplayName => Name;
}

/// <summary>
/// The property data entity of a resource entity.
/// </summary>
public class PropertyResourceEntityInfo<TOwner> : BasePropertyResourceEntityInfo<TOwner>
    where TOwner : BaseResourceEntityInfo
{
    /// <summary>
    /// Intializes a new instance of the PropertyResourceEntityInfo class.
    /// </summary>
    public PropertyResourceEntityInfo()
    {
    }

    /// <summary>
    /// Initializes a new instance of the PropertyResourceEntityInfo class.
    /// </summary>
    /// <param name="args">The initialization arguments.</param>
    public PropertyResourceEntityInfo(ResourceEntityArgs args)
        : base(args)
    {
        if (args is RelatedResourceEntityArgs a) OwnerId = a.OwnerId;
    }

    /// <summary>
    /// Initializes a new instance of the PropertyResourceEntityInfo class.
    /// </summary>
    /// <param name="args">The initialization arguments.</param>
    public PropertyResourceEntityInfo(RelatedResourceEntityArgs args)
        : base(args)
    {
        OwnerId = args?.OwnerId;
    }

    /// <summary>
    /// Intializes a new instance of the PropertyResourceEntityInfo class.
    /// </summary>
    /// <param name="id">The entity identifer.</param>
    /// <param name="owner">The owner.</param>
    /// <param name="creation">The creation date time.</param>
    public PropertyResourceEntityInfo(string id, TOwner owner, DateTime? creation = null)
        : base(id, owner, creation)
    {
    }

    /// <summary>
    /// Intializes a new instance of the PropertyResourceEntityInfo class.
    /// </summary>
    /// <param name="id">The entity identifer.</param>
    /// <param name="owner">The owner.</param>
    /// <param name="creation">The creation date time.</param>
    public PropertyResourceEntityInfo(Guid id, TOwner owner, DateTime? creation = null)
        : base(id, owner, creation)
    {
    }

    /// <summary>
    /// Intializes a new instance of the PropertyResourceEntityInfo class.
    /// </summary>
    /// <param name="id">The entity identifer.</param>
    /// <param name="owner">The owner.</param>
    /// <param name="name">The property key.</param>
    /// <param name="value">The value of property.</param>
    /// <param name="creation">The creation date time.</param>
    public PropertyResourceEntityInfo(string id, TOwner owner, string name, string value, DateTime? creation = null)
        : base(id, owner, name, creation)
    {
        Value = value;
    }

    /// <summary>
    /// Intializes a new instance of the PropertyResourceEntityInfo class.
    /// </summary>
    /// <param name="id">The entity identifer.</param>
    /// <param name="owner">The owner.</param>
    /// <param name="name">The property key.</param>
    /// <param name="value">The value of property.</param>
    /// <param name="creation">The creation date time.</param>
    public PropertyResourceEntityInfo(Guid id, TOwner owner, string name, string value, DateTime? creation = null)
        : base(id, owner, name, creation)
    {
        Value = value;
    }

    /// <summary>
    /// The value of the property.
    /// </summary>
    [DataMember(Name = "value")]
    [JsonPropertyName("value")]
    [Description("The value of the property.")]
#if NETCOREAPP
    [Column("name")]
    [MaxLength(255)]
#endif
    public string Value
    {
        get => GetCurrentProperty<string>();
        set => SetCurrentProperty(value);
    }

    /// <summary>
    /// The additional message note of the property.
    /// </summary>
    [DataMember(Name = "message")]
    [JsonPropertyName("message")]
    [Description("The additional message note of the property.")]
#if NETCOREAPP
    [Column("message")]
    [MaxLength(255)]
#endif
    public string Message
    {
        get => GetCurrentProperty<string>();
        set => SetCurrentProperty(value);
    }

    /// <inheritdoc />
    protected override string Supertype => "property";
}
