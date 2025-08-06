using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Trivial.Data;
using Trivial.Security;
using Trivial.Text;

namespace Trivial.Users;

/// <summary>
/// The user item related info.
/// </summary>
public abstract class RelatedResourceEntityInfo : BaseResourceEntityInfo
{
    /// <summary>
    /// Intializes a new instance of the RelatedResourceEntityInfo class.
    /// </summary>
    protected RelatedResourceEntityInfo()
    {
    }

    /// <summary>
    /// Intializes a new instance of the RelatedResourceEntityInfo class.
    /// </summary>
    /// <param name="id">The entity identifer.</param>
    /// <param name="creation">The creation date time.</param>
    internal RelatedResourceEntityInfo(string id, DateTime? creation)
        : base(id, creation)
    {
    }

    /// <summary>
    /// Intializes a new instance of the RelatedResourceEntityInfo class.
    /// </summary>
    /// <param name="id">The entity identifer.</param>
    /// <param name="creation">The creation date time.</param>
    internal RelatedResourceEntityInfo(Guid id, DateTime? creation)
        : base(id, creation)
    {
    }

    /// <summary>
    /// Intializes a new instance of the RelatedResourceEntityInfo class.
    /// </summary>
    /// <param name="id">The entity identifer.</param>
    /// <param name="ownerId">The owner identifier.</param>
    /// <param name="creation">The creation date time.</param>
    protected RelatedResourceEntityInfo(string id, string ownerId, DateTime? creation = null)
        : base(id, creation)
    {
        OwnerId = ownerId;
    }

    /// <summary>
    /// Intializes a new instance of the RelatedResourceEntityInfo class.
    /// </summary>
    /// <param name="id">The entity identifer.</param>
    /// <param name="ownerId">The owner identifier.</param>
    /// <param name="creation">The creation date time.</param>
    protected RelatedResourceEntityInfo(Guid id, string ownerId, DateTime? creation = null)
        : base(id, creation)
    {
        OwnerId = ownerId;
    }

    /// <summary>
    /// Intializes a new instance of the RelatedResourceEntityInfo class.
    /// </summary>
    /// <param name="id">The entity identifer.</param>
    /// <param name="ownerId">The owner identifier.</param>
    /// <param name="creation">The creation date time.</param>
    protected RelatedResourceEntityInfo(string id, BaseResourceEntityInfo ownerId, DateTime? creation = null)
        : this(id, ownerId?.Id, creation)
    {
    }

    /// <summary>
    /// Intializes a new instance of the RelatedResourceEntityInfo class.
    /// </summary>
    /// <param name="id">The entity identifer.</param>
    /// <param name="ownerId">The owner identifier.</param>
    /// <param name="creation">The creation date time.</param>
    protected RelatedResourceEntityInfo(Guid id, BaseResourceEntityInfo ownerId, DateTime? creation = null)
        : this(id, ownerId?.Id, creation)
    {
    }

    /// <summary>
    /// Intializes a new instance of the RelatedResourceEntityInfo class.
    /// </summary>
    /// <param name="json">The JSON object to parse.</param>
    protected RelatedResourceEntityInfo(JsonObjectNode json)
        : base(json)
    {
    }

    /// <summary>
    /// Gets or sets the identifier of owner.
    /// </summary>
    /// <exception cref="InvalidOperationException">The new owner identifier is not supported to set.</exception>
    [DataMember(Name = "owner")]
    [JsonPropertyName("owner")]
    [Description("The identifier of the owner.")]
    public string OwnerId
    {
        get
        {
            return GetProperty<string>(nameof(OwnerId));
        }

        protected set
        {
            var id = value?.Trim();
            if (string.IsNullOrEmpty(id)) id = null;
            var old = GetProperty<string>(nameof(OwnerId));
            if (old != id)
            {
                if (CanOwnerIdChange(new(GetProperty<string>(nameof(OwnerId)), id)))
                    SetProperty(nameof(OwnerId), id);
                else
                    throw new InvalidOperationException("Unable to set the owner identifier.");
            }

            OnOwnerIdChanged(id);
        }
    }

