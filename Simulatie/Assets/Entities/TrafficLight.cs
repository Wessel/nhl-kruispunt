using UnityEngine;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Linq;

public class TrafficLight : SensorController
{  
  public Sprite redLight;
  public Sprite orangeLight;
  public Sprite greenLight;

  private string id;
  private List<VehicleType> types;
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
    SetVehicleTypes();
  }

  private void SetVehicleTypes()
  {
    types = sensors
        .SelectMany(sensor => sensor.GetAllowedVehicleTypes())
        .Distinct()
        .ToList();

    if (types.Count == 0)
    {
      if (string.IsNullOrEmpty(id)) return;

      char firstChar = id[0];
      types = new List<VehicleType>();

      switch (firstChar)
      {
        case '1':
        case '4':
          types.Add(VehicleType.Car);
          break;
        case '2':
          types.Add(VehicleType.Bike);
          break;
        case '3':
          types.Add(VehicleType.Walk);
          break;
        case '5':
          types.Add(VehicleType.Bike);
          types.Add(VehicleType.Walk);
          break;
        case '7':
          types.Add(VehicleType.Boat);
          break;
        default:
          Debug.LogWarning($"Unhandled ID prefix '{firstChar}' in TrafficLight ID '{id}'");
          break;
      }
    }
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
    Dictionary<string, bool> inner = new();

    foreach (Sensor sensor in sensors)
    {
      inner[sensor.GetID()] = sensor.IsActive();
    }

    Dictionary<string, object> outer = new()
    {
      [id] = inner
    };

    return JsonConvert.SerializeObject(outer, Formatting.Indented);
  }

  public List<VehicleType> GetVehicleTypes()
  {
    return types;
  }
}
