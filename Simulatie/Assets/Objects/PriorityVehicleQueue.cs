using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

public class PriorityVehicleQueue
{
  private List<PriorityVehicle> queue = new();
  public void Enqueue(PriorityVehicle vehicle)
  {
    // Remove any existing instance of the vehicle
    queue.RemoveAll(v => v.Equals(vehicle));
    queue.Add(vehicle);

    SendOnTopic();
  }

  public void Dequeue(PriorityVehicle vehicle)
  {
    queue.Remove(vehicle);
    SendOnTopic();
  }

  private void SendOnTopic()
  {
    string json = BuildJson();
    Debug.Log(json);
    EventManager.Instance.PublishMessage.Invoke("voorrangsvoertuig", json);
  }

  public string BuildJson()
  {
    List<Dictionary<string, object>> queueList = new ();

    foreach (PriorityVehicle vehicle in queue)
    {
      Dictionary<string,object> item = new()
      {
        { "baan", vehicle.GetCurrentRoad().GetName() },
        { "simulatie_tijd_ms", vehicle.SimulationTimeMs },
        { "prioriteit", vehicle.PriorityLevel }
      };
      queueList.Add(item);
    }

    Dictionary<string,object> outer = new()
    {
      { "queue", queueList }
    };

    return JsonConvert.SerializeObject(outer, Formatting.Indented);
  }
}
