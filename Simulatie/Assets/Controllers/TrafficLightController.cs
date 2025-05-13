using UnityEngine;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

public class TrafficLightController : MonoBehaviour
{
	private List<TrafficLight> trafficLights = new();
  private string topic = "sensoren_rijbaan";

  private void Start()
	{
		FindTrafficLights();
    EventManager.Instance?.OnTrafficLightUpdate.AddListener(UpdateTrafficLights);
  }

  private void FindTrafficLights()
  {
    foreach (Transform child in transform)
    {
      if (child.CompareTag("TrafficLight"))
      {
        if (child.TryGetComponent<TrafficLight>(out var trafficLight))
        {
          trafficLights.Add(trafficLight);
          trafficLight.SetController(this);
        }
      }
    }
  }

  private void UpdateTrafficLights(string data)
  {
    Dictionary<string, LightState> updates = JsonConvert.DeserializeObject<Dictionary<string, LightState>>(data);
    foreach (var light in trafficLights)
    {
      if (updates.TryGetValue(light.GetID(), out var newState))
      {
        light.SetLight(newState);
      }
    }
  }

  private string BuildCombinedJson()
  {
    JObject combined = new JObject();

    foreach (TrafficLight light in trafficLights)
    {
      string json = light.BuildJson();
      JObject parsed = JObject.Parse(json);

      combined.Merge(parsed, new JsonMergeSettings
      {
        MergeArrayHandling = MergeArrayHandling.Union
      });
    }

    return combined.ToString(Formatting.Indented);
  }
  public void OnTrafficLightSensorChanged()
  {
    EventManager.Instance?.PublishMessage.Invoke(topic, BuildCombinedJson());
  }
}
