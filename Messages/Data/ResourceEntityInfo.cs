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
using System.Xml.Linq;
using Trivial.Reflection;
using Trivial.Text;
using Trivial.Web;

namespace Trivial.Data;

/// <summary>
/// The base resource entity information.
/// </summary>
public abstract class BaseResourceEntityInfo : BaseObservableProperties, IJsonObjectHost
{
    /// <summary>
    /// The configuration node.
    /// </summary>
    private JsonObjectNode configInfo;

    /// <summary>
    /// Initializes a new instance of the BaseResourceObservableProperties class.
    /// </summary>
    protected BaseResourceEntityInfo()
    {
        InitializationTime = DateTime.Now;
        State = ResourceEntityStates.Normal;
        CreationTime = LastModificationTime = InitializationTime;
        LastSavingStatus = new();
    }

    /// <summary>
    /// Initializes a new instance of the BaseResourceObservableProperties class.
    /// </summary>
    /// <param name="id">The identifier.</param>
    protected BaseResourceEntityInfo(Guid id)
        : this(id.ToString("N"))
    {
        State = ResourceEntityStates.Normal;
    }

    /// <summary>
    /// Initializes a new instance of the BaseResourceObservableProperties class.
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <param name="creation">The creation date time.</param>
    protected BaseResourceEntityInfo(Guid id, DateTime? creation)
        : this(id.ToString("N"), creation)
    {
    }

    /// <summary>
    /// Initializes a new instance of the BaseResourceObservableProperties class.
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <param name="creation">The creation date time.</param>
    /// <param name="modification">The last modification date time.</param>
    protected BaseResourceEntityInfo(Guid id, DateTime creation, DateTime modification)
        : this(id.ToString("N"), creation, modification)
    {
    }

    /// <summary>
    /// Initializes a new instance of the BaseResourceObservableProperties class.
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <param name="initTime">true if initialize the creation and last modification time.</param>
    private BaseResourceEntityInfo(string id, bool initTime)
    {
        InitializationTime = DateTime.Now;
        Id = id;
        State = ResourceEntityStates.Normal;
        LastSavingStatus = new();
        if (initTime) CreationTime = LastModificationTime = InitializationTime;
    }

    /// <summary>
    /// Initializes a new instance of the BaseResourceObservableProperties class.
    /// </summary>
    /// <param name="id">The identifier.</param>
    protected BaseResourceEntityInfo(string id)
        : this(id, true)
    {
    }

    /// <summary>
    /// Initializes a new instance of the BaseResourceObservableProperties class.
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <param name="creation">The creation date time.</param>
    protected BaseResourceEntityInfo(string id, DateTime? creation)
        : this(id, false)
    {
        CreationTime = creation ?? DateTime.Now;
        LastModificationTime = CreationTime;
    }

    /// <summary>
    /// Initializes a new instance of the BaseResourceObservableProperties class.
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <param name="creation">The creation date time.</param>
    /// <param name="modification">The last modification date time.</param>
    protected BaseResourceEntityInfo(string id, DateTime creation, DateTime modification)
        : this(id, false)
    {
        CreationTime = creation;
        LastModificationTime = modification;
    }

    /// <summary>
    /// Initializes a new instance of the BaseResourceObservableProperties class.
    /// </summary>
    /// <param name="json">The JSON node to read and fill.</param>
    internal BaseResourceEntityInfo(JsonObjectNode json)
    {
        InitializationTime = DateTime.Now;
        LastSavingStatus = new();
        if (json == null) return;
        State = ResourceEntityStates.Normal;
        CreationTime = json.TryGetDateTimeValue("created") ?? DateTime.Now;
        LastModificationTime = CreationTime;
        try
        {
            Fill(json);
        }
        catch (InvalidOperationException)
        {
        }
        catch (ArgumentException)
        {
        }
        catch (JsonException)
        {
        }
        catch (InvalidCastException)
        {
        }
    }

    /// <summary>
    /// Gets or sets the identifier.
    /// </summary>
    [DataMember(Name = "id")]
    [JsonPropertyName("$id")]
    [Description("The unique identifier of the entity.")]
    public string Id
    {
        get => GetCurrentProperty<string>();
        protected set => SetCurrentProperty(value?.Trim());
    }

