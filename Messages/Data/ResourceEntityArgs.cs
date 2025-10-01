using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Trivial.Text;

namespace Trivial.Data;

/// <summary>
/// The arguments to initializes a resource entity.
/// </summary>
[Guid("088063F3-3225-45D2-A16F-D26265183B8E")]
public class ResourceEntityArgs
{
    /// <summary>
    /// Initializes a new instance of the ResourceEntityArgs class.
    /// </summary>
    /// <param name="keepNullId">true if keep the identifier as null; otherwise, false.</param>
    public ResourceEntityArgs(bool keepNullId)
    {
        var now = DateTime.Now;
        if (!keepNullId) Id = Guid.NewGuid().ToString("N");
        State = ResourceEntityStates.Normal;
        CreationTime = LastModificationTime = now;
    }

    /// <summary>
    /// Initializes a new instance of the ResourceEntityArgs class.
    /// </summary>
    public ResourceEntityArgs()
        : this(false)
    {
    }

    /// <summary>
    /// Initializes a new instance of the ResourceEntityArgs class.
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <param name="creation">The creation date time.</param>
    public ResourceEntityArgs(Guid id, DateTime? creation = null)
        : this(id.ToString("N"), creation)
    {
    }

    /// <summary>
    /// Initializes a new instance of the ResourceEntityArgs class.
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <param name="creation">The creation date time.</param>
    /// <param name="modification">The last modification date time.</param>
    public ResourceEntityArgs(Guid id, DateTime creation, DateTime modification)
        : this(id.ToString("N"), creation, modification)
    {
    }

    /// <summary>
    /// Initializes a new instance of the ResourceEntityArgs class.
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <param name="creation">The creation date time.</param>
    public ResourceEntityArgs(string id, DateTime? creation = null)
    {
        CreationTime = creation ?? DateTime.Now;
        Id = id?.Trim();
        State = ResourceEntityStates.Normal;
        LastModificationTime = CreationTime;
    }

    /// <summary>
    /// Initializes a new instance of the ResourceEntityArgs class.
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <param name="creation">The creation date time.</param>
    /// <param name="modification">The last modification date time.</param>
    public ResourceEntityArgs(string id, DateTime creation, DateTime modification)
    {
        Id = id?.Trim();
        State = ResourceEntityStates.Normal;
        CreationTime = creation;
        LastModificationTime = modification;
    }

    /// <summary>
    /// Gets or sets the identifier.
    /// </summary>
    [DataMember(Name = "id")]
    [JsonPropertyName("$id")]
    [Description("The unique identifier of the entity.")]
    public string Id { get; set; }

    /// <summary>
    /// Gets or sets the resource name, if available, including logname, config key, package name, or other kind of name or key of this entity.
    /// </summary>
    [DataMember(Name = "name")]
    [JsonPropertyName("name")]
    [Description("The resource name, if available, including logname, config key, package name, or other kind of name or key of this entity.")]
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the state.
    /// </summary>
    [DataMember(Name = "state")]
    [JsonPropertyName("state")]
    [JsonConverter(typeof(JsonIntegerEnumCompatibleConverter<ResourceEntityStates>))]
    public ResourceEntityStates State { get; set; }

    /// <summary>
    /// Gets or sets the date time when the entity creates.
    /// </summary>
    [DataMember(Name = "created")]
    [JsonPropertyName("created")]
    [Description("The date time when the entity creates.")]
    public DateTime CreationTime { get; set; }

    /// <summary>
    /// Gets or sets the date time when the entity updates recently.
    /// </summary>
    [DataMember(Name = "updated")]
    [JsonPropertyName("updated")]
    [Description("The date time when the entity updates recently.")]
    public DateTime LastModificationTime { get; set; }

    /// <summary>
    /// Gets the latest saving status of this entity.
    /// </summary>
    [JsonIgnore]
    public ResourceEntitySavingStatus LastSavingStatus { get; set; }

    /// <summary>
    /// Gets or sets the identifier.
    /// </summary>
    [DataMember(Name = "rev")]
    [JsonPropertyName("rev")]
    [Description("The hash value of the entity revision.")]
    public string RevisionId { get; set; }
}

