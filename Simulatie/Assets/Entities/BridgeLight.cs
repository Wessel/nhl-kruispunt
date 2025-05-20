using UnityEngine;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class BridgeLight : TrafficLight
{
  public override void SetLight(LightState newState)
  {
    Debug.Log($"BridgeLight state changed to {newState}");
    BridgeState bridgeState = newState switch
    {
      LightState.Red => BridgeState.Closed,
      LightState.Green => BridgeState.Open,
      LightState.Orange => BridgeState.Unknown,
      _ => BridgeState.Unknown
    };
    EventManager.Instance?.SetBridgeState.Invoke(bridgeState);
  }
}
