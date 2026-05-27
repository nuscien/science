using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Claims;
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
/// The account of companies, schools, governments, institutions or other kind of organizations.
/// It may also be a department of a large organization.
/// It can be controled by its administrators configured.
/// </summary>
[Guid("3E5511FC-4206-4691-9C3D-F24F31343A83")]
public class OrgAccountItemInfo : BaseUserItemInfo, IBasicPublisherInfo
{
    /// <summary>
    /// Initializes a new instance of the OrgAccountItemInfo class.
    /// </summary>
    public OrgAccountItemInfo()
        : base(AccountEntityTypes.Organization)
    {
    }

    /// <summary>
    /// Initializes a new instance of the OrgAccountItemInfo class.
    /// </summary>
    /// <param name="args">The initialization arguments.</param>
    /// <param name="nickname">The nickname or display name.</param>
    /// <param name="avatar">The avatar URI.</param>
    public OrgAccountItemInfo(ResourceEntityArgs args, string nickname = null, Uri avatar = null)
        : base(AccountEntityTypes.Organization, args, nickname, Genders.Asexual, avatar)
    {
    }

    /// <summary>
    /// Initializes a new instance of the OrgAccountItemInfo class.
    /// </summary>
    /// <param name="args">The initialization arguments.</param>
    public OrgAccountItemInfo(AccountEntityArgs args)
        : base(AccountEntityTypes.Organization, args, Genders.Asexual)
    {
    }

    /// <summary>
    /// Initializes a new instance of the OrgAccountItemInfo class.
    /// </summary>
    /// <param name="id">The resource identifier.</param>
    /// <param name="nickname">The nickname or display name.</param>
    /// <param name="avatar">The avatar URI.</param>
    /// <param name="creation">The creation date time.</param>
    public OrgAccountItemInfo(string id, string nickname, Uri avatar = null, DateTime? creation = null)
        : base(AccountEntityTypes.Organization, id, nickname, Genders.Asexual, avatar, creation)
    {
    }

    /// <summary>
    /// Gets the publisher identifier.
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    [JsonIgnore]
#if NETCOREAPP
    [NotMapped]
#endif
    string IBasicPublisherInfo.Id => Id;

    /// <summary>
    /// Gets the publisher display name.
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    [JsonIgnore]
#if NETCOREAPP
    [NotMapped]
#endif
    string IBasicPublisherInfo.DisplayName => Nickname;

    /// <summary>
    /// Gets or sets the official website URI.
    /// </summary>
    [JsonPropertyName("url")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [Description("The official website of the organization.")]
#if NETCOREAPP
    [Column("url")]
#endif
    public Uri Website
    {
        get => GetCurrentProperty<Uri>();
        set => SetCurrentProperty(value);
    }

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

        if (Website != null) yield return ResourceEntityUtils.ToClaim(ClaimTypes.Webpage, Website.OriginalString);
    }
}
