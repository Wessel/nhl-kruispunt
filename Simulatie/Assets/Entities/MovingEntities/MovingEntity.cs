using UnityEngine;
using UnityEngine.Splines;
using Unity.Mathematics;
using System;
using UnityEngine.UIElements;

public class MovingEntity : MonoBehaviour
{
  [SerializeField] private float maxSpeedKmh = 50f;
  [SerializeField] private int strength = 1;

  protected VehicleType type;
  protected float currentSpeed;
  protected bool isStopped = false;

  private float maxSpeed;
  private float splineDistance = 0f;

  Vector2 size;
  private float length = 0f;
  private float width = 0f;

  private MovingEntity entityInFront;
  private TrafficLight currentTrafficLight;
  private Rigidbody2D rigidBody;
  private new Collider2D collider2D;
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

    collider2D = GetComponentInChildren<Collider2D>();
    size = collider2D.bounds.size;
    length = size.x;
    width = size.y;
  }
  
protected virtual void Update()
	{
		if (currentRoad == null) return;

		CheckTrafficLight();
    UpdateSpeed();
    if (isStopped)
		{
			if (currentTrafficLight == null && entityInFront == null)
			{
				Unfreeze();
			}
			else
			{
				currentSpeed = 0f;
				return;
			}
		}
    else MoveOnRoad();
	}

  protected void MoveOnRoad()
  {
    float roadLength = currentRoad.GetLength();
    float deltaProgress = (currentSpeed / roadLength) * Time.deltaTime;
    splineDistance += deltaProgress;

    if (splineDistance >= 1f)
    {
      Despawn();
    }
    else
    {
      UpdateEntity();
    }
  }

  private void UpdateSpeed()
  {
    bool aheadBlocked = IsBlocked();

    if (entityInFront == null && !aheadBlocked)
    {
      currentSpeed = maxSpeed;
      return;
    }
    else
    {
      if (entityInFront != null && entityInFront.GetCurrentSpeed() < currentSpeed)
      {
        currentSpeed = entityInFront.GetCurrentSpeed();
      }
      else if (aheadBlocked)
      {
        Freeze();
        currentSpeed = 0f;
      }
    }
  }

  public bool IsBlocked()
  {
    float distance = width;
    Vector2 forward = transform.right;

    Vector2 frontOrigin = (Vector2)transform.position + forward * (length * 0.5f);
    Vector2 centerOrigin = (Vector2)transform.position;

    bool isBlocked = CheckRayCollision(frontOrigin, forward, distance, Color.red);

    if (splineDistance > 0.8f && currentRoad.DoesMerge()) 
    {
      // Directions for merging into other lane
      Vector2 right45 = Quaternion.Euler(0, 0, -45f) * forward;
			Vector2 right75 = Quaternion.Euler(0, 0, -45f) * forward;
      Vector2 right90 = Quaternion.Euler(0, 0, -90f) * forward;
      Vector2 right105 = Quaternion.Euler(0, 0, -135f) * forward;

      isBlocked |= CheckRayCollision(frontOrigin, right45, distance, Color.yellow);
      isBlocked |= CheckRayCollision(centerOrigin, right75, distance, Color.green);
      isBlocked |= CheckRayCollision(centerOrigin, right90, distance, Color.cyan);
      isBlocked |= CheckRayCollision(centerOrigin, right105, distance, Color.blue);
    }
    return isBlocked;
  }

  private void UpdateEntity()
  {
    Vector3 position = currentRoad.GetNewPosition(splineDistance);
    float3 tangent = currentRoad.GetNewTangent(splineDistance);
    float angle = Mathf.Atan2(tangent.y, tangent.x) * Mathf.Rad2Deg;

    rigidBody.MovePosition(position);
    rigidBody.MoveRotation(angle);
  }

  private bool CheckRayCollision(Vector2 origin, Vector2 direction, float distance, Color debugColor)
  {
    Debug.DrawRay(origin, direction * distance, debugColor);

    RaycastHit2D hit = Physics2D.Raycast(origin, direction, distance, vehicleLayerMask);
    if (hit.collider != null)
    {
      MovingEntity hitEntity = hit.collider.GetComponentInParent<MovingEntity>();
      if (hitEntity != null && hitEntity != this && !(hitEntity.GetCurrentRoad() == currentRoad) && (hitEntity.GetStrength() >= strength))
      {
        return true;
      }
    }
    return false;
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
    }
  }

  private void OnTriggerExit2D(Collider2D other)
  {
    if (other.CompareTag("StopLine") && currentTrafficLight == other.GetComponentInParent<TrafficLight>())
    {
      currentTrafficLight = null;
    }
  }

	private void CheckTrafficLight()
	{
		if (currentTrafficLight == null) return;

		switch (currentTrafficLight.GetLight())
		{
			case LightState.Red:
				Freeze();
				break;
			case LightState.Orange:
				// Check if the current traffic light is a BridgeLight
				if (currentTrafficLight is BridgeLight)
				{
					Freeze();
					break;
				}
				Transform stopLineTransform = currentTrafficLight.transform.Find("Stop line");
				float stopLineDistance = currentRoad.GetClosestDistanceOnSpline(stopLineTransform.position);
				// If across stop line, continue else stop
				if (splineDistance < stopLineDistance)
				{
					Freeze();
				}
				else
				{
					Unfreeze();
				}
				break;
			case LightState.Green:
				Unfreeze();
				break;
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

  public void ResolveEncounterWith(MovingEntity other)
  {
    if (other == null || other == this) return;

    int strengthComparison = GetStrength().CompareTo(other.GetStrength());

    switch (strengthComparison)
    {
      case > 0:
        other.Freeze();
        Unfreeze();
        break;

      case < 0:
        Freeze();
        break;

      default:
        bool isRight = IsToTheRightOf(other);
        if (isRight)
        {
          Unfreeze();
          other.Freeze();
        }
        else
        {
          Freeze();
          other.Unfreeze();
        }
        break;
    }
    SetEntityInFront(other);
  }

  public Vector3 PredictFuturePosition()
  {
    if (currentRoad == null) return transform.position;

    float roadLength = currentRoad.GetLength();
    float distanceToTravel = currentSpeed * Time.deltaTime;
    float deltaT = distanceToTravel / roadLength;

    float predictedSplineDistance = splineDistance + deltaT;
    predictedSplineDistance = Mathf.Clamp01(predictedSplineDistance);

    return currentRoad.GetPointOnSpline(predictedSplineDistance);
  }
  public void SetEntityInFront(MovingEntity front) => entityInFront = front;
  public void ClearEntityInFront() => entityInFront = null;
  public MovingEntity GetEntityInFront() => entityInFront;
  public Road GetCurrentRoad() => currentRoad;
  public float GetCurrentSplineDistance() => splineDistance;
  internal VehicleType GetVehicleType() => type;
  public int GetStrength() => strength;
  public float GetLength() => length;
  public float GetCurrentSpeed() => currentSpeed;

  public Vector2 GetSize() => size;
  public float GetRotation() => rigidBody.rotation;
  public Collider2D GetCollider() => collider2D;
}
