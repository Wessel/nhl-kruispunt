using UnityEngine;
using UnityEngine.Events;

public class EventManager: MonoBehaviour
{
  public static EventManager Instance { get; private set; }

  public UnityEvent onStartClient = new UnityEvent();
  public UnityEvent onStopClient = new UnityEvent();
  public UnityEvent onClientStarted = new UnityEvent();
  public UnityEvent onClientStopped = new UnityEvent();

  private void Awake()
  {
    if (Instance == null)
      Instance = this;
    else
      Destroy(gameObject);
  }
}
