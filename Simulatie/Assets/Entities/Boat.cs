using UnityEngine;

public class Boat : MovingEntity
{
  protected override void Awake()
  {
    base.Awake();
    roadType = RoadType.River;
  }

  protected override void Update()
  {
    base.Update();
  }

}
