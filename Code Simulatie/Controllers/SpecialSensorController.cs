using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class SpecialSensorController : SensorController
{
  protected override void Start()
  {
    base.Start();
    topic = "sensoren_speciaal";
  }

  public override string BuildJson()
  {
    var stateDict = new Dictionary<string, bool>();

    foreach (var sensor in sensors)
    {
      stateDict[sensor.GetID()] = sensor.IsActive();
    }

    return JsonConvert.SerializeObject(stateDict, Formatting.Indented);
  }

  public override void HandleSensorStateChange()
  {
    EventManager.Instance?.PublishMessage.Invoke(topic, BuildJson());
  }
}
