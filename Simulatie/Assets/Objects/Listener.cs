using NetMQ.Sockets;
using NetMQ;
using System.Collections.Concurrent;
using System.Threading;
using System;
using UnityEngine;

public class Listener
{
  private Thread _listenerThread;
  private readonly string _method;
  private readonly string _host;
  private readonly int _port;
  private readonly Action<string> _messageCallback;
  private bool _isListening;
  private readonly ConcurrentQueue<string> _messageQueue = new ConcurrentQueue<string>();

  public Listener(string host, int port, string method, Action<string> messageCallback)
  {
    _host = host;
    _port = port;
    _method = method;
    _messageCallback = messageCallback;
  }

  public void Start()
  {
    if (_isListening) return;

    _isListening = true;
    _listenerThread = new Thread(ListenerWork);
    _listenerThread.Start();
    EventManager.Instance.onClientStarted.Invoke();
  }

  public void Stop()
  {
    _isListening = false;
    _listenerThread?.Join();
    _listenerThread = null;
    EventManager.Instance.onClientStopped.Invoke();
    Debug.Log("Listener stopped.");
  }

  private void ListenerWork()
  {
    AsyncIO.ForceDotNet.Force();
    using (var subSocket = new SubscriberSocket())
    {
      subSocket.Options.ReceiveHighWatermark = 1000;
      subSocket.Connect($"{_method}://{_host}:{_port}");
      subSocket.SubscribeToAnyTopic();
      Debug.Log($"Listener connected to {_method}://{_host}:{_port}");

      while (_isListening)
      {
        if (subSocket.TryReceiveFrameString(out var message))
          _messageQueue.Enqueue(message);
      }
      subSocket.Close();
    }
    NetMQConfig.Cleanup();
  }

  public void DigestMessage()
  {
    while (!_messageQueue.IsEmpty)
    {
      if (_messageQueue.TryDequeue(out var message))
        _messageCallback(message);
      else
        break;
    }
  }
}
