using System.Collections;
using UnityEngine;

public class MainController : MonoBehaviour
{
  private void Start()
  {
    Config.LoadConfig();
    StartCoroutine(InvokeAfterDelay());
  }

  private IEnumerator InvokeAfterDelay()
  {
    // Wait for one frame before invoking the events
    yield return null;
    EventManager.Instance.onStartClient.Invoke();
  }

  private void OnApplicationQuit()
  {
    if (EventManager.Instance != null)
    {
      EventManager.Instance.onStopClient.Invoke();
    }
  }
}