    /// <summary>
    /// Gets or sets the JSON schema of the entity.
    /// </summary>
    [JsonIgnore]
#if NETCOREAPP
    [NotMapped]
#endif
    public string Schema { get; protected set; }

    /// <summary>
    /// Gets or sets the state.
    /// </summary>
    [DataMember(Name = "state")]
    [JsonPropertyName("state")]
    [JsonConverter(typeof(JsonIntegerEnumCompatibleConverter<ResourceEntityStates>))]
    public ResourceEntityStates State
    {
        get => GetCurrentProperty<ResourceEntityStates>();
        set => SetCurrentProperty(value);
    }

    /// <summary>
    /// Gets or sets the date time when the entity creates.
    /// </summary>
    [DataMember(Name = "created")]
    [JsonPropertyName("created")]
    [Description("The date time when the entity creates.")]
    public DateTime CreationTime
    {
        get => GetCurrentProperty<DateTime>();
        protected set => SetCurrentProperty(value);
    }

    /// <summary>
    /// Gets or sets the date time when the entity updates recently.
    /// </summary>
    [DataMember(Name = "updated")]
    [JsonPropertyName("updated")]
    [Description("The date time when the entity updates recently.")]
    public DateTime LastModificationTime
    {
        get => GetCurrentProperty<DateTime>();
        set => SetCurrentProperty(value);
    }

    /// <summary>
    /// Gets the date time of this instance initialization.
    /// </summary>
    [JsonIgnore]
#if NETCOREAPP
    [NotMapped]
#endif
    public DateTime InitializationTime { get; }

    /// <summary>
    /// Gets the latest saving status of this entity.
    /// </summary>
    [JsonIgnore]
#if NETCOREAPP
    [NotMapped]
#endif
    public ResourceEntitySavingStatus LastSavingStatus
    {
        get => GetCurrentProperty<ResourceEntitySavingStatus>();
        private set => SetCurrentProperty(value);
    }

    /// <summary>
    /// Gets the supertype.
    /// </summary>
    [JsonIgnore]
#if NETCOREAPP
    [NotMapped]
#endif
    protected virtual string Supertype { get; }

    /// <summary>
    /// Gets the resource entity type.
    /// </summary>
    [JsonIgnore]
#if NETCOREAPP
    [NotMapped]
#endif
    protected virtual string ResourceType { get; }

    /// <summary>
    /// Gets or sets the identifier.
    /// </summary>
    [DataMember(Name = "rev")]
    [JsonPropertyName("rev")]
    [Description("The hash value of the entity revision.")]
    protected string RevisionId
    {
        get => GetCurrentProperty<string>();
        set => SetCurrentProperty(value);
    }

    /// <summary>
    /// Converts to JSON object.
    /// </summary>
    /// <returns>A JSON object.</returns>
    public virtual JsonObjectNode ToJson()
    {
        var json = new JsonObjectNode
        {
            TypeDiscriminator = Supertype,
            Schema = Schema,
            Id = Id,
        };
        json.SetValueIfNotEmpty("type", ResourceType ?? GetType().FullName);
        var name = base.GetProperty<string>("Name")?.Trim();
        json.SetValueIfNotEmpty("name", name);
        json.SetValueIfNotNull("config", configInfo);
        json.SetValue("state", State.ToString());
        json.SetValue("created", CreationTime);
        json.SetValue("updated", LastModificationTime);
        json.SetValueIfNotEmpty("rev", RevisionId);
        return json;
    }

    /// <summary>
    /// Updates the saving status of this entity.
    /// </summary>
    /// <param name="state">The new state.</param>
    /// <param name="message">The additional message about this saving status.</param>
    public void UpdateSavingStatus(ResourceEntitySavingStates state, string message = null)
    {
        var status = LastSavingStatus;
        if (status == null)
        {
            status = new();
            if (LastSavingStatus == null)
            {
                LastSavingStatus = status;
            }
            else
            {
                var tmp = status;
                status = LastSavingStatus;
                if (status == null)
                {
                    LastSavingStatus = tmp;
                    status = tmp;
                }
            }
        }

        status.Update(state, message);
    }

