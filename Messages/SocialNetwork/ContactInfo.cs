using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Principal;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

using Trivial.Collection;
using Trivial.Data;
using Trivial.Reflection;
using Trivial.Text;
using Trivial.Users;

namespace Trivial.SocialNetwork;

/// <summary>
/// The roles targeted for the contact information.
/// </summary>
public enum ContactInfoRoles : byte
{
    /// <summary>
    /// Personal prime information.
    /// </summary>
    Person = 0,

    /// <summary>
    /// For home.
    /// </summary>
    Home = 1,

    /// <summary>
    /// For business (including company and indivial commercial entity).
    /// </summary>
    Business = 2,

    /// <summary>
    /// For school.
    /// </summary>
    School = 3,

    /// <summary>
    /// For other kinds of organization.
    /// </summary>
    Organization = 4,

    /// <summary>
    /// Used for club joined in.
    /// </summary>
    Club = 5,

    /// <summary>
    /// Virtual information (e.g. online network only).
    /// </summary>
    Virtual = 6,

    /// <summary>
    /// Others.
    /// </summary>
    Other = 7
}

/// <summary>
/// The phone number types.
/// </summary>
public enum PhoneNumberTypes : byte
{
    /// <summary>
    /// Mobile phone.
    /// </summary>
    Mobile = 0,

    /// <summary>
    /// Landline telephone.
    /// </summary>
    Telephone = 1,

    /// <summary>
    /// Private satellite phone.
    /// </summary>
    Satellite = 3,

    /// <summary>
    /// Fax.
    /// </summary>
    Fax = 4,

    /// <summary>
    /// Internet phone used by app.
    /// </summary>
    Internet = 5,

    /// <summary>
    /// Built-in call on vehicle, helicopter or yacht.
    /// </summary>
    Vehicle = 6,

    /// <summary>
    /// IoT.
    /// </summary>
    IoT = 7,

    /// <summary>
    /// The meeting room phone.
    /// </summary>
    Meeting = 8,

    /// <summary>
    /// Public satellite phone on ship.
    /// </summary>
    Ship = 9,

    /// <summary>
    /// Internal phone with short number.
    /// </summary>
    Internal = 11,

    /// <summary>
    /// Voice record or video record only.
    /// </summary>
    Record = 12,

    /// <summary>
    /// Virtual phone.
    /// </summary>
    Virtual = 13,

    /// <summary>
    /// A bot or auto-respond service.
    /// </summary>
    Bot = 14,

    /// <summary>
    /// Others.
    /// </summary>
    Other = 31
}

/// <summary>
/// The address information.
/// </summary>
public class AddressInfo : BaseObservableProperties
{
    /// <summary>
    /// Gets or sets the country or region.
    /// </summary>
    [JsonPropertyName("region")]
    public string Region
    {
        get => GetCurrentProperty<string>();
        set => SetCurrentProperty(value);
    }

    /// <summary>
    /// Gets or sets the 1st level region, e.g. province, and others levels if the city is not the 2nd.
    /// </summary>
    [JsonPropertyName("province")]
    public string Province
    {
        get => GetCurrentProperty<string>();
        set => SetCurrentProperty(value);
    }

    /// <summary>
    /// Gets or sets the city name.
    /// </summary>
    [JsonPropertyName("locality")]
    public string Locality
    {
        get => GetCurrentProperty<string>();
        set => SetCurrentProperty(value);
    }

    /// <summary>
    /// Gets or sets the street address.
    /// </summary>
    [JsonPropertyName("street")]
    public string Street
    {
        get => GetCurrentProperty<string>();
        set => SetCurrentProperty(value);
    }

    /// <summary>
    /// Gets or sets the post office box.
    /// </summary>
    [JsonPropertyName("post")]
    public string PostOfficeBox
    {
        get => GetCurrentProperty<string>();
        set => SetCurrentProperty(value);
    }

    /// <summary>
    /// Gets or sets the extended address.
    /// </summary>
    [JsonPropertyName("ext")]
    public string ExtendedAddress
    {
        get => GetCurrentProperty<string>();
        set => SetCurrentProperty(value);
    }

