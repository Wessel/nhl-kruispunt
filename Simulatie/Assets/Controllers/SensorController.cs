using System.Collections.Generic;
using UnityEngine;

public abstract class SensorController : MonoBehaviour
{
  protected string topic;

  protected List<Sensor> sensors = new();

  public abstract string BuildJson();

  public abstract void HandleSensorStateChange();

  protected virtual void Start()
  {
    foreach (Transform child in transform)
    {
      if (child.TryGetComponent<Sensor>(out var sensor))
      {
        sensors.Add(sensor);
        sensor.onStateChanged.AddListener(HandleSensorStateChange);
      }
    }
  }
}
