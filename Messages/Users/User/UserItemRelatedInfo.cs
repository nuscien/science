using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
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
/// The base related entity of assets or data associated to an account.
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
        AccountType = owner?.AccountEntityType ?? AccountEntityTypes.Unknown;
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
        AccountType = owner?.AccountEntityType ?? AccountEntityTypes.Unknown;
    }

    /// <inheritdoc />
    protected override string Supertype => "own";

    /// <summary>
    /// Gets or sets the account entity type of or owner.
    /// </summary>
    [DataMember(Name = "ownertype")]
    [JsonPropertyName("ownertype")]
    [Description("The account entity type of owner.")]
#if NETCOREAPP
    [Column("ownertype")]
#endif
    public AccountEntityTypes AccountType // ToDo: Update logic
    {
        get => GetCurrentProperty<AccountEntityTypes>();
        protected set => SetCurrentProperty(value);
    }

    internal override void OnOwnerIdChanged(string id)
    {
        base.OnOwnerIdChanged(id);
        SetProperty(nameof(AccountType), Owner?.AccountEntityType ?? AccountEntityTypes.Unknown);
    }
}

/// <summary>
/// The base related entity of assets or data associated to an account.
/// </summary>
public abstract class UserItemRelatedInfo<T> : RelatedResourceEntityInfo<T>
    where T : BaseAccountEntityInfo
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
    protected UserItemRelatedInfo(string id, T owner, DateTime? creation = null)
        : base(id, owner, creation)
    {
    }

    /// <summary>
    /// Intializes a new instance of the UserItemRelatedInfo class.
    /// </summary>
    /// <param name="id">The entity identifer.</param>
    /// <param name="owner">The owner.</param>
    /// <param name="creation">The creation date time.</param>
    protected UserItemRelatedInfo(Guid id, T owner, DateTime? creation = null)
        : base(id, owner, creation)
    {
    }

    /// <inheritdoc />
    protected override string Supertype => "own";

    /// <summary>
    /// Gets or sets the account entity type of or owner.
    /// </summary>
    [JsonIgnore]
#if NETCOREAPP
    [NotMapped]
#endif
    public AccountEntityTypes AccountType
        => Owner?.AccountEntityType ?? AccountEntityTypes.Unknown;
}
