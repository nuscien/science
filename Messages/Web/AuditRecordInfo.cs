using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Trivial.Reflection;

namespace Trivial.Web;

/// <summary>
/// The audit record information.
/// </summary>
public class AuditRecordInfo : BaseObservableProperties
{
    /// <summary>
    /// Initializes a new instance of the AuditRecordInfo class.
    /// </summary>
    public AuditRecordInfo()
    {
    }

    /// <summary>
    /// Initializes a new instance of the AuditRecordInfo class.
    /// </summary>
    /// <param name="issuer">The issuer.</param>
    /// <param name="value">The record code.</param>
    /// <param name="uri">The link.</param>
    /// <param name="description">The optional description.</param>
    public AuditRecordInfo(IBasicPublisherInfo issuer, string value, Uri uri = null, string description = null)
        : this(value, uri, description)
    {
        Issuer = issuer;
    }

    /// <summary>
    /// Initializes a new instance of the AuditRecordInfo class.
    /// </summary>
    /// <param name="value">The record code.</param>
    /// <param name="uri">The link.</param>
    /// <param name="description">The optional description.</param>
    public AuditRecordInfo(string value, Uri uri = null, string description = null)
    {
        Value = value;
        Link = uri;
        Description = description;
    }

    /// <summary>
    /// Gets or sets the issuer information.
    /// </summary>
    [JsonPropertyName("issuer")]
    public IBasicPublisherInfo Issuer
    {
        get => GetCurrentProperty<IBasicPublisherInfo>();
        set => SetCurrentProperty(value);
    }

    /// <summary>
    /// Gets the issuer display name.
    /// </summary>
    [JsonIgnore]
    public string IssuerName => Issuer?.DisplayName;

    /// <summary>
    /// Gets or sets the record code.
    /// </summary>
    [JsonPropertyName("value")]
    public string Value
    {
        get => GetCurrentProperty<string>();
        set => SetCurrentProperty(value);
    }

    /// <summary>
    /// Gets or sets the link of the record.
    /// </summary>
    [JsonIgnore]
    public Uri Link
    {
        get => GetCurrentProperty<Uri>();
        set => SetCurrentProperty(value);
    }

    /// <summary>
    /// Gets or sets the link of the record.
    /// </summary>
    [JsonPropertyName("url")]
    public string LinkUrl
    {
        get => Link?.OriginalString;
        set => Link = new Uri(value, UriKind.RelativeOrAbsolute);
    }

    /// <summary>
    /// Gets or sets the additional description of the record.
    /// </summary>
    [JsonPropertyName("description")]
    public string Description
    {
        get => GetCurrentProperty<string>();
        set => SetCurrentProperty(value);
    }
}
