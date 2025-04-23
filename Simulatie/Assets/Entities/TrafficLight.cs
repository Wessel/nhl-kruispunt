using UnityEngine;
using System.Collections;
using NUnit.Framework;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Linq;

public class TrafficLight : SensorController
{  
  public Sprite redLight;
  public Sprite orangeLight;
  public Sprite greenLight;

  private string id;
  private LightState currentLight;
  private SpriteRenderer spriteRenderer;

  private TrafficLightController controller;

  public void SetController(TrafficLightController controller)
  {
    this.controller = controller;
  }

  protected override void Start()
  {
    base.Start();
    spriteRenderer = GetComponentsInChildren<SpriteRenderer>()
                 .FirstOrDefault(sr => sr.CompareTag("Sprite"));
    id = gameObject.name;
  }
  public virtual void SetLight(LightState newState)
  {
    currentLight = newState;
    switch (newState)
    {
      case LightState.Red:
        spriteRenderer.sprite = redLight;
        break;
      case LightState.Orange:
        spriteRenderer.sprite = orangeLight;
        break;
      case LightState.Green:
        spriteRenderer.sprite = greenLight;
        break;
    }
  }
  public LightState GetLight()
  {
    return currentLight;
  }

  public string GetID()
  {
    return id;
  }

  public override void HandleSensorStateChange()
  {
    controller?.OnTrafficLightSensorChanged();
  }

  public override string BuildJson()
  {
    var inner = new Dictionary<string, bool>();

    foreach (var sensor in sensors)
    {
      inner[sensor.GetID()] = sensor.IsActive();
    }

    var outer = new Dictionary<string, object>
    {
      [id] = inner
    };

    return JsonConvert.SerializeObject(outer, Formatting.Indented);
  }
}