    /// <summary>
    /// Tests if the owner identifier is not null or empty.
    /// </summary>
    /// <returns>true if has an owner; otherwise, false.</returns>
    /// <remarks>This is only test if the owner identifier has value but no resource validation.</remarks>
    public bool HasOwner() => !string.IsNullOrWhiteSpace(OwnerId);

    /// <inheritdoc />
    protected override void Fill(JsonObjectNode json)
    {
        base.Fill(json);
        OwnerId = json.TryGetStringTrimmedValue("owner", true);
    }

    /// <summary>
    /// Converts to JSON object.
    /// </summary>
    /// <returns>A JSON object.</returns>
    public override JsonObjectNode ToJson()
    {
        var json = base.ToJson();
        json.SetValue("owner", OwnerId);
        return json;
    }

    /// <summary>
    /// Tests if the owner identifier can be changed to the new one.
    /// </summary>
    /// <param name="args">The change event arguments.</param>
    protected virtual bool CanOwnerIdChange(ChangeEventArgs<string> args)
        => true;

    internal virtual void OnOwnerIdChanged(string id)
    {
    }
}

/// <summary>
/// The user item related info.
/// </summary>
public abstract class RelatedResourceEntityInfo<TOwner> : RelatedResourceEntityInfo
    where TOwner : BaseResourceEntityInfo
{
    /// <summary>
    /// Intializes a new instance of the RelatedResourceEntityInfo class.
    /// </summary>
    protected RelatedResourceEntityInfo()
    {
    }

    /// <summary>
    /// Intializes a new instance of the RelatedResourceEntityInfo class.
    /// </summary>
    /// <param name="id">The entity identifer.</param>
    /// <param name="owner">The owner.</param>
    /// <param name="creation">The creation date time.</param>
    protected RelatedResourceEntityInfo(string id, TOwner owner, DateTime? creation = null)
        : base(id, creation)
    {
        Owner = owner;
    }

    /// <summary>
    /// Intializes a new instance of the RelatedResourceEntityInfo class.
    /// </summary>
    /// <param name="id">The entity identifer.</param>
    /// <param name="owner">The owner.</param>
    /// <param name="creation">The creation date time.</param>
    protected RelatedResourceEntityInfo(Guid id, TOwner owner, DateTime? creation = null)
        : base(id, creation)
    {
        Owner = owner;
    }

    /// <summary>
    /// Intializes a new instance of the RelatedResourceEntityInfo class.
    /// </summary>
    /// <param name="json">The JSON object to parse.</param>
    protected RelatedResourceEntityInfo(JsonObjectNode json)
        : base(json)
    {
    }

    /// <summary>
    /// Gets the owner resource.
    /// </summary>
    [JsonIgnore]
    public TOwner Owner
    {
        get
        {
            return GetProperty<TOwner>(nameof(Owner));
        }

        protected internal set
        {
            SetProperty(nameof(Owner), value);
            SetProperty(nameof(OwnerId), value?.Id);
        }
    }

    /// <summary>
    /// Tests if the owner is not null.
    /// </summary>
    /// <returns>true if has an owner cache; otherwise, false.</returns>
    public bool HasOwnerCache() => Owner?.Id != null && Owner.Id == OwnerId;

    /// <summary>
    /// Clears the cache of the owner resource entity.
    /// </summary>
    public void ClearOwnerCache()
        => RemoveProperty(nameof(Owner));

    internal override void OnOwnerIdChanged(string id)
    {
        if (id == null)
        {
            SetProperty(nameof(Owner), null);
            return;
        }

        var owner = GetProperty<TOwner>(nameof(Owner));
        if (owner != null && owner.Id != id) SetProperty(nameof(Owner), null);
    }
}

