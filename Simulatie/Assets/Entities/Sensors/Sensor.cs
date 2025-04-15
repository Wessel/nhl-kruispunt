using System.Collections.Generic;
using UnityEngine;

public class Sensor : MonoBehaviour
{
  public string id;
  private bool isActive = false;
  private HashSet<Collider2D> objectsInside = new HashSet<Collider2D>();

  void OnTriggerEnter2D(Collider2D other)
  {
    if (!objectsInside.Contains(other))
    {
      objectsInside.Add(other);
      if (objectsInside.Count == 1)
      {
        isActive = true;
      }
    }
  }

  void OnTriggerExit2D(Collider2D other)
  {
    if (objectsInside.Contains(other))
    {
      objectsInside.Remove(other);
      if (objectsInside.Count == 0)
      {
        isActive = false;
      }
    }
  }

  public bool IsActive()
  {
    return isActive;
  }
}
