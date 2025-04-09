using UnityEngine;
using UnityEngine.Splines;
using Unity.Mathematics;

public class Car : MonoBehaviour
{
  public SplineContainer road;
  public float speed = 5f;
  public float splineDistance = 0f;
  public float followDistance = 1.5f;

  private float currentSpeed;
  private Car carInFront;
  private bool isStopped = false;
  private TrafficLight currentTrafficLight;
  private Rigidbody2D rigidBody;
  private BoxCollider2D boxCollider;

  void Awake()
  {
    rigidBody = GetComponent<Rigidbody2D>();
    boxCollider = GetComponent<BoxCollider2D>();
  }

  void Update()
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

  private void MoveOnRoad()
  {
    currentSpeed = speed;

    if (carInFront != null)
    {
      float distanceToFront = Vector3.Distance(transform.position, carInFront.transform.position);

      currentSpeed = speed * 0.5f;

      if (distanceToFront < followDistance)
      {
        currentSpeed = 0f;
      }
    }

    splineDistance += (currentSpeed / road.Spline.GetLength()) * Time.deltaTime;

    Vector3 position = road.EvaluatePosition(splineDistance);
    float3 tangent = road.EvaluateTangent(splineDistance);

    float angle = Mathf.Atan2(tangent.y, tangent.x) * Mathf.Rad2Deg;

    rigidBody.MovePosition(position);
    rigidBody.MoveRotation(angle);
  }
  private void StopCar()
  {
    isStopped = true;
  }
  private void StartCar()
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

  // Check the current light state and act accordingly
  private void CheckTrafficLight()
  {
    if (currentTrafficLight != null)
    {
      switch (currentTrafficLight.currentLight)
      {
        case LightState.Red:
          StopCar();
          break;
        case LightState.Green:
          StartCar();
          break;
        case LightState.Yellow:
          // Optional: Can slow down or stop briefly for yellow
          break;
      }
    }
  }
  public void SetNewRoad(SplineContainer newRoad)
  {
    road = newRoad;
    splineDistance = 0f;
  }

  public void SetCarInFront(Car car)
  {
    carInFront = car;
  }

  public Car GetCarInFront()
  {
    return carInFront;
  }

  public void ClearCarInFront()
  {
    carInFront = null;
  }

}
