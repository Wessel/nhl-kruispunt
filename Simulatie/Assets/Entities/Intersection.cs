using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Intersection : MonoBehaviour
{
  public List<Road> incomingRoads;
  public List<Road> outgoingRoads;

  private HashSet<MovingEntity> mergingEntities = new();

  private void OnTriggerEnter2D(Collider2D other)
  {
    MovingEntity entity = other.GetComponent<MovingEntity>();
    if (entity == null || mergingEntities.Contains(entity)) return;

    Road currentRoad = entity.GetCurrentRoad();
    if (!incomingRoads.Contains(currentRoad)) return;

    List<Road> validOutgoings = GetValidRoads(entity);
    if (validOutgoings.Count == 0)
    {
      Debug.LogWarning($"No valid outgoing roads for {entity.name}");
      return;
    }

    Road chosenRoad = (validOutgoings.Count == 1) ? validOutgoings[0] : ChooseNewRoad(validOutgoings);
    if (chosenRoad == currentRoad) return;

    StartCoroutine(SwitchWhenSafe(entity, chosenRoad));
  }

  private List<Road> GetValidRoads(MovingEntity entity)
  {
    return outgoingRoads.FindAll(road =>
        road != null &&
        road.GetVehicleTypes().Contains(entity.GetVehicleType())
    );
  }

  private Road ChooseNewRoad(List<Road> roads)
  {
    return roads[Random.Range(0, roads.Count)];
  }

  private IEnumerator SwitchWhenSafe(MovingEntity entity, Road newRoad)
  {
    mergingEntities.Add(entity);
    entity.Freeze();

    float entityLength = entity.GetLength();
    float startDistance = newRoad.GetClosestDistanceOnSpline();

    while (!IsRoadClearAt(newRoad, startDistance, entityLength))
    {
      yield return new WaitForSeconds(0.2f);
    }

    entity.SwitchToRoad(newRoad, startDistance);
    entity.Unfreeze();
    mergingEntities.Remove(entity);
  }

  private bool IsRoadClearAt(Road road, float distance, float entityLength)
  {
    Vector2 position = road.GetNewPosition(distance);
    Vector2 direction = new Vector2(road.GetNewTangent(distance).x, road.GetNewTangent(distance).y).normalized;
    Vector2 checkPosition = position + direction * (entityLength / 2f);
    Vector2 boxSize = new Vector2(entityLength * 1.1f, 0.8f); 

    Collider2D hit = Physics2D.OverlapBox(checkPosition, boxSize, 0f, LayerMask.GetMask("Vehicles"));
    return hit == null;
  }
}
