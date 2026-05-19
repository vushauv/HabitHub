using System.Text.Json.Serialization;

namespace backend.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum EntryStatus
{
    Logged,
    Pending,
    Skipped
}