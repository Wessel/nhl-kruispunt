using UnityEngine;

public class Bike : MovingEntity
{
  protected override void Awake()
  {
    base.Awake();
    type = VehicleType.Bike;
  }

  protected override void Update()
  {
    base.Update();
  }
}