    /// <summary>
    /// Gets or sets the postal code.
    /// </summary>
    [JsonPropertyName("zipcode")]
    public string ZipCode
    {
        get => GetCurrentProperty<string>();
        set => SetCurrentProperty(value);
    }

    /// <summary>
    /// Gets or sets the remark.
    /// </summary>
    [JsonPropertyName("remark")]
    public string Remark
    {
        get => GetCurrentProperty<string>();
        set => SetCurrentProperty(value);
    }
}

/// <summary>
/// Social account information.
/// </summary>
public class SocialAccountInfo : BaseObservableProperties
{
    /// <summary>
    /// Gets or sets the social account provider name.
    /// </summary>
    [JsonPropertyName("provider")]
    public string Provider
    {
        get => GetCurrentProperty<string>();
        set => SetCurrentProperty(value);
    }

    /// <summary>
    /// Gets or sets the account name.
    /// </summary>
    [JsonPropertyName("account")]
    public string Account
    {
        get => GetCurrentProperty<string>();
        set => SetCurrentProperty(value);
    }

    /// <summary>
    /// Gets or sets the protocal link used to contact with.
    /// </summary>
    [JsonPropertyName("url")]
    public string Link
    {
        get => GetCurrentProperty<string>();
        set => SetCurrentProperty(value);
    }

    /// <summary>
    /// Gets or sets the optional description.
    /// </summary>
    [JsonPropertyName("desc")]
    public string Description
    {
        get => GetCurrentProperty<string>();
        set => SetCurrentProperty(value);
    }

    /// <summary>
    /// Gets or sets the remark.
    /// </summary>
    [JsonPropertyName("remark")]
    public string Remark
    {
        get => GetCurrentProperty<string>();
        set => SetCurrentProperty(value);
    }
}

/// <summary>
/// The organiztion information and title of the person.
/// </summary>
public class OrganizationRelationshipInfo : BaseObservableProperties
{
    /// <summary>
    /// Gets or sets the name of company, school or other kind of the organization.
    /// </summary>
    [JsonPropertyName("org")]
    public string Organization
    {
        get => GetCurrentProperty<string>();
        set => SetCurrentProperty(value);
    }

    /// <summary>
    /// Gets or sets the branch or department name.
    /// </summary>
    [JsonPropertyName("dept")]
    public string Department
    {
        get => GetCurrentProperty<string>();
        set => SetCurrentProperty(value);
    }

    /// <summary>
    /// Gets or sets the title.
    /// </summary>
    [JsonPropertyName("title")]
    public string Title
    {
        get => GetCurrentProperty<string>();
        set => SetCurrentProperty(value);
    }

    /// <summary>
    /// Gets or sets the full titles.
    /// </summary>
    [JsonPropertyName("position")]
    public ObservableCollection<string> FullTitles
    {
        get => GetCurrentProperty<ObservableCollection<string>>();
        set => SetCurrentProperty(value);
    }

    /// <summary>
    /// Gets or sets the optional alias.
    /// </summary>
    [JsonPropertyName("alias")]
    public string Alias
    {
        get => GetCurrentProperty<string>();
        set => SetCurrentProperty(value);
    }

    /// <summary>
    /// Gets or sets the official website.
    /// </summary>
    [JsonPropertyName("url")]
    public string OfficialWebsite
    {
        get => GetCurrentProperty<string>();
        set => SetCurrentProperty(value);
    }
}

/// <summary>
/// The phone number information.
/// </summary>
public class PhoneNumber : BaseObservableProperties
{
    /// <summary>
    /// Gets or sets the phone number type.
    /// </summary>
    [JsonPropertyName("type")]
    public PhoneNumberTypes Type
    {
        get => GetCurrentProperty<PhoneNumberTypes>();
        set => SetCurrentProperty(value);
    }

    /// <summary>
    /// Gets or sets the phone number.
    /// </summary>
    [JsonPropertyName("number")]
    public string Number
    {
        get => GetCurrentProperty<string>();
        set => SetCurrentProperty(value);
    }

    /// <summary>
    /// Gets or sets the optional title of the contact way.
    /// </summary>
    [JsonPropertyName("title")]
    public string Title
    {
        get => GetCurrentProperty<string>();
        set => SetCurrentProperty(value);
    }

