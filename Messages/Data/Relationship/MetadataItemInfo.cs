using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Trivial.Security;
using Trivial.Text;

namespace Trivial.Data;

/// <summary>
/// The metadata entity of a resouce entity.
/// </summary>
[Guid("3C13A17F-753F-4229-B5B4-908A1148F1E6")]
public class MetadataResourceEntityInfo<TOwner> : BasePropertyResourceEntityInfo<TOwner>
    where TOwner : BaseResourceEntityInfo
{
    /// <summary>
    /// Intializes a new instance of the MetadataResourceEntityInfo class.
    /// </summary>
    public MetadataResourceEntityInfo()
    {
    }

    /// <summary>
    /// Initializes a new instance of the MetadataResourceEntityInfo class.
    /// </summary>
    /// <param name="args">The initialization arguments.</param>
    public MetadataResourceEntityInfo(ResourceEntityArgs args)
        : base(args)
    {
        if (args is RelatedResourceEntityArgs a) OwnerId = a.OwnerId;
    }

    /// <summary>
    /// Initializes a new instance of the MetadataResourceEntityInfo class.
    /// </summary>
    /// <param name="args">The initialization arguments.</param>
    public MetadataResourceEntityInfo(RelatedResourceEntityArgs args)
        : base(args)
    {
        OwnerId = args?.OwnerId;
    }

    /// <summary>
    /// Intializes a new instance of the MetadataResourceEntityInfo class.
    /// </summary>
    /// <param name="id">The entity identifer.</param>
    /// <param name="owner">The owner.</param>
    /// <param name="creation">The creation date time.</param>
    public MetadataResourceEntityInfo(string id, TOwner owner, DateTime? creation = null)
        : base(id, owner, creation)
    {
    }

    /// <summary>
    /// Intializes a new instance of the MetadataResourceEntityInfo class.
    /// </summary>
    /// <param name="id">The entity identifer.</param>
    /// <param name="owner">The owner.</param>
    /// <param name="creation">The creation date time.</param>
    public MetadataResourceEntityInfo(Guid id, TOwner owner, DateTime? creation = null)
        : base(id, owner, creation)
    {
    }

    /// <summary>
    /// Intializes a new instance of the MetadataResourceEntityInfo class.
    /// </summary>
    /// <param name="id">The entity identifer.</param>
    /// <param name="owner">The owner.</param>
    /// <param name="name">The property key.</param>
    /// <param name="value">The value of property.</param>
    /// <param name="creation">The creation date time.</param>
    public MetadataResourceEntityInfo(string id, TOwner owner, string name, JsonObjectNode value, DateTime? creation = null)
        : base(id, owner, name, creation)
    {
        ConfigInfo = value;
    }

    /// <summary>
    /// Intializes a new instance of the MetadataResourceEntityInfo class.
    /// </summary>
    /// <param name="id">The entity identifer.</param>
    /// <param name="name">The property key.</param>
    /// <param name="value">The value of property.</param>
    /// <param name="owner">The owner.</param>
    /// <param name="creation">The creation date time.</param>
    public MetadataResourceEntityInfo(Guid id, TOwner owner, string name, JsonObjectNode value, DateTime? creation = null)
        : base(id, owner, name, creation)
    {
        ConfigInfo = value;
    }

    /// <inheritdoc />
    protected override string Supertype => "metadata";

    /// <summary>
    /// The metadata value.
    /// </summary>
    [Description("The metadata value.")]
#if NETCOREAPP
    [Column("value")]
#endif
    public new JsonObjectNode ConfigInfo
    {
        get => base.ConfigInfo;
        set => base.ConfigInfo = value;
    }
}
