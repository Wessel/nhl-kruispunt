using UnityEngine;

public class Boat : Vehicle
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
