using UnityEngine;

public class MovingEntitySensor : MonoBehaviour
{
  public MovingEntity owner;

  private void Awake()
  {
     owner = GetComponentInParent<MovingEntity>();
  }

  private void OnTriggerEnter2D(Collider2D other)
  {
    MovingEntity otherMovingEntity = other.GetComponentInParent<MovingEntity>();
    if (otherMovingEntity != null && otherMovingEntity != owner)
    {
      owner.SetEntityInFront(otherMovingEntity);
    }
  }

  private void OnTriggerExit2D(Collider2D other)
  {
    MovingEntity otherMovingEntity = other.GetComponentInParent<MovingEntity>();
    if (otherMovingEntity != null && otherMovingEntity == owner.GetEntityInFront())
    {
      owner.ClearEntityInFront();
    }
  }
}
