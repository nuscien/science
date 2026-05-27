using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security.AccessControl;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Xml.Linq;
using Trivial.Reflection;
using Trivial.Text;
using Trivial.Users;
using Trivial.Web;

namespace Trivial.Data;

/// <summary>
/// The base resource entity information.
/// </summary>
public abstract class BaseResourceEntityInfo : BaseObservableProperties
{
    /// <summary>
    /// Initializes a new instance of the BaseResourceEntityInfo class.
    /// </summary>
    protected BaseResourceEntityInfo()
    {
        InitializationTime = DateTime.Now;
        State = ResourceEntityStates.Normal;
        CreationTime = LastModificationTime = InitializationTime;
        LastSavingStatus = new();
    }

    /// <summary>
    /// Initializes a new instance of the BaseResourceEntityInfo class.
    /// </summary>
    /// <param name="args">The initialization arguments.</param>
    protected BaseResourceEntityInfo(ResourceEntityArgs args)
    {
        args ??= new(true);
        var creation = args.CreationTime;
        var diff = (DateTime.Now - creation).TotalSeconds;
        InitializationTime = diff > 0 && diff < 1 ? creation : DateTime.Now;
        Id = args.Id;
        State = args.State;
        CreationTime = creation;
        LastModificationTime = args.LastModificationTime;
        LastSavingStatus = args.LastSavingStatus ?? new();
        if (!string.IsNullOrWhiteSpace(args.Name)) SetProperty("Name", args.Name);
        if (!string.IsNullOrWhiteSpace(args.RevisionId)) RevisionId = args.RevisionId;
    }

    /// <summary>
    /// Initializes a new instance of the BaseResourceEntityInfo class.
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <param name="creation">The creation date time.</param>
    protected BaseResourceEntityInfo(Guid id, DateTime? creation = null)
        : this(id.ToString("N"), creation)
    {
    }

    /// <summary>
    /// Initializes a new instance of the BaseResourceEntityInfo class.
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <param name="creation">The creation date time.</param>
    /// <param name="modification">The last modification date time.</param>
    protected BaseResourceEntityInfo(Guid id, DateTime creation, DateTime modification)
        : this(id.ToString("N"), creation, modification)
    {
    }

    /// <summary>
    /// Initializes a new instance of the BaseResourceEntityInfo class.
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <param name="creation">The creation date time.</param>
    protected BaseResourceEntityInfo(string id, DateTime? creation = null)
    {
        InitializationTime = DateTime.Now;
        Id = id;
        State = ResourceEntityStates.Normal;
        LastSavingStatus = new();
        if (creation.HasValue) CreationTime = creation ?? InitializationTime;
        LastModificationTime = CreationTime;
    }

    /// <summary>
    /// Initializes a new instance of the BaseResourceEntityInfo class.
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <param name="creation">The creation date time.</param>
    /// <param name="modification">The last modification date time.</param>
    protected BaseResourceEntityInfo(string id, DateTime creation, DateTime modification)
    {
        InitializationTime = DateTime.Now;
        Id = id;
        State = ResourceEntityStates.Normal;
        LastSavingStatus = new();
        CreationTime = creation;
        LastModificationTime = modification;
    }

    /// <summary>
    /// Gets or sets the identifier.
    /// </summary>
    [DataMember(Name = "id")]
    [JsonPropertyName("$id")]
    [Description("The unique identifier of the entity.")]
#if NETCOREAPP
    [Column("id")]
    [MaxLength(ResourceEntityUtils.MAX_ID_LENGTH)]
#endif
    public string Id
    {
        get => GetCurrentProperty<string>();
        set => SetCurrentProperty(value?.Trim());
    }

    /// <summary>
    /// Gets or sets the state.
    /// </summary>
    [DataMember(Name = "state")]
    [JsonPropertyName("state")]
    [JsonConverter(typeof(JsonIntegerEnumCompatibleConverter<ResourceEntityStates>))]
    [Description("The state of the entity, including Deleted, Recycle (in trash bin), Placehoder (pending to create), Progress (filling properties), Request (pending to approve), Draft, Publishing, Normal (in service).")]
#if NETCOREAPP
    [Column("state")]
#endif
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
#if NETCOREAPP
    [Column("created")]
#endif
    public DateTime CreationTime
    {
        get => GetCurrentProperty<DateTime>();
        set => SetCurrentProperty(value);
    }

