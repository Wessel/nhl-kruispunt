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

    if (owner.GetInstanceID() > otherEntity.GetInstanceID()) return;

    int ownerStrength = owner.GetStrength();
    int otherStrength = otherEntity.GetStrength();

    if (otherStrength > ownerStrength)
    {
      owner.Freeze();
    }
    else if (otherStrength < ownerStrength)
    {
      owner.Unfreeze();
    }
    else
    {
      bool ownerIsRight = owner.IsToTheRightOf(otherEntity);
      if (ownerIsRight)
      {
        owner.Unfreeze();
        otherEntity.Freeze();
      }
      else
      {
        owner.Freeze();
        otherEntity.Unfreeze();
      }
    }

    owner.SetEntityInFront(otherEntity);
  }

  private void OnTriggerExit2D(Collider2D other)
  {
    MovingEntity otherEntity = other.GetComponentInParent<MovingEntity>();
    if (otherEntity != null && otherEntity == owner.GetEntityInFront())
    {
      owner.ClearEntityInFront();
      owner.Unfreeze();
    }
  }
}