    /// <summary>
    /// Updates the saving status of this entity.
    /// </summary>
    /// <param name="ex">The exception thrown during saving.</param>
    public void UpdateSavingStatus(Exception ex)
        => UpdateSavingStatus(ResourceEntitySavingStates.Failure, ex?.Message);

    /// <summary>
    /// Gets a property value.
    /// </summary>
    /// <typeparam name="T">The type of the property value.</typeparam>
    /// <param name="key">The key.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <returns>A property value.</returns>
    public new T GetProperty<T>(string key, T defaultValue = default)
        => base.GetProperty(key, defaultValue);

    /// <summary>
    /// Gets a property value.
    /// </summary>
    /// <typeparam name="T">The type of the property value.</typeparam>
    /// <param name="key">The key.</param>
    /// <param name="result">The property value.</param>
    /// <returns>true if contains; otherwise, false.</returns>
    public new bool GetProperty<T>(string key, out T result)
        => base.GetProperty(key, out result);

    /// <summary>
    /// Tests if the specific identifier and supertype are same as this entity.
    /// </summary>
    /// <param name="id">The entity identifier to compare.</param>
    /// <param name="supertype">The supertype to compare.</param>
    /// <returns>true if it is the same; otherwise, false.</returns>
    public bool IsSameId(string id, string supertype)
        => Id == id && Supertype == supertype;

    /// <summary>
    /// Tests if the entity is created after the given date time.
    /// </summary>
    /// <param name="time">The target date time to test.</param>
    /// <returns>true if the entity is created after the specific date time; otherwise, false.</returns>
    public bool IsCreatedAfter(DateTime time)
        => CreationTime > time;

    /// <summary>
    /// Tests if the entity is created after the given duration.
    /// </summary>
    /// <param name="duration">The target time span to test.</param>
    /// <returns>true if the entity is created after the specific duration before now; otherwise, false.</returns>
    public bool IsCreatedAfter(TimeSpan duration)
        => (DateTime.Now - CreationTime) < duration;

    /// <summary>
    /// Tests if the entity is created before the given date time.
    /// </summary>
    /// <param name="time">The target date time to test.</param>
    /// <returns>true if the entity is created before the specific date time; otherwise, false.</returns>
    public bool IsCreatedBefore(DateTime time)
        => CreationTime < time;

    /// <summary>
    /// Tests if the entity is created before the given duration.
    /// </summary>
    /// <param name="duration">The target time span to test.</param>
    /// <returns>true if the entity is created before the specific duration before now; otherwise, false.</returns>
    public bool IsCreatedBefore(TimeSpan duration)
        => (DateTime.Now - CreationTime) > duration;

    /// <summary>
    /// Tests if the entity is modified after the given date time.
    /// </summary>
    /// <param name="time">The target date time to test.</param>
    /// <returns>true if the entity is modified after the specific date time; otherwise, false.</returns>
    public bool IsModifiedAfter(DateTime time)
        => LastModificationTime > time;

    /// <summary>
    /// Tests if the entity is modified after the given duration.
    /// </summary>
    /// <param name="duration">The target time span to test.</param>
    /// <returns>true if the entity is modified after the specific duration before now; otherwise, false.</returns>
    public bool IsModifiedAfter(TimeSpan duration)
        => (DateTime.Now - LastModificationTime) < duration;

    /// <summary>
    /// Tests if the entity is modified before the given date time.
    /// </summary>
    /// <param name="time">The target date time to test.</param>
    /// <returns>true if the entity is modified before the specific date time; otherwise, false.</returns>
    public bool IsModifiedBefore(DateTime time)
        => LastModificationTime < time;

    /// <summary>
    /// Tests if the entity is modified before the given duration.
    /// </summary>
    /// <param name="duration">The target time span to test.</param>
    /// <returns>true if the entity is modified before the specific duration before now; otherwise, false.</returns>
    public bool IsModifiedBefore(TimeSpan duration)
        => (DateTime.Now - LastModificationTime) > duration;

    /// <summary>
    /// Sets the configuration information.
    /// </summary>
    /// <returns>The JSON object node of configuration of this item; or null, if not supported.</returns>
    protected JsonObjectNode GetConfig()
        => configInfo;

