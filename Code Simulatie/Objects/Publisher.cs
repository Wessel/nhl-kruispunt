using UnityEngine;
using NetMQ;
using NetMQ.Sockets;
using System.Collections.Concurrent;
using System.Threading;
using System;
using UnityEditor;

public class Publisher : MonoBehaviour
{
  private Thread publisherThread;
  private PublisherSocket pubSocket;
  private CancellationTokenSource cancellationTokenSource;
  private bool isSocketInitialized;
  private ConcurrentQueue<(string topic, string message)> messageQueue = new();
  private readonly object socketLock = new();

  private void Start()
  {
    EventManager.Instance.SendSimulationTime.AddListener(SendSimulationTime);
    EventManager.Instance.PublishMessage.AddListener(PublishMessage);

    cancellationTokenSource = new CancellationTokenSource();
    publisherThread = new Thread(() => PublisherWork(cancellationTokenSource.Token))
    {
      IsBackground = true
    };
    publisherThread.Start();
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

      lock (socketLock)
      {
        pubSocket = new PublisherSocket();
        pubSocket.Options.SendHighWatermark = 1000;
        pubSocket.Bind($"{ZeroMQConfig.Instance.Method}://{ZeroMQConfig.Instance.PublishIP}:{ZeroMQConfig.Instance.PublishPort}");
        isSocketInitialized = true;
      }

      Debug.Log("Publisher started.");

      while (!token.IsCancellationRequested)
      {
        while (messageQueue.TryDequeue(out var msg))
        {
          lock (socketLock)
          {
            if (pubSocket != null && isSocketInitialized)
            {
              pubSocket.SendMoreFrame(msg.topic).SendFrame(msg.message);
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
      lock (socketLock)
      {
        pubSocket?.Close();
        pubSocket?.Dispose();
        pubSocket = null;
        isSocketInitialized = false;
      }

      NetMQConfig.Cleanup();
    }
  }
  private void Shutdown()
  {
    if (cancellationTokenSource != null)
    {
      cancellationTokenSource.Cancel();
    }
    Debug.Log("Publisher shutdown.");
  }

  private void PublishMessage(string topic, string message)
  {
    if (isSocketInitialized)
    {
      messageQueue.Enqueue((topic, message));
    }
  }

  private void SendSimulationTime(float simulationTime)
  {
    string message = $"{{ \"simulatie_tijd_ms\": {Mathf.FloorToInt(simulationTime * 1000)} }}";
    PublishMessage("tijd", message);
  }
}
