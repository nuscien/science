using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Trivial.Data;
using Trivial.Reflection;
using Trivial.Tasks;
using Trivial.Text;
using Trivial.Web;
using Trivial.Users;

namespace Trivial.Security;

/// <summary>
/// The agent item information.
/// </summary>
[JsonConverter(typeof(AgentAccountItemInfoConverter))]
public class AgentAccountItemInfo : BaseUserItemInfo
{
    /// <summary>
    /// Initializes a new instance of the AgentAccountItemInfo class.
    /// </summary>
    public AgentAccountItemInfo()
        : base(PrincipalEntityTypes.Agent)
    {
        Scope = new();
    }

    /// <summary>
    /// Initializes a new instance of the AgentAccountItemInfo class.
    /// </summary>
    /// <param name="id">The resource identifier.</param>
    /// <param name="nickname">The nickname or display name.</param>
    /// <param name="avatar">The avatar URI.</param>
    /// <param name="creation">The creation date time.</param>
    public AgentAccountItemInfo(string id, string nickname, Uri avatar = null, DateTime? creation = null)
        : base(PrincipalEntityTypes.Agent, id, nickname, Genders.Asexual, avatar, creation)
    {
        Scope = new();
    }

    /// <summary>
    /// Initializes a new instance of the AgentAccountItemInfo class.
    /// </summary>
    /// <param name="json">The JSON object to parse.</param>
    protected internal AgentAccountItemInfo(JsonObjectNode json)
        : base(json, PrincipalEntityTypes.Agent)
    {
    }

    /// <summary>
    /// Gets or sets the type of the subject of the agent.
    /// </summary>
    [JsonIgnore]
    [Description("The type of the subject of the agent.")]
    public string SubjectType
    {
        get => GetCurrentProperty<string>();
        private set => SetCurrentProperty(value);
    }

    /// <summary>
    /// Gets or sets the identifier of the subject of the agent.
    /// </summary>
    [JsonIgnore]
    [Description("The identifier of the subject of the agent.")]
    public string SubjectId
    {
        get => GetCurrentProperty<string>();
        private set => SetCurrentProperty(value);
    }

    /// <summary>
    /// Gets or sets the agent scope.
    /// </summary>
    [DataMember(Name = "scope")]
    [JsonPropertyName("scope")]
    [Description("The agent scope.")]
    public List<string> Scope
    {
        get => GetCurrentProperty<List<string>>();
        private set => SetCurrentProperty(value);
    }

    /// <summary>
    /// Adds a scope item.
    /// </summary>
    /// <param name="item">The item to add.</param>
    public void AddScope(string item)
    {
        Scope ??= new();
        Scope.Add(item);
    }

    /// <summary>
    /// Removes a scope item.
    /// </summary>
    /// <param name="item">The item to remove.</param>
    /// <returns>true if item is successfully removed; otherwise, false. This method also returns false if item was not found in the scope.</returns>
    public bool RemoveScope(string item)
    {
        if (Scope == null) return false;
        return Scope.Remove(item);
    }

    /// <summary>
    /// Sets the subject.
    /// </summary>
    /// <param name="type">The type of the subject.</param>
    /// <param name="id">The identifier of the subject.</param>
    public void SetSubject(string type, string id)
    {
        SubjectType = type;
        SubjectId = id;
    }

    /// <summary>
    /// Sets the subject.
    /// </summary>
    /// <param name="entity">The subject entity.</param>
    public void SetSubject(BasePrincipalEntityInfo entity)
    {
        if (entity == null)
        {
            SubjectType = null;
            SubjectId = null;
            return;
        }

        SubjectType = entity.PrincipalEntityType.ToString();
        SubjectId = entity.Id;
    }

    /// <inheritdoc />
    protected override void Fill(JsonObjectNode json)
    {
        base.Fill(json);
        var subject = json.TryGetObjectValue("subject");
        if (subject != null)
        {
            SubjectType = subject.TryGetStringTrimmedValue("type", true);
            SubjectId = subject.TryGetStringTrimmedValue("id", true) ?? subject.Id;
        }

        Scope = (json.TryGetStringListValue("scope", true) ?? new()).Where(ele => !string.IsNullOrWhiteSpace(ele)).Distinct().ToList();
    }

    /// <inheritdoc />
    protected override void ToString(StringBuilder sb)
    {
        sb.AppendLine();
        sb.Append("Subject = ");
        var subjectType = SubjectType?.Trim();
        if (string.IsNullOrEmpty(subjectType)) sb.Append('?');
        else sb.Append(subjectType);
        sb.Append(' ');
        sb.Append(SubjectId ?? "?");
    }

    /// <summary>
    /// Converts to JSON object.
    /// </summary>
    /// <returns>A JSON object.</returns>
    public override JsonObjectNode ToJson()
    {
        var json = base.ToJson();
        var subject = new JsonObjectNode();
        subject.SetValueIfNotEmpty("type", SubjectType);
        subject.SetValueIfNotEmpty("id", SubjectId);
        json.SetValue("subject", subject);
        var scope = Scope;
        if (scope != null) json.SetValue("scope", scope.Where(ele => !string.IsNullOrWhiteSpace(ele)).Distinct());
        return json;
    }

    /// <summary>
    /// Converts the JSON raw back.
    /// </summary>
    /// <param name="value">The source value.</param>
    /// <returns>The request instance.</returns>
    public static implicit operator AgentAccountItemInfo(JsonObjectNode value)
        => value is null ? null : new(value);

    /// <summary>
    /// Converts to JSON object.
    /// </summary>
    /// <param name="value">The JSON value.</param>
    /// <returns>A JSON object.</returns>
    public static explicit operator JsonObjectNode(AgentAccountItemInfo value)
        => value?.ToJson();
}

/// <summary>
/// JSON value node converter.
/// </summary>
internal sealed class AgentAccountItemInfoConverter : JsonObjectHostConverter<AgentAccountItemInfo>
{
    /// <inheritdoc />
    protected override AgentAccountItemInfo Create(JsonObjectNode json)
        => new(json);
}
