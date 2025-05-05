using System.Collections.Generic;
using UnityEngine;

public class Intersection : MonoBehaviour
{
  public List<Road> roads;

  private void OnTriggerEnter2D(Collider2D other)
  {
    MovingEntity entity = other.GetComponent<MovingEntity>();
    if (entity == null) return;

    Road currentRoad = entity.GetCurrentRoad();
    List<Road> matchingRoads = roads.FindAll(road =>
        road != currentRoad &&
        road.getRoadTypes().Contains(entity.GetRoadType())
    );

    if (matchingRoads.Count > 0)
    {
      Road newRoad = matchingRoads[Random.Range(0, matchingRoads.Count)];
      entity.PrepareRoadSwitch(newRoad);
    }
  }
}
