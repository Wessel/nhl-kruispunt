using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

[JsonConverter(typeof(StringEnumConverter))]
public enum BridgeState
{
  [EnumMember(Value = "open")]
  Open,

  [EnumMember(Value = "onbekend")]
  Unknown,

  [EnumMember(Value = "dicht")]
  Closed
}
