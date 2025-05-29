using UnityEngine;

public class Boat : MovingEntity
{
  protected override void Awake()
  {
    base.Awake();
    type = VehicleType.Boat;
  }

  protected override void Update()
  {
    base.Update();
  }

}
