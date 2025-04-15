using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

public class Intersection : MonoBehaviour
{
  public List<Road> roads;

   private void OnTriggerEnter2D(Collider2D other)
    {
        MovingEntity entity = other.GetComponent<MovingEntity>();
        if (entity != null)
        {
            if (roads.Count > 0)
            {
				      List<Road> matchingRoads = roads.FindAll(road => road.getRoadTypes().Contains(entity.GetRoadType()));

				      if (matchingRoads.Count > 0)
				      {
					      Road newRoad = matchingRoads[UnityEngine.Random.Range(0, matchingRoads.Count)];
					      entity.SetNewRoad(newRoad);
				      }
            }
        }
    }
}
