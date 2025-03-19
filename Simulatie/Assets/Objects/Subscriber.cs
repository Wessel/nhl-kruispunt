using UnityEngine;

public class Subscriber: MonoBehaviour
{
  private Listener _listener;

  private void Start()
  {
    _listener = new Listener(Config.Instance.IP, Config.Instance.ListenPort, Config.Instance.Method, HandleMessage);
    EventManager.Instance.onStartClient.AddListener(_listener.Start);
    EventManager.Instance.onStopClient.AddListener(_listener.Stop);
  }

  private void Update()
  {
    _listener?.DigestMessage();
  }

  private void HandleMessage(string message)
  {
    Debug.Log($"Received: {message}");
  }
}
