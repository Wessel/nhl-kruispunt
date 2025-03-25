using UnityEngine;
using NetMQ;
using NetMQ.Sockets;
using System.Collections.Concurrent;
using System.Threading;

public class Subscriber : MonoBehaviour
{
  private Thread _listenerThread;
  private bool _isListening;
  private readonly ConcurrentQueue<string> _messageQueue = new ConcurrentQueue<string>();
  private SubscriberSocket _subSocket;

  private void Start()
  {
    EventManager.Instance.onStartClient.AddListener(StartListening);
    EventManager.Instance.onStopClient.AddListener(StopListening);
  }

  private void StartListening()
  {
    if (_isListening) return;

    _isListening = true;
    _listenerThread = new Thread(ListenerWork) { IsBackground = true };
    _listenerThread.Start();
    EventManager.Instance.onClientStarted.Invoke();
  }

  private void StopListening()
  {
    _isListening = false;
    _listenerThread?.Join();
    _listenerThread = null;

    _subSocket?.Close();
    _subSocket = null;

    NetMQConfig.Cleanup();
    EventManager.Instance.onClientStopped.Invoke();
    Debug.Log("Subscriber stopped.");
  }

  private void ListenerWork()
  {
    AsyncIO.ForceDotNet.Force();
    _subSocket = new SubscriberSocket();
    _subSocket.Options.ReceiveHighWatermark = 1000;
    _subSocket.Connect($"{Config.Instance.Method}://{Config.Instance.ListenIP}:{Config.Instance.ListenPort}");
    _subSocket.SubscribeToAnyTopic();
    Debug.Log("Subscriber connected.");

    while (_isListening)
    {
      if (_subSocket.TryReceiveFrameString(out var message))
        _messageQueue.Enqueue(message);
    }
  }

	private void Update()
	{
		while (!_messageQueue.IsEmpty)
		{
			if (_messageQueue.TryDequeue(out var topic) && _messageQueue.TryDequeue(out var message))
				Debug.Log($"Received Topic: {topic}, Message: {message}");
		}
	}
}
