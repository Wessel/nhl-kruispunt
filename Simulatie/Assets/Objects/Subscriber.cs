using UnityEngine;
using NetMQ;
using NetMQ.Sockets;
using System.Collections.Concurrent;
using System.Threading;
using System.Collections.Generic;

public class Subscriber : MonoBehaviour
{
  private Thread _listenerThread;
  private bool _isListening;
  private SubscriberSocket _subSocket;
  private readonly ConcurrentQueue<(string topic, string message)> _messageQueue = new();

  private void Start()
  {
    _isListening = true;
    _listenerThread = new Thread(Listen) { IsBackground = true };
    _listenerThread.Start();
  }

  private void Listen()
  {
    try
    {
      AsyncIO.ForceDotNet.Force();

      _subSocket = new SubscriberSocket();
      _subSocket.Options.ReceiveHighWatermark = 1000;
      _subSocket.Connect($"{Config.Instance.Method}://{Config.Instance.ListenIP}:{Config.Instance.ListenPort}");
      _subSocket.Subscribe("stoplichten");

      Debug.Log("Subscriber connected.");

      while (_isListening)
      {
        List<string> messageParts = new List<string>();
        if (_subSocket.TryReceiveMultipartStrings(ref messageParts) && messageParts.Count == 2)
        {
          _messageQueue.Enqueue((messageParts[0], messageParts[1]));
        }

        Thread.Sleep(5);
      }
    }
    catch (System.Exception ex)
    {
      Debug.LogError($"Subscriber thread exception: {ex}");
    }
  }

  private void Update()
  {
    while (_messageQueue.TryDequeue(out var msg))
    {
      if (msg.topic == "stoplichten")
      {
        EventManager.Instance?.OnTrafficLightUpdate?.Invoke(msg.message);
      }
    }
  }

  private void OnDestroy()
  {
    Shutdown();
  }

  private void Shutdown()
  {
    _isListening = false;

    if (_listenerThread != null)
    {
      if (!_listenerThread.Join(2000))
      {
        Debug.LogWarning("Listener thread did not stop in time.");
      }
      _listenerThread = null;
    }

    _subSocket?.Close();
    _subSocket = null;

    NetMQConfig.Cleanup();
    Debug.Log("Subscriber shutdown complete.");
  }
}
