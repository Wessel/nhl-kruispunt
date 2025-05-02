using UnityEngine;
using UnityEngine.Splines;
using Unity.Mathematics;

public class MovingEntity : MonoBehaviour
{
  public Road currentRoad;
  public float maxSpeed;
  public float splineDistance = 0f;
  public float followDistance = 1.5f;

  protected VehicleType type;
  protected float currentSpeed;
  protected bool isStopped = false;
  
  private MovingEntity entityInFront;
  private TrafficLight currentTrafficLight;
  private Rigidbody2D rigidBody;
  private BoxCollider2D boxCollider;
  private EntityPool entityPool;

  public void Initialize(EntityPool pool) 
  {
    entityPool = pool;
  }

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

    if (splineDistance >= currentRoad.getLength() * 0.9f)
    {
      ResetPosition(); // Reset positie
    }
    else
    {
      Vector3 position = currentRoad.getNewPosition(splineDistance);
      float3 tangent = currentRoad.getNewTangent(splineDistance);

      float angle = Mathf.Atan2(tangent.y, tangent.x) * Mathf.Rad2Deg;

      rigidBody.MovePosition(position);
      rigidBody.MoveRotation(angle);
    } 
  }
  private void ResetPosition()
  {
    splineDistance = 0f; // Reset splineDistance
    entityPool.ReturnObject(gameObject); //entity terug naar pool
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
      switch (currentTrafficLight.GetLight())
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

  public VehicleType GetRoadType()
  {
    return type;
  }
}
