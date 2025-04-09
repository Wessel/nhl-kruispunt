using UnityEngine;

public class Bike : Vehicle
{
  protected override void Awake()
  {
    base.Awake();
    roadType = RoadType.Bike;
  }

  protected override void Update()
  {
    base.Update();
  }
}
