using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TrafficLightController : MonoBehaviour
{
	public float redTime = 5f;
	public float yellowTime = 1f;
	public float greenTime = 5f;

	public List<TrafficLight> trafficLights = new List<TrafficLight>();

	private void Start()
	{
		FindTrafficLights();
    StartCoroutine(TrafficCycle());
	}

	IEnumerator TrafficCycle()
	{
		while (true)
		{
			SetLights(LightState.Red);
			yield return new WaitForSeconds(redTime);

			SetLights(LightState.Yellow);
			yield return new WaitForSeconds(yellowTime);

			SetLights(LightState.Green);
			yield return new WaitForSeconds(greenTime);
		}
	}

  private void FindTrafficLights()
  {
    foreach (Transform child in transform)
    {
      if (child.CompareTag("TrafficLight"))
      {
        TrafficLight trafficLight = child.GetComponent<TrafficLight>();
        if (trafficLight != null)
        {
          trafficLights.Add(trafficLight);
        }
      }
    }
  }

	private void SetLights(LightState state)
	{
		foreach (var light in trafficLights)
		{
			light.SetLight(state);
		}
	}
}
