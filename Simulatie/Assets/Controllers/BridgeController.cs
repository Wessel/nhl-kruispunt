using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;

public class BridgeController : SensorController
{
  protected override void Start()
  {
    base.Start();
    topic = "sensoren_bruggen";
  }

  public override string BuildJson()
  {
    var bridgeData = new Dictionary<string, object>();

    foreach (var sensor in sensors)
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
    EventManager.Instance?.PublishMessage.Invoke(topic, BuildJson());
  }
}
