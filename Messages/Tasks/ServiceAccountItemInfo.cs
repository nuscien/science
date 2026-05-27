using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Trivial.Data;
using Trivial.Reflection;
using Trivial.Text;
using Trivial.Users;
using Trivial.Web;

namespace Trivial.Tasks;

/// <summary>
/// The service account used by a back-end service, a local app or an automation,
/// which requires to run as an identity and related roles to access resources under permission scope.
/// </summary>
[Guid("63AAF308-C3E7-4A5C-B390-A8B05E78B703")]
public class ServiceAccountItemInfo : BaseUserItemInfo
{
    /// <summary>
    /// Initializes a new instance of the ServiceAccountItemInfo class.
    /// </summary>
    public ServiceAccountItemInfo()
        : base(AccountEntityTypes.Service)
    {
    }

    /// <summary>
    /// Initializes a new instance of the ServiceAccountItemInfo class.
    /// </summary>
    /// <param name="id">The resource identifier.</param>
    /// <param name="nickname">The nickname or display name.</param>
    /// <param name="avatar">The avatar URI.</param>
    /// <param name="publisher">The publisher info.</param>
    /// <param name="creation">The creation date time.</param>
    public ServiceAccountItemInfo(string id, string nickname, Uri avatar = null, IBasicPublisherInfo publisher = null, DateTime? creation = null)
        : base(AccountEntityTypes.Service, id, nickname, Genders.Asexual, avatar, creation)
    {
        Publisher = publisher;
    }

    /// <summary>
    /// Gets or sets the basic information of publisher or developer.
    /// </summary>
    [DataMember(Name = "publisher")]
    [JsonPropertyName("publisher")]
    [Description("The basic information of publisher or developer.")]
    [JsonConverter(typeof(GenericPublisherInfoConverter))]
#if NETCOREAPP
    [NotMapped]
#endif
    public IBasicPublisherInfo Publisher // ToDo: DB column
    {
        get => GetCurrentProperty<IBasicPublisherInfo>();
        set => SetCurrentProperty(value);
    }

    /// <summary>
    /// Gets or sets the service name for application mapping.
    /// </summary>
    [DataMember(Name = "name")]
    [JsonPropertyName("name")]
    [JsonConverter(typeof(GenericPublisherInfoConverter))]
    [Description("The service name for application mapping.")]
#if NETCOREAPP
    [NotMapped]
#endif
    public string ServiceName
    {
        get => GetCurrentProperty<string>();
        set => SetCurrentProperty(value);
    }

    /// <inheritdoc />
    protected override void ToString(StringBuilder sb)
    {
        var publisher = Publisher.DisplayName;
        if (string.IsNullOrWhiteSpace(publisher)) return;
        sb.AppendLine();
        sb.Append("Publisher = ");
        sb.Append(publisher);
        var pid = Publisher?.Id?.Trim();
        if (string.IsNullOrEmpty(pid)) return;
        sb.Append(" (");
        sb.Append(pid);
        sb.Append(')');
    }
}
