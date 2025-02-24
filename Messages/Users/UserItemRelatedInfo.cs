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
public abstract class UserItemRelatedInfo : RelatedResourceEntityInfo<BaseAccountEntityInfo>
{
    /// <summary>
    /// Intializes a new instance of the UserItemRelatedInfo class.
    /// </summary>
    protected UserItemRelatedInfo()
    {
    }

    /// <summary>
    /// Intializes a new instance of the UserItemRelatedInfo class.
    /// </summary>
    /// <param name="id">The entity identifer.</param>
    /// <param name="owner">The owner.</param>
    /// <param name="creation">The creation date time.</param>
    protected UserItemRelatedInfo(string id, BaseAccountEntityInfo owner, DateTime? creation = null)
        : base(id, owner, creation)
    {
        OwnerType = owner?.AccountEntityType ?? AccountEntityTypes.Unknown;
    }

    /// <summary>
    /// Intializes a new instance of the UserItemRelatedInfo class.
    /// </summary>
    /// <param name="id">The entity identifer.</param>
    /// <param name="owner">The owner.</param>
    /// <param name="creation">The creation date time.</param>
    protected UserItemRelatedInfo(Guid id, BaseAccountEntityInfo owner, DateTime? creation = null)
        : base(id, owner, creation)
    {
        OwnerType = owner?.AccountEntityType ?? AccountEntityTypes.Unknown;
    }

    /// <summary>
    /// Intializes a new instance of the UserItemRelatedInfo class.
    /// </summary>
    /// <param name="json">The JSON object to parse.</param>
    protected UserItemRelatedInfo(JsonObjectNode json)
        : base(json)
    {
    }

    /// <summary>
    /// Gets or sets the account entity type of or owner.
    /// </summary>
    [DataMember(Name = "ownertype")]
    [JsonPropertyName("ownertype")]
    [Description("The account entity type of owner.")]
    public AccountEntityTypes OwnerType
    {
        get => GetCurrentProperty<AccountEntityTypes>();
        protected set => SetCurrentProperty(value);
    }

    /// <inheritdoc />
    protected override void Fill(JsonObjectNode json)
    {
        base.Fill(json);
        OwnerType = json.TryGetEnumValue<AccountEntityTypes>("ownertype") ?? AccountEntityTypes.Unknown;
    }

    /// <summary>
    /// Converts to JSON object.
    /// </summary>
    /// <returns>A JSON object.</returns>
    public override JsonObjectNode ToJson()
    {
        var json = base.ToJson();
        json.SetValue("ownertype", OwnerType.ToString());
        return json;
    }
}
