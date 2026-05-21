using System.Text.Json.Serialization;

namespace backend.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum NotificationStatus
    {
        Unread = 0,
        Read = 1,
        Deleted = 2
    }
}
