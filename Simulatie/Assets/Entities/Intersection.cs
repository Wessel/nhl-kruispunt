
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
        road.GetVehicleTypes().Contains(entity.GetRoadType())
    );

    Road newRoad = matchingRoads[Random.Range(0, matchingRoads.Count)];
    if (newRoad == currentRoad) return;
    float startDistance = newRoad.GetClosestDistanceOnSpline(transform.position);

    entity.SwitchToRoad(newRoad, startDistance);
  }
}
