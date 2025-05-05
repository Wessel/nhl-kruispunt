using UnityEngine;
using UnityEngine.Splines;
using Unity.Mathematics;

public class MovingEntity : MonoBehaviour
{
  public float maxSpeed = 5f;
  public float followDistance = 1.5f;

  public float splineDistance = 0f;

  protected VehicleType type;
  protected float currentSpeed;
  protected bool isStopped = false;

  private MovingEntity entityInFront;
  private TrafficLight currentTrafficLight;
  private Rigidbody2D rigidBody;
  private EntityPool entityPool;

  private Road currentRoad;
  private Road pendingRoad;

  public void Initialize(EntityPool pool)
  {
    entityPool = pool;
  }

  protected virtual void Awake()
  {
    rigidBody = GetComponent<Rigidbody2D>();
  }

  protected virtual void Update()
  {
    if (currentRoad == null && pendingRoad != null)
    {
      currentRoad = pendingRoad;
      pendingRoad = null;
      splineDistance = 0f;
    }

    if (currentRoad == null) return;

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
    UpdateSpeedBasedOnEntityInFront();

    float roadLength = currentRoad.getLength();
    float deltaProgress = (currentSpeed / roadLength) * Time.deltaTime;
    splineDistance += deltaProgress;

    if (splineDistance >= 1f)
    {
      if (pendingRoad != null)
      {
        SwitchToPendingRoad();
      }
      else
      {
        Despawn();
      }
    }
    else
    {
      UpdateEntityPositionAndRotation();
    }

  }

  private void UpdateSpeedBasedOnEntityInFront()
  {
    if (entityInFront == null)
    {
      currentSpeed = maxSpeed;
      return;
    }

    float distanceToFront = Vector3.Distance(transform.position, entityInFront.transform.position);

    currentSpeed = (distanceToFront < followDistance)
        ? 0f
        : maxSpeed * 0.5f;
  }

  private void UpdateEntityPositionAndRotation()
  {
    Vector3 position = currentRoad.getNewPosition(splineDistance);
    float3 tangent = currentRoad.getNewTangent(splineDistance);
    float angle = Mathf.Atan2(tangent.y, tangent.x) * Mathf.Rad2Deg;

    rigidBody.MovePosition(position);
    rigidBody.MoveRotation(angle);
  }

  private void SwitchToPendingRoad()
  {
    currentRoad = pendingRoad;
    pendingRoad = null;
    splineDistance = 0f;
  }

  private void Despawn()
  {
    // Reset movement state
    currentSpeed = 0f;
    splineDistance = 0f;
    isStopped = false;

    // Clear road references
    currentRoad = null;
    pendingRoad = null;

    ClearEntityInFront();

    // Move offscreen or to a neutral reset position
    transform.position = new Vector3(-1000f, -1000f, 0f);
    transform.rotation = Quaternion.identity;

    // Return to pool
    entityPool.ReturnObject(this);
  }

  protected void Stop() => isStopped = true;
  protected void Move() => isStopped = false;

  private void OnTriggerEnter2D(Collider2D other)
  {
    if (other.CompareTag("StopLine"))
    {
      var light = other.GetComponentInParent<TrafficLight>();
      if (light != null)
      {
        currentTrafficLight = light;
      }
    }
  }

  private void OnTriggerExit2D(Collider2D other)
  {
    if (other.CompareTag("StopLine"))
    {
      var light = other.GetComponentInParent<TrafficLight>();
      if (light != null && light == currentTrafficLight)
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
    pendingRoad = null;
    splineDistance = 0f;
  }

  public void PrepareRoadSwitch(Road firstRoad)
  {
    SetNewRoad(firstRoad);
  }

  public void QueueRoadSwitch(Road nextRoad)
  {
    pendingRoad = nextRoad;
  }

  public VehicleType GetRoadType() => type;

  public void SetEntityInFront(MovingEntity front) => entityInFront = front;
  public void ClearEntityInFront() => entityInFront = null;
  public MovingEntity GetEntityInFront() => entityInFront;
  public Road GetCurrentRoad() => currentRoad;
}
