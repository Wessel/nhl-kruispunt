using UnityEngine;

public class BridgeSensor : Sensor
{
  private BridgeState state = BridgeState.Closed;

  public BridgeState GetState()
  {
    return state;
  }

  public void SetState(BridgeState state) {
    this.state = state;
    this.onStateChanged.Invoke();
    EventManager.Instance?.BridgeStateChanged.Invoke(state);
  }
}
