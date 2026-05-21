using System.Text.Json.Serialization;

namespace backend.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum Unit
{
    Km,
    Hours, 
    Minutes, 
    Kg, 
    Cups, 
    Steps, 
    Pages
}