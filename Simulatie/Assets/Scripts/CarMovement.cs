using UnityEngine;
using System.Collections;

public class CarMovement : MonoBehaviour
{
  public Transform target;
  public float speed = 5f;
  private float currentSpeed;

  private bool isStopped = false;
  private TrafficLight currentTrafficLight;

  private Rigidbody2D rb;

  void Start()
  {
    rb = GetComponent<Rigidbody2D>();
    currentSpeed = speed;
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
      MoveTowardsTarget();
    }
  }

  void MoveTowardsTarget()
  {
    if (target == null) return;

    Vector2 direction = (target.position - transform.position).normalized;
    rb.linearVelocity = direction * currentSpeed;

    // Rotate the object to face the target
    float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
    transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));
  }

  public void StopCar()
  {
    isStopped = true;
  }
  public void StartCar()
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
