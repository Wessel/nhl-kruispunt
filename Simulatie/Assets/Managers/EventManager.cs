using UnityEngine;
using UnityEngine.Events;

public class EventManager : MonoBehaviour
{
  public static EventManager Instance { get; private set; }

  public UnityEvent<float> SendSimulationTime = new UnityEvent<float>();
  public UnityEvent<string, string> PublishMessage = new UnityEvent<string, string>();
  public UnityEvent<string> OnTrafficLightUpdate = new UnityEvent<string>();
  public UnityEvent<BridgeState> SetBridgeState = new UnityEvent<BridgeState>();
  public UnityEvent<SpawnMode> SetSpawnMode = new UnityEvent<SpawnMode>();
  public UnityEvent Reset = new UnityEvent();

  private void Awake()
  {
    if (Instance == null)
      Instance = this;
    else
      Destroy(gameObject);
  }
}
