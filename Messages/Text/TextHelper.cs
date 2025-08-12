using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Net.WebSockets;
using System.Security.Authentication;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Trivial.Users;
using Trivial.Web;

namespace Trivial.Text;

internal static class TextHelper
{
    public static bool IsCjkv(char c)
        => (c >= 0x3400 && c < 0xA000) || (c >= 0x20000 && c < 0x2EF00) || (c >= 0x30000 && c < 0x323B0);

    public static bool IsCjkv(string s, bool all)
    {
        if (s == null) return false;
        if (all)
        {
            foreach (var c in s)
            {
                if (!IsCjkv(c)) return false;
            }

            return true;
        }
        else
        {
            foreach (var c in s)
            {
                if (IsCjkv(c)) return true;
            }

            return false;
        }
    }

    public static bool IsNotWhiteSpace(string s)
        => !string.IsNullOrWhiteSpace(s);

    public static IBasicPublisherInfo ToPublisherInfo(JsonObjectNode json)
    {
        if (json == null) return null;
        var type = json.TryGetStringTrimmedValue("type", true);
        if (type == null) return json.Deserialize<PublisherBasicInfo>();
        return type.ToLowerInvariant() switch
        {
            "organization" or "org" => json.Deserialize<OrgAccountItemInfo>(),
            _ => json.Deserialize<PublisherBasicInfo>()
        };
    }

    public static string GetIfNotEmpty(string s, string defaultValue)
        => string.IsNullOrWhiteSpace(s) ? defaultValue : s;

    public static ExtendedChatMessageParameter ToParameter(object parameter)
        => parameter is ExtendedChatMessageParameter p ? p : new(parameter);

    public static JsonObjectNode ToJson(Exception ex, int maxInnerException = 10)
    {
        var json = new JsonObjectNode();
        json.SetValue("message", ex.Message);
        var inner = ex.InnerException;
        if (ex is AggregateException agg)
        {
            if (agg.InnerExceptions != null && agg.InnerExceptions.Count > 0)
            {
                var col = new JsonArrayNode();
                json.SetValue("col", col);
                foreach (var item in agg.InnerExceptions)
                {
                    if (ReferenceEquals(item, inner)) inner = null;
                    col.Add(ToJson(item, maxInnerException));
                }
            }
        }
        else if (ex is Net.FailedHttpException httpEx)
        {
            json.SetValue("kind", "http");
            if (httpEx.StatusCode.HasValue) json.SetValue("status", (int)httpEx.StatusCode.Value);
            json.SetValueIfNotEmpty("reason", httpEx.ReasonPhrase);
        }
        else if (ex is HttpRequestException reqEx)
        {
            json.SetValue("kind", "http");
#if NETCOREAPP
            if (reqEx.StatusCode.HasValue) json.SetValue("status", (int)reqEx.StatusCode.Value);
#endif
        }
        else if (ex is JsonException || ex is FormatException || ex is InvalidCastException)
        {
            json.SetValue("kind", "format");
        }
        else if (ex is UnauthorizedAccessException || ex is AuthenticationException)
        {
            json.SetValue("kind", "auth");
        }
        else if (ex is OutOfMemoryException)
        {
            json.SetValue("kind", "oom");
        }
        else if (ex is OperationCanceledException)
        {
            json.SetValue("kind", "abort");
        }
        else if (ex is WebSocketException socket)
        {
            json.SetValue("kind", "socket");
            json.SetValue("status", socket.WebSocketErrorCode.ToString());
        }

        var arr = new JsonArrayNode();
        while (inner != null && maxInnerException > 0)
        {
            arr.Add(ToJson(ex, 0));
            inner = inner.InnerException;
            maxInnerException--;
        }

        if (arr.Count > 0) json.SetValue("inner", arr);
        json.SetValue("type", ex.GetType()?.Name);
        return json;
    }

    public static void Add<T>(this List<BaseExtendedChatConversationCache<T>> list, ExtendedChatConversation conversation, T provider)
    {
        if (conversation?.Source is null) return;
        foreach (var item in list)
        {
            if (item is null) continue;
            if (ReferenceEquals(item.Provider, provider)) return;
        }

        list.Add(new(conversation, provider));
    }

    /// <summary>
    /// Registers a set of provider.
    /// </summary>
    /// <param name="list">The source collection.</param>
    /// <param name="providers">The providers to register.</param>
    /// <param name="conversationMaker">The handler to create conversation from the provider.</param>
    public static int AddRange<T>(this List<BaseExtendedChatConversationCache<T>> list, IEnumerable<T> providers, Func<T, ExtendedChatConversation> conversationMaker)
    {
        var i = 0;
        if (providers == null || conversationMaker is null) return i;
        foreach (var provider in providers)
        {
            Add(list, conversationMaker(provider), provider);
            i++;
        }

        return i;
    }