    /// <summary>
    /// Gets or sets the remark.
    /// </summary>
    [JsonPropertyName("remark")]
    public string Remark
    {
        get => GetCurrentProperty<string>();
        set => SetCurrentProperty(value);
    }
}

/// <summary>
/// The contact information details.
/// </summary>
public class ContactInfo : BaseObservableProperties
{
    /// <summary>
    /// Gets or sets the contact information role.
    /// </summary>
    [JsonPropertyName("role")]
    public ContactInfoRoles Role
    {
        get => GetCurrentProperty<ContactInfoRoles>();
        set => SetCurrentProperty(value);
    }

    /// <summary>
    /// Gets or sets the organization relationship.
    /// </summary>
    [JsonPropertyName("org")]
    public OrganizationRelationshipInfo OrganizationRelationship
    {
        get => GetCurrentProperty<OrganizationRelationshipInfo>();
        set => SetCurrentProperty(value);
    }

    /// <summary>
    /// Gets or sets the phone number.
    /// </summary>
    [JsonPropertyName("phone")]
    public ObservableCollection<PhoneNumber> Phones
    {
        get => GetCurrentProperty<ObservableCollection<PhoneNumber>>();
        set => SetCurrentProperty(value);
    }

    /// <summary>
    /// Gets or sets the email address.
    /// </summary>
    [JsonPropertyName("email")]
    [JsonConverter(typeof(JsonStringListConverter))]
    public ObservableCollection<string> Emails
    {
        get => GetCurrentProperty<ObservableCollection<string>>();
        set => SetCurrentProperty(value);
    }

    /// <summary>
    /// Gets the primary email.
    /// </summary>
    [JsonIgnore]
    public string PrimaryEmail => Emails?.FirstOrDefault();

    /// <summary>
    /// Gets or sets the address information.
    /// </summary>
    [JsonPropertyName("addr")]
    public ObservableCollection<AddressInfo> Addresses
    {
        get => GetCurrentProperty<ObservableCollection<AddressInfo>>();
        set => SetCurrentProperty(value);
    }

    /// <summary>
    /// Gets the primary email.
    /// </summary>
    [JsonIgnore]
    public AddressInfo PrimaryAddress => Addresses?.FirstOrDefault();

    /// <summary>
    /// Gets or sets the social account information.
    /// </summary>
    [JsonPropertyName("sns")]
    public ObservableCollection<SocialAccountInfo> SocialAccounts
    {
        get => GetCurrentProperty<ObservableCollection<SocialAccountInfo>>();
        set => SetCurrentProperty(value);
    }

    /// <summary>
    /// Gets the primary social account information.
    /// </summary>
    [JsonIgnore]
    public SocialAccountInfo PrimarySocialAccount => SocialAccounts?.FirstOrDefault();

    /// <summary>
    /// Gets or sets the remark.
    /// </summary>
    [JsonPropertyName("remark")]
    public string Remark
    {
        get => GetCurrentProperty<string>();
        set => SetCurrentProperty(value);
    }

    /// <summary>
    /// Adds a phone number.
    /// </summary>
    /// <param name="value">The value.</param>
    public void AddPhone(PhoneNumber value)
    {
        var col = Phones;
        if (col == null)
        {
            if (Phones != null) col = Phones;
            col = new();
            Phones = col;
        }

        col.Add(value);
    }

    /// <summary>
    /// Adds a phone number.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="type">The phone number type.</param>
    public void AddPhone(string value, PhoneNumberTypes type = PhoneNumberTypes.Telephone)
        => AddPhone(new()
        {
            Number = value,
            Type = type
        });

    /// <summary>
    /// Adds an email address.
    /// </summary>
    /// <param name="value">The value.</param>
    public void AddEmail(string value)
    {
        var col = Emails;
        if (col == null)
        {
            if (Emails != null) col = Emails;
            col = new();
            Emails = col;
        }

        col.Add(value);
    }

    /// <summary>
    /// Adds an address information.
    /// </summary>
    /// <param name="value">The value.</param>
    public void AddAddress(AddressInfo value)
    {
        var col = Addresses;
        if (col == null)
        {
            if (Addresses != null) col = Addresses;
            col = new();
            Addresses = col;
        }

        col.Add(value);
    }

