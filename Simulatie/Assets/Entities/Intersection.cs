using System.Collections.Generic;
using UnityEngine;

public class Intersection : MonoBehaviour
{
  public List<Road> roads;

  private void OnTriggerEnter2D(Collider2D other)
  {
    MovingEntity entity = other.GetComponent<MovingEntity>();
    if (entity == null) return;

    float currentDistance = entity.GetCurrentSplineDistance();
    Road currentRoad = entity.GetCurrentRoad();

    List<Road> matchingRoads;

    if (currentDistance > 0.9f)
    {
      // Exclude the current road
      matchingRoads = roads.FindAll(road =>
        road != currentRoad &&
        road.GetVehicleTypes().Contains(entity.GetRoadType())
      );
    }
    else
    {
      // Include all matching roads
      matchingRoads = roads.FindAll(road =>
        road.GetVehicleTypes().Contains(entity.GetRoadType())
      );
    }

    if (matchingRoads.Count == 0) return;

    Road newRoad = matchingRoads[Random.Range(0, matchingRoads.Count)];

    if (newRoad == currentRoad) return;

    float startDistance = newRoad.GetClosestDistanceOnSpline(transform.position);

    entity.SwitchToRoad(newRoad, startDistance);
  }
}
