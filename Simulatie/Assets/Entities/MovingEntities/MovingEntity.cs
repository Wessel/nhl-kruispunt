using UnityEngine;
using UnityEngine.Splines;
using Unity.Mathematics;
using System;

public class MovingEntity : MonoBehaviour
{
  [SerializeField] private float maxSpeedKmh = 50f;
  [SerializeField] private float followDistance = 1.5f;
  [SerializeField] private int strength = 1;

  protected VehicleType type;
  protected float currentSpeed;
  protected bool isStopped = false;

  private float maxSpeed;
  private float splineDistance = 0f;

  private MovingEntity entityInFront;
  private TrafficLight currentTrafficLight;
  private Rigidbody2D rigidBody;
  private EntityPool entityPool;
  private Road currentRoad;
  private LayerMask vehicleLayerMask;

  public void Initialize(EntityPool pool)
  {
    entityPool = pool;
  }

  protected virtual void Awake()
  {
    rigidBody = GetComponent<Rigidbody2D>();
    maxSpeed = ConvertKmHToUnityUnits(maxSpeedKmh);
    vehicleLayerMask = LayerMask.GetMask("Vehicles");
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
    bool aheadBlocked = IsBlockedAhead();

    if (entityInFront == null && !aheadBlocked)
    {
      currentSpeed = maxSpeed;
      return;
    }

    float distanceToFront = entityInFront != null
        ? Vector3.Distance(transform.position, entityInFront.transform.position)
        : float.MaxValue;

    if (distanceToFront < followDistance || aheadBlocked)
    {
      currentSpeed = 0f;
    }
    else
    {
      currentSpeed = maxSpeed * 0.5f;
    }
  }

  private bool IsBlockedAhead()
  {
    Vector2 direction = rigidBody.transform.right.normalized;
    float checkDistance = followDistance * 0.9f;

    RaycastHit2D hit = Physics2D.Raycast(rigidBody.position, direction, checkDistance, vehicleLayerMask);

    if (hit.collider != null)
    {
      MovingEntity hitEntity = hit.collider.GetComponentInParent<MovingEntity>();
      return hitEntity != null && hitEntity != this;
    }

    return false;
  }

  private void UpdateEntityPositionAndRotation()
  {
    Vector3 position = currentRoad.GetNewPosition(splineDistance);
    float3 tangent = currentRoad.GetNewTangent(splineDistance);
    float angle = Mathf.Atan2(tangent.y, tangent.x) * Mathf.Rad2Deg;

    rigidBody.MovePosition(position);
    rigidBody.MoveRotation(angle);
  }

  public virtual void SwitchToRoad(Road newRoad, float startDistance = 0f)
  {
    currentRoad = newRoad;
    splineDistance = startDistance;

    Vector3 position = newRoad.GetNewPosition(splineDistance);
    float3 tangent = newRoad.GetNewTangent(splineDistance);
    float angle = Mathf.Atan2(tangent.y, tangent.x) * Mathf.Rad2Deg;

    transform.position = position;
    transform.rotation = Quaternion.Euler(0, 0, angle);
  }

  public virtual void Despawn()
  {
    currentSpeed = 0f;
    splineDistance = 0f;
    isStopped = false;
    currentRoad = null;
    ClearEntityInFront();

    transform.position = new Vector3(-1000f, -1000f, 0f);
    transform.rotation = Quaternion.identity;

    entityPool.ReturnObject(this);
  }

  public void Freeze() => isStopped = true;
  public void Unfreeze() => isStopped = false;

  private void OnTriggerEnter2D(Collider2D other)
  {
    if (other.CompareTag("StopLine"))
    {
      TrafficLight trafficLight = other.GetComponentInParent<TrafficLight>();
      if (trafficLight != null && trafficLight.GetVehicleTypes().Contains(type))
      {
        currentTrafficLight = trafficLight;
      }
      MovingEntity entity = other.GetComponentInParent<MovingEntity>();
      if (entity != null)
      {
        Freeze();
      }
    }
  }

  private void OnTriggerExit2D(Collider2D other)
  {
    if (other.CompareTag("StopLine") && currentTrafficLight == other.GetComponentInParent<TrafficLight>())
    {
      currentTrafficLight = null;
    }
    MovingEntity entity = other.GetComponentInParent<MovingEntity>();
    if (entity != null)
    {
      Unfreeze();
    }
  }

  private void CheckTrafficLight()
  {
    if (currentTrafficLight == null) return;

    switch (currentTrafficLight.GetLight())
    {
      case LightState.Red: Freeze(); break;
      case LightState.Green: Unfreeze(); break;
    }
  }

  private float ConvertKmHToUnityUnits(float kmh)
  {
    return (kmh * 1000f / 3600f) / 10f; // 1 Unity unit = 10 meters
  }
  public bool IsToTheRightOf(MovingEntity other)
  {
    Vector2 toSelf = (Vector2)transform.position - (Vector2)other.transform.position;
    Vector2 rightDir = other.transform.right;
    float cross = Vector3.Cross(rightDir, toSelf).z;
    return cross < 0f;
  }

  public void SetEntityInFront(MovingEntity front) => entityInFront = front;
  public void ClearEntityInFront() => entityInFront = null;
  public MovingEntity GetEntityInFront() => entityInFront;
  public Road GetCurrentRoad() => currentRoad;
  public float GetCurrentSplineDistance() => splineDistance;
  internal VehicleType GetVehicleType() => type;
  public int GetStrength() => strength;
}
