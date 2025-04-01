using UnityEngine;
using UnityEngine.Splines;
using Unity.Mathematics;

public class CarMovement : MonoBehaviour
{
  public SplineContainer road;
  public float speed = 5f;

  private bool isStopped = false;
  private TrafficLight currentTrafficLight;

  private float t = 0f;
  private Rigidbody2D rb;

  void Start()
  {
    rb = GetComponent<Rigidbody2D>();
  }

  void Update()
  {
    CheckTrafficLight();
    if (isStopped)
    {
      rb.linearVelocity = Vector2.zero;
    }
    else
    {
      MoveOnRoad();
    }
  }

  private void MoveOnRoad()
  {
    // Move along the spline
    t += (speed / road.Spline.GetLength()) * Time.deltaTime;

    // Get position and tangent along the spline
    Vector3 position = road.EvaluatePosition(t);
    float3 tangent = road.EvaluateTangent(t);

    // Calculate the angle in degrees
    float angle = Mathf.Atan2(tangent.y, tangent.x) * Mathf.Rad2Deg;

    // Apply position and rotation using Rigidbody
    rb.MovePosition(position);
    rb.MoveRotation(angle);
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
}
