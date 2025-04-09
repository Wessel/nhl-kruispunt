using UnityEngine;

public class CarSensor : MonoBehaviour
{
  public Car ownerCar;

  private void Awake()
  {
     ownerCar = GetComponentInParent<Car>();
  }

  private void OnTriggerEnter2D(Collider2D other)
  {
    Car otherCar = other.GetComponentInParent<Car>();
    if (otherCar != null && otherCar != ownerCar)
    {
      ownerCar.SetCarInFront(otherCar);
    }
  }

  private void OnTriggerExit2D(Collider2D other)
  {
    Car otherCar = other.GetComponentInParent<Car>();
    if (otherCar != null && otherCar == ownerCar.GetCarInFront())
    {
      ownerCar.ClearCarInFront();
    }
  }
}
