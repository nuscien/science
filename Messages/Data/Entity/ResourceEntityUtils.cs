using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Trivial.Collection;
using Trivial.Text;
using Trivial.Users;
using static Trivial.Reflection.ExceptionHandler;

namespace Trivial.Data;

/// <summary>
/// The utilities and extensions of resource entity info.
/// </summary>
public static class ResourceEntityUtils
{
    internal const int MAX_ID_LENGTH = 80;

    /// <summary>
    /// Deserializes to a JSON format string.
    /// </summary>
    /// <param name="entity">The entity to deserialize.</param>
    /// <returns>A JSON format string.</returns>
    public static string ToJsonString(BaseResourceEntityInfo entity)
    {
        if (entity is null) return null;
        var json = (JsonObjectNode)entity;
        return json?.ToString();
    }

    /// <summary>
    /// Deserializes to a JSON format string.
    /// </summary>
    /// <param name="entity">The entity to deserialize.</param>
    /// <param name="indentStyle">The ident style.</param>
    /// <returns>A JSON format string.</returns>
    public static string ToJsonString(BaseResourceEntityInfo entity, IndentStyles indentStyle)
    {
        if (entity is null) return null;
        var json = (JsonObjectNode)entity;
        return json?.ToString(indentStyle);
    }

    /// <summary>
    /// Writes the entity to the specified writer as a JSON value.
    /// </summary>
    /// <param name="entity">The entity to write.</param>
    /// <param name="writer">The writer to which to write this instance.</param>
    public static void WriteTo(BaseResourceEntityInfo entity, Utf8JsonWriter writer)
    {
        if (writer == null) return;
        var json = entity is null ? JsonValues.Null : (JsonObjectNode)entity;
        json.WriteTo(writer);
    }

    /// <summary>
    /// Gets the specific entity by identifer.
    /// </summary>
    /// <typeparam name="T">The type of the resource entity.</typeparam>
    /// <param name="entities">The entity collection to find the specific one.</param>
    /// <param name="id">The identifier of the entity.</param>
    /// <returns>The entity found; or null, if does not exist.</returns>
    public static T Get<T>(IEnumerable<T> entities, string id)
        where T : BaseResourceEntityInfo
    {
        foreach (var entity in entities)
        {
            if (entity.Id == id) return entity;
        }

        return null;
    }

    /// <summary>
    /// Gets the specific entity by identifer.
    /// </summary>
    /// <typeparam name="T">The type of the resource entity.</typeparam>
    /// <param name="entities">The entity collection to find the specific one.</param>
    /// <param name="id">The identifier of the entity.</param>
    /// <param name="supertype">The supertype to filter.</param>
    /// <returns>The entity found; or null, if does not exist.</returns>
    public static T Get<T>(IEnumerable<T> entities, string id, string supertype)
        where T : BaseResourceEntityInfo
    {
        foreach (var entity in entities)
        {
            if (entity.IsSameId(id, supertype)) return entity;
        }

        return null;
    }

    /// <summary>
    /// Sets the owner of the related resource entity.
    /// </summary>
    /// <typeparam name="TOwner">The type of the owner resource entity.</typeparam>
    /// <param name="entity">The entity with owner identifier.</param>
    /// <param name="resources">The entities to find the owner.</param>
    /// <returns>The owner entity found; or null, if does not exist.</returns>
    public static TOwner SetOwner<TOwner>(RelatedResourceEntityInfo<TOwner> entity, IList<BaseAccountEntityInfo> resources)
        where TOwner : BaseAccountEntityInfo
    {
        var id = entity?.OwnerId;
        if (string.IsNullOrWhiteSpace(id)) return null;
        foreach (var item in resources)
        {
            if (item is not TOwner owner || owner.Id != id) continue;
            entity.Owner = owner;
            return owner;
        }

        return null;
    }

    /// <summary>
    /// Sets the owner of the related resource entity.
    /// </summary>
    /// <typeparam name="TOwner">The type of the owner resource entity.</typeparam>
    /// <param name="entity">The entity with owner identifier.</param>
    /// <param name="owner">The entity to test if it is the owner.</param>
    /// <returns>The owner entity found; or null, if does not exist.</returns>
    public static bool SetOwner<TOwner>(RelatedResourceEntityInfo<TOwner> entity, TOwner owner)
        where TOwner : BaseAccountEntityInfo
    {
        var id = entity?.OwnerId;
        if (string.IsNullOrWhiteSpace(id) || owner.Id != id) return false;
        entity.Owner = owner;
        return true;
    }

