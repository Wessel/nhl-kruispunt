using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

public class Intersection : MonoBehaviour
{
  public List<SplineContainer> connectedSplines;

   private void OnTriggerEnter2D(Collider2D other)
    {
        Car car = other.GetComponent<Car>();
        if (car != null)
        {
            if (connectedSplines.Count > 0)
            {
                // Pick one road at random
                SplineContainer newRoad = connectedSplines[UnityEngine.Random.Range(0, connectedSplines.Count)];

                // Tell the car to switch
                car.SetNewRoad(newRoad);
            }
        }
    }
}
