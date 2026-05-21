using System.Text.Json.Serialization;

namespace backend.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum NotificationType
    {
        System = 0,
        Reminder = 1
    }
}