    /// <summary>
    /// Sets the target of the related resource entity.
    /// </summary>
    /// <typeparam name="TOwner">The type of the owner resource entity.</typeparam>
    /// <typeparam name="TTarget">The type of the target resource entity.</typeparam>
    /// <param name="entity">The entity with target identifier.</param>
    /// <param name="resources">The entities to find the target.</param>
    /// <returns>The target entity found; or null, if does not exist.</returns>
    public static TTarget SetTarget<TOwner, TTarget>(RelatedResourceEntityInfo<TOwner, TTarget> entity, IList<BaseAccountEntityInfo> resources)
        where TOwner : BaseAccountEntityInfo
        where TTarget : BaseAccountEntityInfo
    {
        var id = entity?.TargetId;
        if (string.IsNullOrWhiteSpace(id)) return null;
        foreach (var item in resources)
        {
            if (item is not TTarget target || target.Id != id) continue;
            entity.Target = target;
            return target;
        }

        return null;
    }

    /// <summary>
    /// Sets the target of the related resource entity.
    /// </summary>
    /// <typeparam name="TOwner">The type of the owner resource entity.</typeparam>
    /// <typeparam name="TTarget">The type of the target resource entity.</typeparam>
    /// <param name="entity">The entity with target identifier.</param>
    /// <param name="target">The entity to test if it is the target.</param>
    /// <returns>The target entity found; or null, if does not exist.</returns>
    public static bool SetTarget<TOwner, TTarget>(RelatedResourceEntityInfo<TOwner, TTarget> entity, TTarget target)
        where TOwner : BaseAccountEntityInfo
        where TTarget : BaseAccountEntityInfo
    {
        var id = entity?.TargetId;
        if (string.IsNullOrWhiteSpace(id) || target.Id != id) return false;
        entity.Target = target;
        return true;
    }

    /// <summary>
    /// Creates a claim.
    /// </summary>
    /// <param name="type">The claim type.</param>
    /// <param name="value">The value of the claim.</param>
    /// <param name="issuer">The optional claim issuer.</param>
    /// <returns>The claim instance.</returns>
    public static Claim ToClaim(string type, string value, string issuer = null)
        => new(type, value, ClaimValueTypes.String, issuer);

    /// <summary>
    /// Creates a claim.
    /// </summary>
    /// <param name="type">The claim type.</param>
    /// <param name="value">The value of the claim.</param>
    /// <param name="issuer">The optional claim issuer.</param>
    /// <returns>The claim instance.</returns>
    public static Claim ToClaim(string type, int value, string issuer = null)
        => new(type, value.ToString(), ClaimValueTypes.Integer32, issuer);

    /// <summary>
    /// Creates a claim.
    /// </summary>
    /// <param name="type">The claim type.</param>
    /// <param name="value">The value of the claim.</param>
    /// <param name="onlyDate">true if only date; otherwise, false, date and time.</param>
    /// <param name="issuer">The optional claim issuer.</param>
    /// <returns>The claim instance.</returns>
    public static Claim ToClaim(string type, DateTime value, bool onlyDate, string issuer = null)
        => new(type, JsonStringNode.ToJson(value, true), onlyDate ? ClaimValueTypes.Date : ClaimValueTypes.DateTime, issuer);

    /// <summary>
    /// Tests if contains the specific item.
    /// </summary>
    /// <param name="source">The collection of attachment item to find.</param>
    /// <param name="item">The attachment item.</param>
    /// <returns>true if contains; otherwise, false.</returns>
    public static bool Contains(this IEnumerable<AttachmentLinkItem> source, AttachmentLinkItem item)
        => item is not null && source is not null && source.Contains(item);

    /// <summary>
    /// Tests if contains the specific item.
    /// </summary>
    /// <param name="source">The collection of attachment item to find.</param>
    /// <param name="link">The attachment link.</param>
    /// <returns>true if contains; otherwise, false.</returns>
    public static bool Contains(this IEnumerable<AttachmentLinkItem> source, Uri link)
        => link is not null && source is not null && source.Any(ele => ele?.Link == link);

    internal static bool IsEmptyId(string id)
    {
        if (string.IsNullOrWhiteSpace(id)) return true;
        return id.Replace("-", string.Empty) == Guid.Empty.ToString("N");
    }
}