    /// <summary>
    /// Gets or sets the date time when the entity updates recently.
    /// </summary>
    [DataMember(Name = "updated")]
    [JsonPropertyName("updated")]
    [Description("The date time when the entity updates recently.")]
#if NETCOREAPP
    [Column("updated")]
#endif
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
    /// Gets the display name of this entity.
    /// </summary>
    /// <returns>The display name.</returns>
    [JsonIgnore]
    [Description("The display name of this entity.")]
#if NETCOREAPP
    [NotMapped]
#endif
    public virtual string DisplayName { get; }

    /// <summary>
    /// Gets the supertype of the resource entity.
    /// </summary>
    /// <remarks>
    /// The length should be less than 80.
    /// </remarks>
    [DataMember(Name = "supertype")]
    [JsonInclude]
    [JsonPropertyName("supertype")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenReading)]
    [Description("The supertype of the resource, e.g. message, account, article, task, etc.")]
#if NETCOREAPP
    [MaxLength(ResourceEntityUtils.MAX_ID_LENGTH)]
    [NotMapped]
#endif
    protected virtual string Supertype { get; }

    /// <summary>
    /// Gets the resource entity type.
    /// </summary>
    /// <remarks>
    /// It is used to identify the type of the resource entity.
    /// The length should be less than 80.
    /// </remarks>
    [DataMember(Name = "type")]
    [JsonInclude]
    [JsonPropertyName("$type")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenReading)]
    [Description("The type of the resource.")]
#if NETCOREAPP
    [MaxLength(ResourceEntityUtils.MAX_ID_LENGTH)]
    [NotMapped]
