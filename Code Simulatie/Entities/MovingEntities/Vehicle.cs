using UnityEngine;

public class Vehicle : MovingEntity
{
  protected override void Awake()
  {
    base.Awake();
    type = VehicleType.Car;
  }

  protected override void Update()
  {
    base.Update();
  }
}
