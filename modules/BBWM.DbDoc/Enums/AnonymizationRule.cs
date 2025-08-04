using BBWM.Core.Web.JsonConverters;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace BBWM.DbDoc.Enums;

[JsonConverter(typeof(JsonStringEnumCamelCaseConverter))]
public enum AnonymizationRule
{
    [EnumMember(Value = "DATE")] Date,
    [EnumMember(Value = "ELVEN_NAME")] ElvenName,
    [EnumMember(Value = "EMAIL_ADDRESS")] EmailAddress,
    [EnumMember(Value = "IBAN")] Iban,
    [EnumMember(Value = "RANDOM_CHARACTERS")] RandomCharacters,
    [EnumMember(Value = "RANDOM_DIGITS")] RandomDigits,
    [EnumMember(Value = "ROMAN_NAME")] RomanName,
    [EnumMember(Value = "STRING")] String,
    [EnumMember(Value = "UUID")] Uuid
}
