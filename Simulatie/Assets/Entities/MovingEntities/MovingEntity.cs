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
    if (isStopped && currentTrafficLight == null && entityInFront == null)
    {
      Unfreeze();
    }

    if (isStopped)
    {
      currentSpeed = 0f;
    }
    else
    {
      MoveOnRoad();
    }
  }
  //private void OnGUI()
  //{
  //  Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position);
  //  GUI.Label(new Rect(screenPos.x, Screen.height - screenPos.y, 400, 80),
  //      $"Speed: {currentSpeed:F2} | Stopped: {(isStopped ? "Yes" : "No")}| Light: {(currentTrafficLight ? "Yes" : "No")}| Blocked: {(entityInFront ? "Yes" : "No")}");
  //}


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

    if (entityInFront != null)
    {
      float requiredSpacing = (GetLength() + entityInFront.GetLength()) * 0.5f;

      if (distanceToFront < requiredSpacing || aheadBlocked)
      {
        currentSpeed = 0f;
      }
      else
      {
        currentSpeed = maxSpeed * 0.5f;
      }
    }

  }

  public bool IsBlockedAhead()
  {
    Vector2 direction = transform.right;
    float checkDistance = 0.3f * length;
    Vector2 origin = (Vector2)transform.position + direction * (length * 0.5f);

    Debug.DrawRay(origin, direction * checkDistance, Color.red);
    
    RaycastHit2D hit = Physics2D.Raycast(origin, direction, checkDistance, vehicleLayerMask);

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