    /// <summary>
    /// Adds a social account information.
    /// </summary>
    /// <param name="value">The value.</param>
    public void AddSocialAccount(SocialAccountInfo value)
    {
        var col = SocialAccounts;
        if (col == null)
        {
            if (SocialAccounts != null) col = SocialAccounts;
            col = new();
            SocialAccounts = col;
        }

        col.Add(value);
    }

    /// <summary>
    /// Returns a string that represents the current contact info.
    /// </summary>
    /// <returns>A string that represents the current contact info.</returns>
    public override string ToString()
    {
        var sb = new StringBuilder();
        AppendTo(sb);
        return sb.ToString();
    }

    internal void AppendTo(StringBuilder sb)
    {
        var phone = Phones;
        if (phone != null)
        {
            foreach (var item in phone)
            {
                var number = item?.Number;
                if (string.IsNullOrWhiteSpace(number)) continue;
                WriteKey(sb, "TEL", item.Type switch
                {
                    PhoneNumberTypes.Meeting or PhoneNumberTypes.Telephone or PhoneNumberTypes.Satellite => "voice",
                    PhoneNumberTypes.Mobile => "cell,voice,video",
                    PhoneNumberTypes.Fax => "fax",
                    PhoneNumberTypes.Vehicle => "car;voice",
                    PhoneNumberTypes.Internal => "voice,video,msg",
                    PhoneNumberTypes.Record => "msg",
                    _ => null
                });
                sb.AppendLine(number);
            }
        }

        var email = Emails;
        if (email != null)
        {
            foreach (var item in email)
            {
                if (string.IsNullOrWhiteSpace(item)) continue;
                WriteKey(sb, "EMAIL");
                sb.AppendLine(item);
            }
        }

        var addr = PrimaryAddress;
        if (addr != null)
        {
            WriteKey(sb, "ADR");
            sb.AppendFormat("{0};{1};{2};{3};{4};{5};{6}", addr.PostOfficeBox, addr.ExtendedAddress, addr.Street, addr.Locality, addr.Province, addr.ZipCode, addr.Region);
            sb.AppendLine();
        }

        var org = OrganizationRelationship;
        if (org != null)
        {
            if (!string.IsNullOrWhiteSpace(org.Organization))
            {
                sb.Append("ORG:");
                sb.Append(org.Organization);
                if (!string.IsNullOrWhiteSpace(org.Department))
                {
                    sb.Append(';');
                    sb.AppendLine(org.Department);
                }
                else
                {
                    sb.AppendLine();
                }
            }

            if (!string.IsNullOrWhiteSpace(org.Title))
            {
                sb.Append("TITLE:");
                sb.AppendLine(org.Title);
            }
        }
    }

    private void WriteKey(StringBuilder sb, string key, string kind = null)
    {
        sb.Append(key);
        var role = Role switch
        {
            ContactInfoRoles.Home => "home",
            ContactInfoRoles.Business or ContactInfoRoles.Organization => "work",
            _ => null,
        };
        if (role != null)
        {
            sb.Append(";TYPE=");
            sb.Append(role);
        }

        if (!string.IsNullOrWhiteSpace(kind))
        {
            if (role == null) sb.Append(";TYPE=");
            else sb.Append(',');
            sb.Append(kind);
        }

        sb.Append(':');
    }
}

/// <summary>
/// The moniker information for the contact.
/// </summary>
public class UserMonikerInfo : BaseObservableProperties
{
    /// <summary>
    /// Gets or sets the display name, nickname or moniker template.
    /// </summary>
    [JsonPropertyName("display")]
    public string DisplayName
    {
        get => GetCurrentProperty<string>();
        set => SetCurrentProperty(value);
    }

    /// <summary>
    /// Gets or sets the family name.
    /// </summary>
    [JsonPropertyName("surname")]
    public string Surname
    {
        get => GetCurrentProperty<string>();
        set => SetCurrentProperty(value);
    }

    /// <summary>
    /// Gets or sets the middle name.
    /// </summary>
    [JsonPropertyName("middlename")]
    public string MiddleName
    {
        get => GetCurrentProperty<string>();
        set => SetCurrentProperty(value);
    }

