using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

[JsonConverter(typeof(StringEnumConverter))]
public enum LightState
{
  [EnumMember(Value = "rood")]
  Red,

  [EnumMember(Value = "oranje")]
  Orange,

  [EnumMember(Value = "groen")]
  Green
}
