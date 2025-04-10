using Unity.VisualScripting;
using UnityEngine;

public class Walker : MovingEntity
{
  Animator animator;

  protected override void Awake()
  {
    base.Awake();
    type = VehicleType.Walk;
    animator = GetComponent<Animator>();
  }

  protected override void Update()
  {
    base.Update();
    animator.SetBool("isStopped", isStopped || currentSpeed == 0f);
  }
}
