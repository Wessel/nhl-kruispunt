using System.Collections.Generic;
using UnityEngine;

public class Intersection : MonoBehaviour
{
  public List<Road> incomingRoads;
  public List<Road> outgoingRoads;

  private void OnTriggerEnter2D(Collider2D other)
  {
    MovingEntity entity = other.GetComponent<MovingEntity>();
    if (entity == null) return;

    Road currentRoad = entity.GetCurrentRoad();
    if (!incomingRoads.Contains(currentRoad)) return;

    // If only one valid outgoing road exists, switch to it immediately
    if (outgoingRoads.Count == 1)
    {
      if (outgoingRoads[0] == currentRoad) return;
      SwitchEntityToRoad(entity, outgoingRoads[0]);
      return;
    }

    List<Road> validOutgoings = GetValidRoads(entity);
    if (validOutgoings.Count == 0) return;

    Road newRoad = ChooseNewRoad(validOutgoings);
    if (newRoad == currentRoad) return;
    SwitchEntityToRoad(entity, newRoad);
  }

  private List<Road> GetValidRoads(MovingEntity entity)
  {
    float currentDistance = entity.GetCurrentSplineDistance();

    return outgoingRoads.FindAll(road =>
        road != null &&
        road.GetVehicleTypes().Contains(entity.GetVehicleType())
    );
  }

  private Road ChooseNewRoad(List<Road> roads)
  {
    return roads[Random.Range(0, roads.Count)];
  }

  private void SwitchEntityToRoad(MovingEntity entity, Road newRoad)
  {
    float startDistance = newRoad.GetClosestDistanceOnSpline(entity.PredictFuturePosition());
    entity.SwitchToRoad(newRoad, startDistance);
  }
}
