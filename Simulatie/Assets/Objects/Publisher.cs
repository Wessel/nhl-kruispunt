using UnityEngine;
using NetMQ;
using NetMQ.Sockets;
using System.Collections.Concurrent;
using System.Threading;

public class Publisher : MonoBehaviour
{
  private Thread _publisherThread;
  private PublisherSocket _pubSocket;
  private bool _isRunning;
  private bool _isSocketInitialized;
  private ConcurrentQueue<(string topic, string message)> _messageQueue = new();

  private void Start()
  {
    Application.wantsToQuit += HandleApplicationWantsToQuit;

    EventManager.Instance.SendSimulationTime.AddListener(SendSimulationTime);
    EventManager.Instance.PublishMessage.AddListener(PublishMessage);

    _isRunning = true;
    _publisherThread = new Thread(PublisherWork) { IsBackground = true };
    _publisherThread.Start();
  }

  private void OnDisable()
  {
    Application.wantsToQuit -= HandleApplicationWantsToQuit;
  }

  private bool HandleApplicationWantsToQuit()
  {
    Shutdown();
    return true;
  }

  private void PublisherWork()
  {
    try
    {
      AsyncIO.ForceDotNet.Force();

      _pubSocket = new PublisherSocket();
      _pubSocket.Options.SendHighWatermark = 1000;
      _pubSocket.Bind($"{Config.Instance.Method}://{Config.Instance.PublishIP}:{Config.Instance.PublishPort}");
      _isSocketInitialized = true;

      Debug.Log("Publisher started.");

      while (_isRunning)
      {
        while (_messageQueue.TryDequeue(out var msg))
        {
          _pubSocket.SendMoreFrame(msg.topic).SendFrame(msg.message);
          Debug.Log($"Published: ({msg.topic}) {msg.message}");
        }

        Thread.Sleep(10);
      }
    }
    catch (System.Exception ex)
    {
      Debug.LogError($"Publisher thread exception: {ex}");
    }

    Debug.Log("Publisher thread exiting.");
  }

  private void SendSimulationTime(float simulationTime)
  {
    string message = $"{{ \"simulatie_tijd_ms\": {Mathf.FloorToInt(simulationTime * 1000)} }}";
    PublishMessage("tijd", message);
  }

  private void PublishMessage(string topic, string message)
  {
    if (_isSocketInitialized)
    {
      _messageQueue.Enqueue((topic, message));
    }
  }

  private void Shutdown()
  {
    _isRunning = false;

    if (_publisherThread != null)
    {
      if (!_publisherThread.Join(2000))
      {
        Debug.LogWarning("Publisher thread did not stop in time.");
      }
      _publisherThread = null;
    }

    _pubSocket?.Close();
    _pubSocket = null;

    NetMQConfig.Cleanup();
    Debug.Log("Publisher shutdown complete.");
  }
}