#endif
    protected virtual string ResourceType { get; }

    /// <summary>
    /// Gets or sets the optional JSON schema of the entity.
    /// </summary>
    [JsonInclude]
    [DataMember(Name = "schema")]
    [JsonPropertyName("$schema")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [Description("The optional JSON shema.")]
#if NETCOREAPP
    [NotMapped]
#endif
    protected string Schema { get; set; }

    /// <summary>
    /// The additional configuration node.
    /// </summary>
    [JsonInclude]
    [DataMember(Name = "config")]
    [JsonPropertyName("config")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [Description("The additional configuration node.")]
#if NETCOREAPP
    [NotMapped]
#endif
    protected JsonObjectNode ConfigInfo
    {
        get => GetCurrentProperty<JsonObjectNode>();
        set => SetCurrentProperty(value);
    }

    /// <summary>
    /// Gets or sets the hash value of the entity revision.
    /// It will change on entity updating.
    /// </summary>
    [DataMember(Name = "rev")]
    [JsonInclude]
    [JsonPropertyName("rev")]
    [Description("The hash value of the entity revision.")]
#if NETCOREAPP
    [Column("rev")]
    [MaxLength(ResourceEntityUtils.MAX_ID_LENGTH)]
#endif
    protected string RevisionId
    {
        get => GetCurrentProperty<string>();
        set => SetCurrentProperty(value);
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
    /// Gets the specific property value.
    /// </summary>
    /// <param name="key">The property key.</param>
    /// <returns>The value of the property.</returns>

    public string GetProperty(ResourceEntitySpecialProperties key)
        => key switch
        {
            ResourceEntitySpecialProperties.Id => Id,
            ResourceEntitySpecialProperties.DisplayName => DisplayName,
            ResourceEntitySpecialProperties.ResourceType => ResourceType,
            ResourceEntitySpecialProperties.Supertype => Supertype,
            ResourceEntitySpecialProperties.Schema => Schema,
            ResourceEntitySpecialProperties.State => State.ToString(),
            _ => null
        };

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

    /// <inheritdoc />
    protected override PropertySettingPolicies IsPropertyValid(ChangeEventArgs<object> ev)
    {
        switch (ev.Key)
        {
            case nameof(Id):
                {
                    if (ev.OldValue is null || ev.OldValue is DBNull || ev.Method == ChangeMethods.Add) return PropertySettingPolicies.Allow;
                    if (ev.NewValue is null || ev.NewValue is DBNull) return PropertySettingPolicies.Skip;
                    if (ev.NewValue is not string id)
                    {
                        if (ev.NewValue is int i1) id = i1.ToString();
                        else if (ev.NewValue is long i2) id = i2.ToString();
                        else if (ev.NewValue is uint i3) id = i3.ToString();
                        else if (ev.NewValue is ulong i4) id = i4.ToString();
                        else if (ev.NewValue is Guid i5) id = i5.ToString("N");
                        else return PropertySettingPolicies.Forbidden;
                    }
                    else
                    {
                        id = id.Trim().Replace("-", string.Empty);
                        if (id.Length < 1) return PropertySettingPolicies.Skip;
                        if (id.Length > ResourceEntityUtils.MAX_ID_LENGTH) id = id.Substring(0, ResourceEntityUtils.MAX_ID_LENGTH);
                    }

                    if (ev.OldValue is not string s)
                    {
                        if (ev.OldValue is int i1) s = i1.ToString();
                        else if (ev.OldValue is long i2) s = i2.ToString();
                        else if (ev.OldValue is uint i3) s = i3.ToString();
                        else if (ev.OldValue is ulong i4) s = i4.ToString();
                        else if (ev.OldValue is Guid i5) s = i5.ToString("N");
                        else return PropertySettingPolicies.Forbidden;
                    }
                    else
                    {
                        s = s.Trim().Replace("-", string.Empty);
                        if (s.Length < 1) return PropertySettingPolicies.Allow;
                    }

                    return s.Equals(id, StringComparison.OrdinalIgnoreCase) ? PropertySettingPolicies.Skip : PropertySettingPolicies.Forbidden;
                }
            case nameof(CreationTime):
                if (ev.OldValue is null || ev.OldValue is DBNull || ev.Method == ChangeMethods.Add) return PropertySettingPolicies.Allow;
                if (ev.NewValue is null || ev.NewValue is DBNull) return PropertySettingPolicies.Skip;
                if (ev.OldValue is not DateTime dt)
                {
                    if (ev.OldValue is long i)
                    {
                        dt = WebFormat.ParseDate(i);
                    }
                    else if (ev.OldValue is string s)
                    {
                        var ndt = WebFormat.ParseDate(s);
                        if (ndt.HasValue) dt = ndt.Value;
                        else return PropertySettingPolicies.Allow;
                    }
                    else
                    {
                        return PropertySettingPolicies.Forbidden;
                    }
                }

                if (ev.NewValue is not DateTime created)
                {
                    if (ev.NewValue is long i)
                    {
                        created = WebFormat.ParseDate(i);
                    }
                    else if (ev.NewValue is string s)
                    {
                        var ndt = WebFormat.ParseDate(s);
                        if (ndt.HasValue) created = ndt.Value;
                        else return PropertySettingPolicies.Skip;
                    }
                    else
                    {
                        return PropertySettingPolicies.Forbidden;
                    }
                }

                if (dt == created) return PropertySettingPolicies.Skip;
                var span = Math.Abs((dt - created).TotalSeconds);
                return span < 2 ? PropertySettingPolicies.Skip : PropertySettingPolicies.Forbidden;
            default:
                return base.IsPropertyValid(ev);
        };
    }

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
        => ResourceEntityUtils.IsEmptyId(Id);

    /// <summary>
    /// Converts to JSON object.
    /// </summary>
    /// <param name="value">The JSON value.</param>
    /// <returns>A JSON object.</returns>
    public static explicit operator JsonObjectNode(BaseResourceEntityInfo value)
        => value is IJsonObjectHost obj ? obj.ToJson() : JsonObjectNode.ConvertFrom(value);
}
