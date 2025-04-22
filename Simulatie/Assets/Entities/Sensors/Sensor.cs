using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Sensor : MonoBehaviour
{
  private string id;
  private bool isActive = false;
  private HashSet<Collider2D> objectsInside = new HashSet<Collider2D>();

  public UnityEvent onStateChanged = new();

  private void Start()
  {
    id = gameObject.name;
  }

  //Simulate fake traffic light changes
  private void Update()
  {
    if (Random.Range(1, 8000) == 1)
    {
      ChangeState();
    }
  }

  private void ChangeState()
  {
    isActive = !isActive;
    onStateChanged.Invoke();
  }

  private void OnTriggerEnter2D(Collider2D other)
  {
    if (!objectsInside.Contains(other))
    {
      objectsInside.Add(other);
      if (objectsInside.Count == 1)
      {
        ChangeState();
      }
    }
  }

  private void OnTriggerExit2D(Collider2D other)
  {
    if (objectsInside.Contains(other))
    {
      objectsInside.Remove(other);
      if (objectsInside.Count == 0)
      {
        ChangeState();
      }
    }
  }

  public bool IsActive() => isActive;
  public string GetID() => id;
}
