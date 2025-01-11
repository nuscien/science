using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Trivial.Net;
using Trivial.Text;

namespace Trivial.Users;

/// <summary>
/// The information of the user which is currently signed in.
/// </summary>
public class IdentityAccountInfo
{
    /// <summary>
    /// Gets or sets the unique identifier of the user.
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; }

    /// <summary>
    /// Gets or sets the family name of the user.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the nickname of the user.
    /// </summary>
    [JsonPropertyName("given_name")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string GivenName { get; set; }

    /// <summary>
    /// Gets or sets the email address of the user.
    /// </summary>
    [JsonPropertyName("email")]
    public string Email { get; set; }

    /// <summary>
    /// Gets or sets the the avatar image URL of the user.
    /// </summary>
    [JsonPropertyName("picture")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string AvatarUrl { get; set; }

    /// <summary>
    /// Gets or sets the client identifiers that the user has registered with.
    /// </summary>
    [JsonPropertyName("approved_clients")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<string> ApprovedClients { get; set; }

    /// <summary>
    /// Gets or sets a strings representing the account.
    /// These are used to filter the list of account options that the browser offers for the user to sign-in.
    /// </summary>
    [JsonPropertyName("login_hints")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<string> LoginHints { get; set; }
}

/// <summary>
/// The information of the user which is currently signed in.
/// </summary>
public class IdentityAccountListInfo
{
    /// <summary>
    /// Gets or sets the account list.
    /// </summary>
    [JsonPropertyName("accounts")]
    public List<IdentityAccountInfo> Accounts { get; set; }
}

/// <summary>
/// The request information to assert the signning in state of the account.
/// </summary>
public class IdentityAccountAssertionRequest
{
    /// <summary>
    /// Gets or sets the client identifier.
    /// </summary>
    [JsonPropertyName("client_id")]
    public string ClientId { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier of the user account to be signed in.
    /// </summary>
    [JsonPropertyName("account_id")]
    public string AccountId { get; set; }

    /// <summary>
    /// Gets or sets the request nonce, provided by the relying party.
    /// </summary>
    [JsonPropertyName("nonce")]
    public string Nonce { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the disclosure text is shown.
    /// The disclosure text is the information shown to the user (which can include the terms of service and privacy policy links, if provided) if the user is signed in to the identity provider but doesn't have an account specifically on the current relying party.
    /// </summary>
    [JsonPropertyName("disclosure_text_shown")]
    public bool IsDisclosureTextShown { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the authentication validation request has been issued as a result of auto-reauthentication, i.e. without user mediation.
    /// </summary>
    [JsonPropertyName("is_auto_selected")]
    public bool IsAutoSelected { get; set; }

    /// <summary>
    /// Converts to a query data.
    /// </summary>
    /// <returns>The query data instance.</returns>
    public virtual QueryData ToQueryData()
        => new()
        {
            { "client_id", ClientId },
            { "account_id", AccountId },
            { "nonce", Nonce },
            { "disclosure_text_shown", IsDisclosureTextShown },
            { "is_auto_selected", IsAutoSelected }
        };

    /// <summary>
    /// Converts to query data.
    /// </summary>
    /// <param name="info">The request info to convert.</param>
    /// <returns>An instance of the query data.</returns>
    public static explicit operator QueryData(IdentityAccountAssertionRequest info)
        => info?.ToQueryData();
}
