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
using System.Xml.Linq;
using Trivial.Data;
using Trivial.Users;

namespace Trivial.Text;

/// <summary>
/// The method information of message sender.
/// </summary>
[Guid("DEDC5045-1A66-40C6-9079-59F7DAC03CA8")]
public class MessageSenderMethodInfo : UserItemRelatedInfo
{
    /// <summary>
    /// Initializes a new instance of the MessageSenderMethodInfo class.
    /// </summary>
    /// <param name="owner">The owner.</param>
    /// <param name="type">The connection account entity type.</param>
    /// <param name="name">The connection account identifier or logname.</param>
    /// <param name="config">The connection configuration settings.</param>
    /// <param name="description">The description.</param>
    /// <param name="creation">The creation date time.</param>
    public MessageSenderMethodInfo(BaseUserItemInfo owner, string type, string name, JsonObjectNode config, string description = null, DateTime? creation = null)
        : this(Guid.NewGuid(), owner, type, name, config, description, creation)
    {
    }

    /// <summary>
    /// Initializes a new instance of the MessageSenderMethodInfo class.
    /// </summary>
    /// <param name="owner">The owner.</param>
    /// <param name="type">The connection account entity type.</param>
    /// <param name="name">The connection account identifier or logname.</param>
    /// <param name="description">The description.</param>
    /// <param name="creation">The creation date time.</param>
    public MessageSenderMethodInfo(BaseUserItemInfo owner, string type, string name, string description = null, DateTime? creation = null)
        : this(Guid.NewGuid(), owner, type, name, null, description, creation)
    {
    }

    /// <summary>
    /// Initializes a new instance of the MessageSenderMethodInfo class.
    /// </summary>
    /// <param name="id">The resource identifier.</param>
    /// <param name="owner">The owner.</param>
    /// <param name="type">The connection account entity type.</param>
    /// <param name="name">The connection account identifier or logname.</param>
    /// <param name="config">The connection configuration settings.</param>
    /// <param name="description">The description.</param>
    /// <param name="creation">The creation date time.</param>
    public MessageSenderMethodInfo(string id, BaseUserItemInfo owner, string type, string name, JsonObjectNode config, string description = null, DateTime? creation = null)
        : base(id, owner, creation)
    {
        Provider = type;
        Name = name;
        ConfigInfo = config;
        Description = description;
    }

    /// <summary>
    /// Initializes a new instance of the MessageSenderMethodInfo class.
    /// </summary>
    /// <param name="id">The resource identifier.</param>
    /// <param name="owner">The owner.</param>
    /// <param name="type">The connection account entity type.</param>
    /// <param name="name">The connection account identifier or logname.</param>
    /// <param name="config">The connection configuration settings.</param>
    /// <param name="description">The description.</param>
    /// <param name="creation">The creation date time.</param>
    public MessageSenderMethodInfo(Guid id, BaseUserItemInfo owner, string type, string name, JsonObjectNode config, string description = null, DateTime? creation = null)
        : base(id, owner, creation)
    {
        Provider = type;
        Name = name;
        ConfigInfo = config;
        Description = description;
    }

    /// <summary>
    /// Initializes a new instance of the MessageSenderMethodInfo class.
    /// </summary>
    /// <param name="id">The resource identifier.</param>
    /// <param name="owner">The owner.</param>
    /// <param name="type">The connection account entity type.</param>
    /// <param name="name">The connection account identifier or logname.</param>
    /// <param name="description">The description.</param>
    /// <param name="creation">The creation date time.</param>
    public MessageSenderMethodInfo(string id, BaseUserItemInfo owner, string type, string name, string description = null, DateTime? creation = null)
        : this(id, owner, type, name, null, description, creation)
    {
    }

    /// <summary>
    /// Initializes a new instance of the MessageSenderMethodInfo class.
    /// </summary>
    /// <param name="id">The resource identifier.</param>
    /// <param name="owner">The owner.</param>
    /// <param name="type">The connection account entity type.</param>
    /// <param name="name">The connection account identifier or logname.</param>
    /// <param name="description">The description.</param>
    /// <param name="creation">The creation date time.</param>
    public MessageSenderMethodInfo(Guid id, BaseUserItemInfo owner, string type, string name, string description = null, DateTime? creation = null)
        : this(id, owner, type, name, null, description, creation)
    {
    }

    /// <summary>
    /// Gets the name or kind of provider (telecom operator, SNS or email).
    /// </summary>
    [JsonIgnore]
#if NETCOREAPP
    [Column("provider")]
    [MaxLength(255)]
#endif
    public string Provider { get; set; }

    /// <summary>
    /// Gets the connection account identifier or logname.
    /// </summary>
    [DataMember(Name = "name")]
    [JsonPropertyName("name")]
    [Description("The connection account identifier or logname.")]
#if NETCOREAPP
    [Column("name")]
    [MaxLength(255)]
#endif
    public string Name
    {
        get => GetCurrentProperty<string>();
        protected set => GetCurrentProperty(value);
    }

    /// <summary>
    /// Gets or sets the description.
    /// </summary>
    [DataMember(Name = "desc")]
    [JsonPropertyName("desc")]
    [Description("The description.")]
#if NETCOREAPP
    [Column("desc")]
#endif
    public string Description
    {
        get => GetCurrentProperty<string>();
        set => GetCurrentProperty(value);
    }

    /// <summary>
    /// Gets the additional connection configuration settings.
    /// </summary>
    [DataMember(Name = "config")]
    [JsonPropertyName("config")]
    [Description("The additional connection configuration settings.")]
#if NETCOREAPP
    [Column("config")]
#endif
    public new JsonObjectNode ConfigInfo
    {
        get => base.ConfigInfo;
        set => base.ConfigInfo = value;
    }

    /// <inheritdoc />
    [JsonIgnore]
#if NETCOREAPP
    [NotMapped]
#endif
    protected override string Supertype => "sender";

    /// <inheritdoc />
    [JsonIgnore]
#if NETCOREAPP
    [NotMapped]
#endif
    protected override string ResourceType => Provider;
}
