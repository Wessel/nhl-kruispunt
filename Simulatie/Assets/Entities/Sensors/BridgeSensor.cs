using UnityEngine;

public class BridgeSensor : Sensor
{
  private BridgeState state = BridgeState.Closed;

  public BridgeState GetState()
  {
    return state;
  }
}
