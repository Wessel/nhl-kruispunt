using NetMQ.Sockets;
using NetMQ;
using System.Threading;
using System;
using UnityEngine;
using UnityEditor.Experimental.GraphView;

public class Publisher : MonoBehaviour
{
  private Thread _publisherThread;
  private bool _isRunning;

  private void Start()
  {
    EventManager.Instance.onStartClient.AddListener(StartPublishing);
    EventManager.Instance.onStopClient.AddListener(StopPublishing);
  }

  private void StartPublishing()
  {
    if (_isRunning) return;

    Debug.Log("Starting Publisher...");
    _isRunning = true;
    _publisherThread = new Thread(PublisherWork);
    _publisherThread.Start();
  }

  private void StopPublishing()
  {
    _isRunning = false;
    _publisherThread?.Join();
    _publisherThread = null;
    Debug.Log("Publisher stopped.");
  }

  private void PublisherWork()
  {
    AsyncIO.ForceDotNet.Force();
    using (var pubSocket = new PublisherSocket())
    {
      pubSocket.Options.SendHighWatermark = 1000;
      pubSocket.Bind($"{Config.Instance.Method}://{Config.Instance.PublishIP}:{Config.Instance.PublishPort}");
      Debug.Log($"Publisher bound to {Config.Instance.Method}://{Config.Instance.PublishIP}:{Config.Instance.PublishPort}");

      while (_isRunning)
      {
        string topic = "simulatie_send";
        string message = "Hallo vanuit mijn simulatie";
        pubSocket.SendMoreFrame(topic).SendFrame(message);
        Debug.Log($"Published: ({topic}) {message}");
        Thread.Sleep(1000);
      }
    }
    NetMQConfig.Cleanup();
  }
}
