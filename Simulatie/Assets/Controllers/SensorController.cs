using System.Collections.Generic;
using UnityEngine;

public abstract class SensorController : MonoBehaviour
{
  protected string topic;
  protected float noSendTime;

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

	protected virtual void Update()
	{
		if (noSendTime >= 10 && !string.IsNullOrEmpty(topic))
		{
			EventManager.Instance?.PublishMessage.Invoke(topic, BuildJson());
			noSendTime = 0;
		}
		noSendTime += Time.deltaTime;
	}
}
