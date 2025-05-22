using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BridgeController : SensorController
{
  [SerializeField] private float openScaleX = 0.15f;
  [SerializeField] private float closedScaleX = 0.8f;
  [SerializeField] private float animationDuration = 5.0f;
  [SerializeField] private Vector2 closedPosition =  new Vector2(15.95f, -2.5f);
  [SerializeField] private Vector2 openPosition = new Vector2(16.3f, -2.7f);

  private BridgeSensor sensor;
  private bool isAnimating = false;

  protected override void Start()
  {
    base.Start();
    topic = "sensoren_bruggen";
    EventManager.Instance.SetBridgeState.AddListener(SetBridgeState);
    foreach (Sensor sensor in sensors)
    {
      if (sensor is BridgeSensor bridgeSensor)
      {
        this.sensor = bridgeSensor;
        SetBridgeState(bridgeSensor.GetState());
        break;
      }
    }
  }

  public void SetBridgeState(BridgeState state)
  {
    if (isAnimating || state == BridgeState.Unknown || state == sensor.GetState())
      return;

    StartCoroutine(AnimateBridge(state));
    sensor.SetState(state);
  }

  private IEnumerator AnimateBridge(BridgeState state)
  {
    isAnimating = true;
    sensor.SetState(BridgeState.Unknown);

    float startScaleX = transform.localScale.x;
    float targetScaleX = state == BridgeState.Open ? openScaleX : closedScaleX;

    Vector3 startPos = transform.position;
    Vector3 targetPos = state == BridgeState.Open ? (Vector3)openPosition : (Vector3)closedPosition;

    float startSimTime = SimulationManager.Instance.GetSimulationTime();
    float endSimTime = startSimTime + animationDuration;

    while (SimulationManager.Instance.GetSimulationTime() < endSimTime)
    {
      float currentSimTime = SimulationManager.Instance.GetSimulationTime();
      float t = Mathf.InverseLerp(startSimTime, endSimTime, currentSimTime);

      float newScaleX = Mathf.Lerp(startScaleX, targetScaleX, t);
      Vector3 newPos = Vector3.Lerp(startPos, targetPos, t);

      transform.localScale = new Vector3(newScaleX, transform.localScale.y, transform.localScale.z);
      transform.position = new Vector3(newPos.x, newPos.y, transform.position.z);

      yield return null;
    }

    transform.localScale = new Vector3(targetScaleX, transform.localScale.y, transform.localScale.z);
    transform.position = new Vector3(targetPos.x, targetPos.y, transform.position.z);

    sensor.SetState(state);
    isAnimating = false;
  }


  public override string BuildJson()
  {
    Dictionary<string, object> bridgeData = new ();

    foreach (Sensor sensor in sensors)
    {
      if (sensor is BridgeSensor bridgeSensor)
      {
        bridgeData[sensor.GetID()] = new
        {
          state = bridgeSensor.GetState()
        };
      }
    }

    return JsonConvert.SerializeObject(bridgeData, Formatting.Indented);
  }

  public override void HandleSensorStateChange()
  {
    Debug.Log("Bridge sensor state changed");
    EventManager.Instance?.PublishMessage.Invoke(topic, BuildJson());
  }

  [ContextMenu("Open Bridge")]
  private void OpenBridge()
  {
    SetBridgeState(BridgeState.Open);
  }

  [ContextMenu("Close Bridge")]
  private void CloseBridge()
  {
    SetBridgeState(BridgeState.Closed);
  }
}
