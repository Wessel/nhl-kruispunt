using UnityEngine;

public class MovingEntitySensor : MonoBehaviour
{
  private MovingEntity owner;

  private void Awake()
  {
    owner = GetComponentInParent<MovingEntity>();
  }

  private void OnTriggerEnter2D(Collider2D other)
  {
    MovingEntity otherEntity = other.GetComponentInParent<MovingEntity>();
    if (otherEntity == null || otherEntity == owner) return;

    // Check if the other entity is roughly ahead
    Vector2 toOther = otherEntity.transform.position - owner.transform.position;
    float dot = Vector2.Dot(owner.transform.right, toOther.normalized);

    if (dot > 0.5f) // Ensure it's in front, not side or behind
    {
      owner.ResolveEncounterWith(otherEntity);
    }
  }


  private void OnTriggerExit2D(Collider2D other)
  {
    MovingEntity otherEntity = other.GetComponentInParent<MovingEntity>();
    if (otherEntity != null && otherEntity == owner.GetEntityInFront())
    {
      owner.ClearEntityInFront();
      if (!owner.IsBlockedAhead())
      {
        owner.Unfreeze();
      }

    }
  }
}
