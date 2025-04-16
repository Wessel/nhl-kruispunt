using UnityEngine;
using NetMQ;
using NetMQ.Sockets;
using System.Threading;

public class Publisher : MonoBehaviour
{
  private Thread _publisherThread;
  private PublisherSocket _pubSocket;
  private bool _isRunning;
  private bool _isSocketInitialized = false;

  private void Start()
  {
    // Subscribe to events with the topic and message as parameters
    EventManager.Instance.SendSimulationTime.AddListener(SendSimulationTime);
    EventManager.Instance.PublishMessage.AddListener(PublishMessage);

    StartPublisherThread();
  }

  private void OnApplicationQuit()
  {
    StopPublisherThread();
  }

  private void StartPublisherThread()
  {
    if (_isRunning) return;

    _isRunning = true;
    _publisherThread = new Thread(PublisherWork) { IsBackground = true };
    _publisherThread.Start();
  }

  private void StopPublisherThread()
  {
    _isRunning = false;
    _publisherThread?.Join();
    _publisherThread = null;

    _pubSocket?.Close();
    _pubSocket = null;
    NetMQConfig.Cleanup();
  }

  private void PublisherWork()
  {
    AsyncIO.ForceDotNet.Force();
    _pubSocket = new PublisherSocket();
    _pubSocket.Options.SendHighWatermark = 1000;
    _pubSocket.Bind($"{Config.Instance.Method}://{Config.Instance.PublishIP}:{Config.Instance.PublishPort}");
    _isSocketInitialized = true;
    Debug.Log($"Publisher bound to {Config.Instance.Method}://{Config.Instance.PublishIP}:{Config.Instance.PublishPort}");

    // Continuous loop to listen for events and send messages when needed
    while (_isRunning)
    {
      Thread.Sleep(10);  // Sleep to avoid tight looping
    }
  }

  // Handle the simulation time update event and publish to the "tijd" topic
  private void SendSimulationTime(float simulationTime)
  {
    string message = $"{{ \"simulatie_tijd_ms\": {Mathf.FloorToInt(simulationTime * 1000)} }}";
    PublishMessage("tijd", message);
  }

  // Generic method to handle the publishing of messages to the specified topic
  private void PublishMessage(string topic, string message)
  {
    if (_isSocketInitialized)
    {
      _pubSocket.SendMoreFrame(topic).SendFrame(message);
      Debug.Log($"Published: ({topic}) {message}");
    }
  }
}