/// <summary>
/// The user item related info.
/// </summary>
public abstract class RelatedResourceEntityInfo<TOwner, TTarget> : RelatedResourceEntityInfo<TOwner>
    where TOwner : BaseResourceEntityInfo
    where TTarget : BaseResourceEntityInfo
{
    /// <summary>
    /// Intializes a new instance of the RelatedResourceEntityInfo class.
    /// </summary>
    protected RelatedResourceEntityInfo()
    {
    }

    /// <summary>
    /// Intializes a new instance of the RelatedResourceEntityInfo class.
    /// </summary>
    /// <param name="id">The entity identifer.</param>
    /// <param name="owner">The owner.</param>
    /// <param name="target">The target.</param>
    /// <param name="creation">The creation date time.</param>
    protected RelatedResourceEntityInfo(string id, TOwner owner, TTarget target, DateTime? creation = null)
        : base(id, owner, creation)
    {
        Target = target;
    }

    /// <summary>
    /// Intializes a new instance of the RelatedResourceEntityInfo class.
    /// </summary>
    /// <param name="id">The entity identifer.</param>
    /// <param name="owner">The owner.</param>
    /// <param name="target">The target.</param>
    /// <param name="creation">The creation date time.</param>
    protected RelatedResourceEntityInfo(Guid id, TOwner owner, TTarget target, DateTime? creation = null)
        : base(id, owner, creation)
    {
        Target = target;
    }

    /// <summary>
    /// Intializes a new instance of the RelatedResourceEntityInfo class.
    /// </summary>
    /// <param name="json">The JSON object to parse.</param>
    protected RelatedResourceEntityInfo(JsonObjectNode json)
        : base(json)
    {
    }

    /// <summary>
    /// Gets or sets the identifier of target.
    /// </summary>
    [DataMember(Name = "target")]
    [JsonPropertyName("target")]
    [Description("The identifier of target.")]
    public string TargetId
    {
        get
        {
            return GetProperty<string>(nameof(TargetId));
        }

        protected set
        {
            var id = value?.Trim();
            if (string.IsNullOrEmpty(id)) id = null;
            SetProperty(nameof(TargetId), id);
            if (id == null)
            {
                SetProperty(nameof(Target), null);
                return;
            }

            var target = GetProperty<TTarget>(nameof(Target));
            if (target != null && target.Id != id) SetProperty(nameof(Target), null);
        }
    }

    /// <summary>
    /// Gets the target resource.
    /// </summary>
    [JsonIgnore]
    public TTarget Target
    {
        get
        {
            return GetProperty<TTarget>(nameof(Target));
        }

        protected internal set
        {
            SetProperty(nameof(Target), value);
            SetProperty(nameof(TargetId), value?.Id);
        }
    }

    /// <summary>
    /// Tests if the target identifier is not null or empty.
    /// </summary>
    /// <returns>true if has an target; otherwise, false.</returns>
    /// <remarks>This is only test if the target identifier has value but no resource validation.</remarks>
    public bool HasTarget() => !string.IsNullOrWhiteSpace(TargetId);

    /// <summary>
    /// Tests if the target is not null.
    /// </summary>
    /// <returns>true if has an target cache; otherwise, false.</returns>
    public bool HasTargetCache() => Target?.Id != null && Target.Id == TargetId;

    /// <summary>
    /// Clears the cache of the target resource entity.
    /// </summary>
    public void ClearTargetCache()
        => RemoveProperty(nameof(Target));

    /// <inheritdoc />
    protected override void Fill(JsonObjectNode json)
    {
        base.Fill(json);
        TargetId = json.TryGetStringTrimmedValue("target", true);
    }

    /// <summary>
    /// Converts to JSON object.
    /// </summary>
    /// <returns>A JSON object.</returns>
    public override JsonObjectNode ToJson()
    {
        var json = base.ToJson();
        json.SetValue("target", TargetId);
        return json;
    }
}
