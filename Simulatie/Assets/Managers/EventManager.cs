using UnityEngine;
using UnityEngine.Events;

public class EventManager: MonoBehaviour
{
  public static EventManager Instance { get; private set; }

  public UnityEvent<float> SendSimulationTime = new UnityEvent<float>();

  private void Awake()
  {
    if (Instance == null)
      Instance = this;
    else
      Destroy(gameObject);
  }
}
