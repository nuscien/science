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
using System.Xml.Linq;
using Trivial.Data;
using Trivial.Users;

namespace Trivial.Text;

/// <summary>
/// The method information of message sender.
/// </summary>
[JsonConverter(typeof(ExtendedChatMessageConverter.InternalConverter))]
[Guid("DEDC5045-1A66-40C6-9079-59F7DAC03CA8")]
public class MessageSenderMethodInfo : RelatedResourceEntityInfo
{
    /// <summary>
    /// Initializes a new instance of the MessageSenderMethodInfo class.
    /// </summary>
    /// <param name="args">The initialization arguments.</param>
    /// <param name="type">The connection account entity type.</param>
    /// <param name="config">The connection configuration settings.</param>
    /// <param name="description">The description.</param>
    public MessageSenderMethodInfo(RelatedResourceEntityArgs args, string type, JsonObjectNode config, string description = null)
        : base(args)
    {
        Provider = type;
        Config = config;
        Description = description;
    }

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
        Config = config;
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
        Config = config;
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
    /// Initializes a new instance of the MessageSenderMethodInfo class.
    /// </summary>
    /// <param name="json">The JSON object to parse.</param>
    public MessageSenderMethodInfo(JsonObjectNode json)
        : base(json)
    {
    }

    /// <summary>
    /// Gets the name or kind of provider (telecom operator, SNS or email).
    /// </summary>
    [DataMember(Name = "type")]
    [JsonPropertyName("type")]
    [Description("The name or kind of provider.")]
    public string Provider { get; }

    /// <summary>
    /// Gets the connection account identifier or logname.
    /// </summary>
    [DataMember(Name = "name")]
    [JsonPropertyName("name")]
    [Description("The connection account identifier or logname.")]
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
    public JsonObjectNode Config
    {
        get => GetConfig();
        set => SetConfig(value);
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

    /// <inheritdoc />
    protected override void Fill(JsonObjectNode json)
    {
        base.Fill(json);
        Description = json.TryGetStringTrimmedValue("desc", true);
    }

    /// <summary>
    /// Converts to JSON object.
    /// </summary>
    /// <returns>A JSON object.</returns>
    public override JsonObjectNode ToJson()
    {
        var json = base.ToJson();
        json.SetValueIfNotEmpty("desc", Description);
        return json;
    }
}

/// <summary>
/// JSON value node converter.
/// </summary>
internal sealed class SenderMethodConverter : JsonObjectHostConverter<MessageSenderMethodInfo>
{
    /// <inheritdoc />
    protected override MessageSenderMethodInfo Create(JsonObjectNode json)
        => new(json);
}
