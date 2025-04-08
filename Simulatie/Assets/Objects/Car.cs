using UnityEngine;
using UnityEngine.Splines;
using Unity.Mathematics;

public class Car : MonoBehaviour
{
  public SplineContainer road;
  public float speed = 5f;
  public float splineDistance = 0f;

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
      rigidBody.linearVelocity = Vector2.zero;
    }
    else
    {
      MoveOnRoad();
    }
  }

  private void MoveOnRoad()
  {
    // Move along the spline
    splineDistance += (speed / road.Spline.GetLength()) * Time.deltaTime;

    // Get position and tangent along the spline
    Vector3 position = road.EvaluatePosition(splineDistance);
    float3 tangent = road.EvaluateTangent(splineDistance);

    // Calculate the angle in degrees
    float angle = Mathf.Atan2(tangent.y, tangent.x) * Mathf.Rad2Deg;

    // Apply position and rotation using Rigidbody
    rigidBody.MovePosition(position);
    rigidBody.MoveRotation(angle);

    // Calculate the new size and offset for the box collider
    Vector2 size = boxCollider.size;
    Vector2 offset = boxCollider.offset;

    // Rotate the size and offset based on the angle
    float rad = angle * Mathf.Deg2Rad;
    float cos = Mathf.Cos(rad);
    float sin = Mathf.Sin(rad);

    Vector2 newSize = new Vector2(
        Mathf.Abs(size.x * cos) + Mathf.Abs(size.y * sin),
        Mathf.Abs(size.x * sin) + Mathf.Abs(size.y * cos)
    );

    Vector2 newOffset = new Vector2(
        offset.x * cos - offset.y * sin,
        offset.x * sin + offset.y * cos
    );

    // Apply the new size and offset to the box collider
    boxCollider.size = newSize;
    boxCollider.offset = newOffset;
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
    else if (other.CompareTag("Car"))
    {
      StopCar();
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
    else if (other.CompareTag("Car"))
    {
      StartCar();
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
}