    /// <summary>
    /// Gets or sets the given name.
    /// </summary>
    [JsonPropertyName("givenname")]
    public string GivenName
    {
        get => GetCurrentProperty<string>();
        set => SetCurrentProperty(value);
    }

    /// <summary>
    /// Gets or sets the ordinal number.
    /// </summary>
    [JsonPropertyName("ordinal")]
    public string Ordinal
    {
        get => GetCurrentProperty<string>();
        set => SetCurrentProperty(value);
    }

    /// <summary>
    /// Gets or sets the prefix.
    /// </summary>
    [JsonPropertyName("prefix")]
    public string Prefix
    {
        get => GetCurrentProperty<string>();
        set => SetCurrentProperty(value);
    }

    /// <summary>
    /// Gets or sets the suffix.
    /// </summary>
    [JsonPropertyName("suffix")]
    public string Suffix
    {
        get => GetCurrentProperty<string>();
        set => SetCurrentProperty(value);
    }
}

/// <summary>
/// The dates used for the contact.
/// </summary>
public class ContactDatesInfo : BaseObservableProperties
{
    /// <summary>
    /// Gets or sets the birthday.
    /// </summary>
    [JsonPropertyName("birthday")]
    public DateTime? Birthday
    {
        get => GetCurrentProperty<DateTime?>();
        set => SetCurrentProperty(value);
    }

    /// <summary>
    /// Gets or sets the memorial day.
    /// </summary>
    [JsonPropertyName("anniversary")]
    public DateTime? Anniversary
    {
        get => GetCurrentProperty<DateTime?>();
        set => SetCurrentProperty(value);
    }

    /// <summary>
    /// Gets or sets the other kind of day.
    /// </summary>
    [JsonPropertyName("other1")]
    public DateTime? Other1
    {
        get => GetCurrentProperty<DateTime?>();
        set => SetCurrentProperty(value);
    }

    /// <summary>
    /// Gets or sets the other kind of day.
    /// </summary>
    [JsonPropertyName("other2")]
    public DateTime? Other2
    {
        get => GetCurrentProperty<DateTime?>();
        set => SetCurrentProperty(value);
    }

    /// <summary>
    /// Gets or sets the other kind of day.
    /// </summary>
    [JsonPropertyName("other3")]
    public DateTime? Other3
    {
        get => GetCurrentProperty<DateTime?>();
        set => SetCurrentProperty(value);
    }
}

/// <summary>
/// The contact model.
/// </summary>
public class ContactModel : BaseObservableProperties
{
    /// <summary>
    /// Gets or sets the name.
    /// </summary>
    [JsonPropertyName("name")]
    public UserMonikerInfo Name
    {
        get => GetCurrentProperty<UserMonikerInfo>();
        set => SetCurrentProperty(value);
    }

    /// <summary>
    /// Gets or sets the gender.
    /// </summary>
    [JsonPropertyName("gender")]
    public Genders Gender
    {
        get => GetCurrentProperty<Genders>();
        set => SetCurrentProperty(value);
    }

    /// <summary>
    /// Gets or sets the URL of avatar.
    /// </summary>
    [JsonPropertyName("avatar")]
    public string AvatarUrl
    {
        get => GetCurrentProperty<string>();
        set => SetCurrentProperty(value);
    }

