using UnityEngine;

public class Bike : MovingEntity
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
