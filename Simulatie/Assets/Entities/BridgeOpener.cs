using UnityEngine;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class BridgeOpener : TrafficLight
{
  public override void SetLight(LightState newState)
  {
    BridgeState bridgeState = newState switch
    {
      LightState.Red => BridgeState.Closed,
      LightState.Green => BridgeState.Open,
      LightState.Orange => BridgeState.Unknown,
      _ => BridgeState.Unknown
    };
    EventManager.Instance.SetBridgeState.Invoke(bridgeState);
  }
}