    /// <summary>
    /// Gets the display name.
    /// </summary>
    [JsonIgnore]
    public string DisplayName
    {
        get
        {
            var name = Name;
            if (name == null) return null;
            var template = name.DisplayName?.Trim();
            var surname = name.Surname;
            var givenName = name.GivenName;
            if (!string.IsNullOrWhiteSpace(template))
            {
                if (!template.StartsWith("#")) return template;
                return template.Trim('#').Trim()
                    .Replace("{surname}", surname)
                    .Replace("{Surname}", surname)
                    .Replace("{givenname}", givenName)
                    .Replace("{givenName}", givenName)
                    .Replace("{GivenName}", givenName)
                    .Replace("{middlename}", name.MiddleName)
                    .Replace("{middleName}", name.MiddleName)
                    .Replace("{MiddleName}", name.MiddleName)
                    .Replace("{ordinal}", name.Ordinal)
                    .Replace("{Ordinal}", name.Ordinal)
                    .Replace("{prefix}", name.Prefix)
                    .Replace("{Prefix}", name.Prefix)
                    .Replace("{suffix}", name.Suffix)
                    .Replace("{Suffix}", name.Suffix);
            }

            if (string.IsNullOrWhiteSpace(surname))
            {
                if (!string.IsNullOrWhiteSpace(givenName) && TextHelper.IsCjkv(givenName, true))
                    return string.Concat(name.Prefix, name.MiddleName, givenName, name.Ordinal, name.Suffix);
            }
            else
            {
                if (TextHelper.IsCjkv(surname, true))
                {
                    if (surname.Length < 3 && string.IsNullOrWhiteSpace(name.MiddleName))
                        return string.Concat(name.Prefix, surname, givenName, name.Ordinal, name.Suffix);
                    var list2 = new List<string>
                    {
                        surname
                    };
                    if (!string.IsNullOrWhiteSpace(name.MiddleName)) list2.Add(name.MiddleName);
                    if (!string.IsNullOrWhiteSpace(givenName)) list2.Add(givenName);
                    return string.Concat(name.Prefix, string.Join("·", list2), name.Ordinal, name.Suffix);
                }
            }

            var list = new List<string>
            {
                name.Prefix,
                givenName,
                name.MiddleName,
                surname,
                name.Ordinal,
                name.Suffix
            };
            return string.Join(" ", list.Where(TextHelper.IsNotWhiteSpace));
        }
    }

    /// <summary>
    /// Gets or sets the contact information.
    /// </summary>
    [JsonPropertyName("contacts")]
    public ObservableCollection<ContactInfo> ContactMethods
    {
        get => GetCurrentProperty<ObservableCollection<ContactInfo>>();
        set => SetCurrentProperty(value);
    }

    /// <summary>
    /// Gets or sets the dates.
    /// </summary>
    [JsonPropertyName("dates")]
    public ContactDatesInfo Dates
    {
        get => GetCurrentProperty<ContactDatesInfo>();
        set => SetCurrentProperty(value);
    }

    /// <summary>
    /// Gets or sets the bio.
    /// </summary>
    [JsonPropertyName("bio")]
    public string Bio
    {
        get => GetCurrentProperty<string>();
        set => SetCurrentProperty(value);
    }

    /// <summary>
    /// Adds contact method information.
    /// </summary>
    /// <param name="value">The information.</param>
    public void AddContactMethod(ContactInfo value)
    {
        var col = ContactMethods;
        if (col == null)
        {
            if (ContactMethods != null) col = ContactMethods;
            col = new();
            ContactMethods = col;
        }

        col.Add(value);
    }

    /// <summary>
    /// Returns a string that represents the current contact info.
    /// </summary>
    /// <returns>A string that represents the current contact info.</returns>
    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.AppendLine("BEGIN:VCARD");
        sb.AppendLine("VERSION:3.0");
        var name = Name;
        if (name != null)
        {
            sb.Append("FN:");
            sb.AppendLine(DisplayName);
            sb.Append("N:");
            sb.AppendLine(string.Join(";", new List<string>
            {
                name.Prefix, name.GivenName, name.MiddleName, name.Surname, name.Ordinal, name.Suffix
            }.Where(ele => !string.IsNullOrWhiteSpace(ele))));
        }

        if (!string.IsNullOrWhiteSpace(AvatarUrl))
        {
            sb.Append("PHOTO;VALUE=uri:");
            sb.AppendLine(AvatarUrl);
        }

        var contact = ContactMethods;
        if (contact != null)
        {
            foreach (var item in contact)
            {
                if (item is null) continue;
                item.AppendTo(sb);
            }
        }

        var date = Dates?.Birthday;
        if (date.HasValue)
        {
            sb.Append("BDAY:");
            sb.AppendLine(date.Value.ToString("s"));
        }

        switch (Gender)
        {
            case Genders.Male:
                sb.AppendLine("X-WAB-GENDER:1");
                break;
            case Genders.Female:
                sb.AppendLine("X-WAB-GENDER:2");
                break;
        }

        sb.AppendLine("END:VCARD");
        return sb.ToString();
    }
}
