using UnityEngine;
using NetMQ;
using NetMQ.Sockets;
using System.Collections.Concurrent;
using System.Threading;
using System;
using UnityEditor;

public class Publisher : MonoBehaviour
{
  private Thread _publisherThread;
  private PublisherSocket _pubSocket;
  private CancellationTokenSource _cancellationTokenSource;
  private bool _isSocketInitialized;
  private ConcurrentQueue<(string topic, string message)> _messageQueue = new();
  private readonly object _socketLock = new();

  private void Start()
  {
    EventManager.Instance.SendSimulationTime.AddListener(SendSimulationTime);
    EventManager.Instance.PublishMessage.AddListener(PublishMessage);

    _cancellationTokenSource = new CancellationTokenSource();
    _publisherThread = new Thread(() => PublisherWork(_cancellationTokenSource.Token))
    {
      IsBackground = true
    };
    _publisherThread.Start();
  }

  private void OnDestroy()
  {
    Shutdown();
  }

  private void PublisherWork(CancellationToken token)
  {
    try
    {
      AsyncIO.ForceDotNet.Force();

      lock (_socketLock)
      {
        _pubSocket = new PublisherSocket();
        _pubSocket.Options.SendHighWatermark = 1000;
        _pubSocket.Bind($"{ZeroMQConfig.Instance.Method}://{ZeroMQConfig.Instance.PublishIP}:{ZeroMQConfig.Instance.PublishPort}");
        _isSocketInitialized = true;
      }

      Debug.Log("Publisher started.");

      while (!token.IsCancellationRequested)
      {
        while (_messageQueue.TryDequeue(out var msg))
        {
          lock (_socketLock)
          {
            if (_pubSocket != null && _isSocketInitialized)
            {
              _pubSocket.SendMoreFrame(msg.topic).SendFrame(msg.message);
            }
          }
        }
        Thread.Sleep(10);
      }
    }
    catch (Exception ex)
    {
      Debug.LogError($"Publisher thread exception: {ex}");
    }
    finally
    {
      lock (_socketLock)
      {
        _pubSocket?.Close();
        _pubSocket?.Dispose();
        _pubSocket = null;
        _isSocketInitialized = false;
      }

      NetMQConfig.Cleanup();
    }
  }
  private void Shutdown()
  {
    if (_cancellationTokenSource != null)
    {
      _cancellationTokenSource.Cancel();
    }
    Debug.Log("Publisher shutdown.");
  }

  private void PublishMessage(string topic, string message)
  {
    if (_isSocketInitialized)
    {
      _messageQueue.Enqueue((topic, message));
    }
  }

  private void SendSimulationTime(float simulationTime)
  {
    string message = $"{{ \"simulatie_tijd_ms\": {Mathf.FloorToInt(simulationTime * 1000)} }}";
    PublishMessage("tijd", message);
  }
}
