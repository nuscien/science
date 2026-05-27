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
using Trivial.Tasks;
using Trivial.Text;
using Trivial.Users;
using Trivial.Web;

namespace Trivial.Security;

/// <summary>
/// The agent or proxy of another account entity.
/// </summary>
[Guid("1FA2B538-3658-4349-B3DE-70E778FC7787")]
public class AgentAccountItemInfo : BaseUserItemInfo
{
    /// <summary>
    /// Initializes a new instance of the AgentAccountItemInfo class.
    /// </summary>
    public AgentAccountItemInfo()
        : base(AccountEntityTypes.Agent)
    {
        Scope = new();
    }

    /// <summary>
    /// Initializes a new instance of the AgentAccountItemInfo class.
    /// </summary>
    /// <param name="args">The initialization arguments.</param>
    /// <param name="nickname">The nickname or display name.</param>
    /// <param name="avatar">The avatar URI.</param>
    public AgentAccountItemInfo(ResourceEntityArgs args, string nickname = null, Uri avatar = null)
        : base(AccountEntityTypes.Agent, args, nickname, Genders.Asexual, avatar)
    {
        Scope = new();
    }

    /// <summary>
    /// Initializes a new instance of the AgentAccountItemInfo class.
    /// </summary>
    /// <param name="args">The initialization arguments.</param>
    public AgentAccountItemInfo(AccountEntityArgs args)
        : base(AccountEntityTypes.Agent, args, Genders.Asexual)
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
        : base(AccountEntityTypes.Agent, id, nickname, Genders.Asexual, avatar, creation)
    {
        Scope = new();
    }

    /// <summary>
    /// Gets or sets the type of the subject of the agent.
    /// </summary>
    [JsonIgnore]
    [Description("The type of the subject of the agent.")]
#if NETCOREAPP
    [Column("subtype")]
#endif
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
#if NETCOREAPP
    [Column("subject")]
#endif
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
#if NETCOREAPP
    [NotMapped]
#endif
    public List<string> Scope
    {
        get => GetCurrentProperty<List<string>>();
        private set => SetCurrentProperty(value);
    }

    /// <summary>
    /// Gets or sets the scope in string separated by white space.
    /// </summary>
#if NETCOREAPP
    [Column("scope")]
#endif
    public string ScopeString
    {
        get
        {
            var arr = Scope;
            return arr is null ? string.Empty : string.Join(" ", arr);
        }

        set
        {
            var arr = Scope;
            if (arr is null)
            {
                arr = new();
                if (Scope is not null)
                {
                    arr = Scope;
                    if (arr is null) arr = new();
                }

                Scope = arr;
            }

            if (string.IsNullOrWhiteSpace(value)) arr.Clear();
            var list = value.Split(value.Contains(";") ? new[] { ';' } : new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            arr.Clear();
            foreach (var item in list)
            {
                var s = item.Trim();
                if (s.Length < 1 || arr.Contains(s)) continue;
                arr.Add(s);
            }
        }
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
    public void SetSubject(BaseAccountEntityInfo entity)
    {
        if (entity == null)
        {
            SubjectType = null;
            SubjectId = null;
            return;
        }

        SubjectType = entity.AccountEntityType.ToString();
        SubjectId = entity.Id;
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
}
