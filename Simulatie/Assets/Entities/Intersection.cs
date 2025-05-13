using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Intersection : MonoBehaviour
{
  public List<Road> roads;

  private void OnTriggerEnter2D(Collider2D other)
  {
    MovingEntity entity = other.GetComponent<MovingEntity>();
    if (entity == null) return;

    if (!IsEntityOnValidIncomingRoad(entity)) return;

    List<Road> candidateRoads = GetValidTargetRoads(entity);
    if (candidateRoads.Count == 0) return;

    Road newRoad = ChooseNewRoad(candidateRoads);
    float startDistance = newRoad.GetClosestDistanceOnSpline(transform.position);

    entity.SwitchToRoad(newRoad, startDistance);
  }

  private bool IsEntityOnValidIncomingRoad(MovingEntity entity)
  {
    Road currentRoad = entity.GetCurrentRoad();
    return roads.Contains(currentRoad);
  }

  private List<Road> GetValidTargetRoads(MovingEntity entity)
  {
    Road currentRoad = entity.GetCurrentRoad();
    float currentDistance = entity.GetCurrentSplineDistance();

    return roads.FindAll(road =>
        road != null &&
        road.GetVehicleTypes().Contains(entity.GetVehicleType()) &&
        (currentDistance <= 0.9f || road != currentRoad) && (road.GetClosestDistanceOnSpline(transform.position) < 0.9f)
    );
  }

  private Road ChooseNewRoad(List<Road> roads)
  {
    return roads[Random.Range(0, roads.Count)];
  }
}