/// <summary>
/// The arguments to initializes a resource entity.
/// </summary>
[Guid("93D138E9-913E-47DA-ACF7-2451FC6B3582")]
public class RelatedResourceEntityArgs : ResourceEntityArgs
{
    /// <summary>
    /// Initializes a new instance of the RelatedResourceEntityArgs class.
    /// </summary>
    /// <param name="keepNullId">true if keep the identifier as null; otherwise, false.</param>
    /// <param name="ownerId">The identifier of the owner resource.</param>
    public RelatedResourceEntityArgs(bool keepNullId, string ownerId)
        : base(keepNullId)
    {
        OwnerId = ownerId;
    }
    /// <summary>
    /// Initializes a new instance of the RelatedResourceEntityArgs class.
    /// </summary>
    /// <param name="keepNullId">true if keep the identifier as null; otherwise, false.</param>
    /// <param name="owner">The owner resource.</param>
    public RelatedResourceEntityArgs(bool keepNullId, BaseResourceEntityInfo owner)
        : base(keepNullId)
    {
        OwnerId = owner?.Id;
    }

    /// <summary>
    /// Initializes a new instance of the RelatedResourceEntityArgs class.
    /// </summary>
    public RelatedResourceEntityArgs()
        : base(false)
    {
    }

    /// <summary>
    /// Initializes a new instance of the RelatedResourceEntityArgs class.
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <param name="owner">The owner resource.</param>
    /// <param name="creation">The creation date time.</param>
    public RelatedResourceEntityArgs(Guid id, BaseResourceEntityInfo owner, DateTime? creation = null)
        : base(id.ToString("N"), creation)
    {
        OwnerId = owner?.Id;
    }

    /// <summary>
    /// Initializes a new instance of the RelatedResourceEntityArgs class.
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <param name="owner">The owner resource.</param>
    /// <param name="creation">The creation date time.</param>
    /// <param name="modification">The last modification date time.</param>
    public RelatedResourceEntityArgs(Guid id, BaseResourceEntityInfo owner, DateTime creation, DateTime modification)
        : base(id.ToString("N"), creation, modification)
    {
        OwnerId = owner?.Id;
    }

    /// <summary>
    /// Initializes a new instance of the RelatedResourceEntityArgs class.
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <param name="ownerId">The identifier of the owner resource.</param>
    /// <param name="creation">The creation date time.</param>
    public RelatedResourceEntityArgs(Guid id, string ownerId, DateTime? creation = null)
        : base(id.ToString("N"), creation)
    {
        OwnerId = ownerId;
    }

    /// <summary>
    /// Initializes a new instance of the RelatedResourceEntityArgs class.
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <param name="ownerId">The identifier of the owner resource.</param>
    /// <param name="creation">The creation date time.</param>
    /// <param name="modification">The last modification date time.</param>
    public RelatedResourceEntityArgs(Guid id, string ownerId, DateTime creation, DateTime modification)
        : base(id.ToString("N"), creation, modification)
    {
        OwnerId = ownerId;
    }

    /// <summary>
    /// Initializes a new instance of the RelatedResourceEntityArgs class.
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <param name="owner">The owner resource.</param>
    /// <param name="creation">The creation date time.</param>
    public RelatedResourceEntityArgs(string id, BaseResourceEntityInfo owner, DateTime? creation = null)
        : base(id, creation)
    {
        OwnerId = owner?.Id;
    }

    /// <summary>
    /// Initializes a new instance of the RelatedResourceEntityArgs class.
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <param name="owner">The owner resource.</param>
    /// <param name="creation">The creation date time.</param>
    /// <param name="modification">The last modification date time.</param>
    public RelatedResourceEntityArgs(string id, BaseResourceEntityInfo owner, DateTime creation, DateTime modification)
        : base(id, creation, modification)
    {
        OwnerId = owner?.Id;
    }

    /// <summary>
    /// Initializes a new instance of the RelatedResourceEntityArgs class.
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <param name="ownerId">The identifier of the owner resource.</param>
    /// <param name="creation">The creation date time.</param>
    public RelatedResourceEntityArgs(string id, string ownerId, DateTime? creation = null)
        : base(id, creation)
    {
        OwnerId = ownerId;
    }

    /// <summary>
    /// Initializes a new instance of the RelatedResourceEntityArgs class.
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <param name="ownerId">The identifier of the owner resource.</param>
    /// <param name="creation">The creation date time.</param>
    /// <param name="modification">The last modification date time.</param>
    public RelatedResourceEntityArgs(string id, string ownerId, DateTime creation, DateTime modification)
        : base(id, creation, modification)
    {
        OwnerId = ownerId;
    }

    /// <summary>
    /// Gets or sets the identifier of owner.
    /// </summary>
    [DataMember(Name = "owner")]
    [JsonPropertyName("owner")]
    [Description("The identifier of the owner.")]
    public string OwnerId { get; set; }
}
