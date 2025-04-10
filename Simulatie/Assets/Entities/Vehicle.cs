using UnityEngine;

public class Vehicle : MovingEntity
{
  protected override void Awake()
  {
    base.Awake();
    roadType = RoadType.Car;
  }

  protected override void Update()
  {
    base.Update();
  }
}
