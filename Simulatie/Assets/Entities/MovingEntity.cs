using UnityEngine;
using UnityEngine.Splines;
using Unity.Mathematics;

public class MovingEntity : MonoBehaviour
{
  public Road currentRoad;
  public float maxSpeed;
  public float splineDistance = 0f;
  public float followDistance = 1.5f;

  protected RoadType roadType;
  protected float currentSpeed;
  protected bool isStopped = false;
  
  private MovingEntity entityInFront;
  private TrafficLight currentTrafficLight;
  private Rigidbody2D rigidBody;
  private BoxCollider2D boxCollider;

  protected virtual void Awake()
  {
    rigidBody = GetComponent<Rigidbody2D>();
    boxCollider = GetComponent<BoxCollider2D>();
  }

  protected virtual void Update()
  {
    CheckTrafficLight();
    if (isStopped)
    {
      currentSpeed = 0f;
    }
    else
    {
      MoveOnRoad();
    }
  }

  protected void MoveOnRoad()
  {
    currentSpeed = maxSpeed;

    if (entityInFront != null)
    {
      float distanceToFront = Vector3.Distance(transform.position, entityInFront.transform.position);

      currentSpeed = maxSpeed * 0.5f;

      if (distanceToFront < followDistance)
      {
        currentSpeed = 0f;
      }
    }

    splineDistance += (currentSpeed / currentRoad.getLength()) * Time.deltaTime;

    Vector3 position = currentRoad.getNewPosition(splineDistance);
    float3 tangent = currentRoad.getNewTangent(splineDistance);

    float angle = Mathf.Atan2(tangent.y, tangent.x) * Mathf.Rad2Deg;

    rigidBody.MovePosition(position);
    rigidBody.MoveRotation(angle);
  }
  protected void Stop()
  {
    isStopped = true;
  }
  protected void Move()
  {
    isStopped = false;
  }

  private void OnTriggerEnter2D(Collider2D other)
  {
    if (other.CompareTag("StopLine"))
    {
      TrafficLight trafficLight = other.GetComponentInParent<TrafficLight>();
      if (trafficLight != null)
      {
        currentTrafficLight = trafficLight;
      }
    }
  }

  private void OnTriggerExit2D(Collider2D other)
  {
    if (other.CompareTag("StopLine"))
    {
      TrafficLight trafficLight = other.GetComponentInParent<TrafficLight>();
      if (trafficLight != null && trafficLight == currentTrafficLight)
      {
        currentTrafficLight = null;
      }
    }
  }

  protected void CheckTrafficLight()
  {
    if (currentTrafficLight != null)
    {
      switch (currentTrafficLight.currentLight)
      {
        case LightState.Red:
          Stop();
          break;
        case LightState.Green:
          Move();
          break;
      }
    }
  }
  public void SetNewRoad(Road newRoad)
  {
    currentRoad = newRoad;
    splineDistance = 0f;
  }

  public void SetEntityInFront(MovingEntity car)
  {
    entityInFront = car;
  }

  public MovingEntity GetEntityInFront()
  {
    return entityInFront;
  }

  public void ClearEntityInFront()
  {
    entityInFront = null;
  }

  public RoadType GetRoadType()
  {
    return roadType;
  }
}