    /// <summary>
    /// Registers a set of provider.
    /// </summary>
    /// <param name="list">The source collection.</param>
    /// <param name="providers">The providers to register.</param>
    /// <param name="conversationMaker">The handler to create conversation from the provider.</param>
    public static int AddRange<T>(this List<BaseExtendedChatConversationCache<T>> list, ReadOnlySpan<T> providers, Func<T, ExtendedChatConversation> conversationMaker)
    {
        var i = 0;
        if (conversationMaker is null) return i;
        foreach (var provider in providers)
        {
            Add(list, conversationMaker(provider), provider);
            i++;
        }

        return i;
    }

    /// <summary>
    /// Removes a specific provider from the registry.
    /// </summary>
    /// <param name="list">The source collection.</param>
    /// <param name="provider">The provider to remove.</param>
    /// <returns>true if item is successfully removed; otherwise, false. This method also returns false if item was not found in the provider registry.</returns>
    public static bool Remove<T>(this List<BaseExtendedChatConversationCache<T>> list, T provider)
    {
        if (provider == null) return false;
        BaseExtendedChatConversationCache<T> cache = null;
        foreach (var item in list)
        {
            if (item is null || !ReferenceEquals(item.Provider, provider)) continue;
            cache = item;
            break;
        }

        if (cache == null) return false;
        return list.Remove(cache);
    }

    /// <summary>
    /// Removes a specific provider from the registry.
    /// </summary>
    /// <param name="list">The source collection.</param>
    /// <param name="provider">The provider to remove.</param>
    /// <returns>true if item is successfully removed; otherwise, false. This method also returns false if item was not found in the provider registry.</returns>
    public static bool Remove<T>(this List<BaseExtendedChatConversationCache<T>> list, string provider)
    {
        if (string.IsNullOrWhiteSpace(provider)) return false;
        BaseExtendedChatConversationCache<T> cache = null;
        foreach (var item in list)
        {
            if (item?.Conversation?.Id != provider) continue;
            cache = item;
            break;
        }

        if (cache == null) return false;
        return list.Remove(cache);
    }

    /// <summary>
    /// Adds a set of conversation to the cache.
    /// </summary>
    /// <param name="source">The source conversation list.</param>
    /// <param name="conversations">The conversations to add.</param>
    public static void Add(ObservableCollection<ExtendedChatConversation> source, IEnumerable<ExtendedChatConversation> conversations)
    {
        if (conversations == null) return;
        foreach (var conversation in conversations)
        {
            if (conversation?.Id == null || source.Contains(conversation)) continue;
            source.Add(conversation);
        }
    }

    /// <summary>
    /// Adds a conversation to the cache.
    /// </summary>
    /// <param name="source">The source conversation list.</param>
    /// <param name="conversation">The conversation to add.</param>
    public static void Add(ObservableCollection<ExtendedChatConversation> source, ExtendedChatConversation conversation)
    {
        if (conversation?.Id == null || source.Contains(conversation)) return;
        source.Add(conversation);
    }

    /// <summary>
    /// Removes a set of convsersation from the cache.
    /// </summary>
    /// <param name="source">The source conversation list.</param>
    /// <param name="conversations">The conversations to remove.</param>
    public static int Remove(ObservableCollection<ExtendedChatConversation> source, IEnumerable<ExtendedChatConversation> conversations)
    {
        var i = 0;
        if (conversations == null) return i;
        foreach (var conversation in conversations)
        {
            if (conversation == null) continue;
            if (source.Remove(conversation)) i++;
        }

        return i;
    }

    /// <summary>
    /// Removes a convsersation from the cache.
    /// </summary>
    /// <param name="source">The source conversation list.</param>
    /// <param name="conversation">The conversation to remove.</param>
    public static bool Remove(ObservableCollection<ExtendedChatConversation> source, ExtendedChatConversation conversation)
    {
        if (conversation == null) return false;
        return source.Remove(conversation);
    }

    /// <summary>
    /// Sorts a given conversation in the collection cache.
    /// </summary>
    /// <param name="source">The source conversation list.</param>
    /// <param name="conversation">The conversation item to sort its order in the collection cache.</param>
    public static void Sort(ObservableCollection<ExtendedChatConversation> source, ExtendedChatConversation conversation)
    {
        var last = conversation?.History?.LastOrDefault();
        if (last == null) return;
        var i = 0;
        try
        {
            for (; i < source.Count; i++)
            {
                var item = source[i];
                var testItem = item?.History?.LastOrDefault();
                if (testItem == null) continue;
                if (testItem.CreationTime < last.CreationTime) break;
                if (testItem.CreationTime == last.CreationTime && item == conversation) return;
            }

            var j = source.IndexOf(conversation);
            if (i == j) return;
            else if (j < 0) source.Insert(i, conversation);
            else source.Move(j, i);
        }
        catch (ArgumentException)
        {
        }
        catch (InvalidOperationException)
        {
        }
    }

    public static IBasicPublisherInfo ToPublisherInfo(JsonObjectNode json, string propertyKey)
        => ToPublisherInfo(json?.TryGetObjectValue(propertyKey));

#if NETFRAMEWORK
    public static bool StartsWith(this string s, char value)
        => s.StartsWith(value.ToString(), StringComparison.Ordinal);
#endif
}
