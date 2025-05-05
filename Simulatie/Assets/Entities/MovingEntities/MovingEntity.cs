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

    float roadLength = currentRoad.GetLength();
    float deltaProgress = (currentSpeed / roadLength) * Time.deltaTime;
    splineDistance += deltaProgress;

    if (splineDistance >= 1f)
    {
        Despawn();
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
    Vector3 position = currentRoad.GetNewPosition(splineDistance);
    float3 tangent = currentRoad.GetNewTangent(splineDistance);
    float angle = Mathf.Atan2(tangent.y, tangent.x) * Mathf.Rad2Deg;

    rigidBody.MovePosition(position);
    rigidBody.MoveRotation(angle);
  }

  public void SwitchToRoad(Road newRoad, float startDistance = 0f)
  {
    currentRoad = newRoad;
    splineDistance = startDistance;

    Vector3 position = newRoad.GetNewPosition(splineDistance);
    float3 tangent = newRoad.GetNewTangent(splineDistance);
    float angle = Mathf.Atan2(tangent.y, tangent.x) * Mathf.Rad2Deg;

    transform.position = position;
    transform.rotation = Quaternion.Euler(0, 0, angle);
  }

  private void Despawn()
  {
    currentSpeed = 0f;
    splineDistance = 0f;
    isStopped = false;

    currentRoad = null;

    ClearEntityInFront();

    // move offscreen
    transform.position = new Vector3(-1000f, -1000f, 0f);
    transform.rotation = Quaternion.identity;

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

  public VehicleType GetRoadType() => type;

  public void SetEntityInFront(MovingEntity front) => entityInFront = front;
  public void ClearEntityInFront() => entityInFront = null;
  public MovingEntity GetEntityInFront() => entityInFront;
  public Road GetCurrentRoad() => currentRoad;
}