    /// <summary>
    /// Sets the configuration information.
    /// </summary>
    /// <param name="init">true if initializes an object if null; otherwise, false.</param>
    /// <returns>The JSON object node of configuration of this item; or null, if not supported.</returns>
    protected JsonObjectNode GetConfig(bool init)
    {
        if (configInfo == null && init)
        {
            var json = new JsonObjectNode();
            if (configInfo == null) configInfo = json;
        }

        return configInfo;
    }

    /// <summary>
    /// Sets the configuration information.
    /// </summary>
    /// <param name="json">The JSON object node of this item; or null, if not supported; or an empty JSON object, if supports but no settings or additional information.</param>
    protected virtual void SetConfig(JsonObjectNode json)
        => configInfo = json;

    /// <summary>
    /// Writes this instance to the specified writer as a JSON value.
    /// </summary>
    /// <param name="writer">The writer to which to write this instance.</param>
    protected override void WriteTo(Utf8JsonWriter writer)
        => ToJson().WriteTo(writer);

    /// <summary>
    /// Renews the revision and updates the last modification date time.
    /// </summary>
    /// <returns>The new revision identifier.</returns>
    protected virtual string RenewRevision()
    {
        var rev = Guid.NewGuid().ToString();
        LastModificationTime = DateTime.Now;
        RevisionId = rev;
        return rev;
    }

    /// <summary>
    /// Checks if the identifier is null, empty or consists only of white-space characters.
    /// </summary>
    /// <returns>true if the ientifier is null or empty, or if it consists exclusively of white-space characters; otherwise, false.</returns>
    protected bool IsIdEmpty()
        => string.IsNullOrWhiteSpace(Id);

    /// <summary>
    /// Fills the properties of JSON object to this entity.
    /// </summary>
    /// <param name="json">The JSON object to read.</param>
    /// <exception cref="InvalidOperationException">The identifier is not matched.</exception>
    protected virtual void Fill(JsonObjectNode json)
    {
        var id = json.TryGetId(out _);
        if (id != null)
        {
            if (string.IsNullOrWhiteSpace(Id)) Id = id;
            else if (id != Id) throw new InvalidOperationException("The entity identifier was not matched.");
        }

        var name = json.TryGetStringTrimmedValue("name", true);
        if (name != null) SetProperty("Name", name);
        var config = json.TryGetObjectValue("config");
        if (config != null) configInfo = config;
        var state = json.TryGetEnumValue<ResourceEntityStates>("state");
        if (state.HasValue) State = state.Value;
        var updated = json.TryGetDateTimeValue("updated");
        if (updated.HasValue) LastModificationTime = updated.Value;
        var revKind = json.GetValueKind("rev");
        switch (revKind)
        {
            case JsonValueKind.Undefined:
                break;
            case JsonValueKind.True:
                RevisionId = Guid.NewGuid().ToString();
                break;
            case JsonValueKind.False:
            case JsonValueKind.Null:
                RevisionId = null;
                break;
            case JsonValueKind.Object:
                {
                    var revObj = json.TryGetObjectValue("rev");
                    if (revObj == null)
                    {
                        RevisionId = null;
                        break;
                    }

                    var action = revObj.TryGetStringTrimmedValue("action", true) ?? string.Empty;
                    switch (action.ToLowerInvariant())
                    {
                        case "renew":
                        case "update":
                            RevisionId = Guid.NewGuid().ToString();
                            break;
                        case "clear":
                        case "null":
                            RevisionId = null;
                            break;
                        case "keep":
                        case "":
                            break;
                        case "init":
                            if (string.IsNullOrEmpty(RevisionId)) RevisionId = Guid.NewGuid().ToString();
                            break;
                        case "date":
                        case "time":
                        case "now":
                        case "tick":
                            RevisionId = WebFormat.ParseDate(DateTime.Now).ToString();
                            break;
                    }

                    if (revObj.TryGetBooleanValue("update") == true && !updated.HasValue) LastModificationTime = DateTime.Now;
                    break;
                }
            default:
                RevisionId = json.TryGetStringTrimmedValue("rev", true);
                break;
        }
    }
}
