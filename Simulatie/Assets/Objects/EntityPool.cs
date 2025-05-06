using System.Collections.Generic;
using UnityEngine;

public class EntityPool : MonoBehaviour
{
  public GameObject prefab;
  public int poolSize;

  private Queue<MovingEntity> pool = new Queue<MovingEntity>();

  void Start()
  {
    for (int i = 0; i < poolSize; i++)
    {
      GameObject obj = Instantiate(prefab, transform);
      obj.SetActive(false);

      MovingEntity entity = obj.GetComponent<MovingEntity>();
      if (entity != null)
      {
        pool.Enqueue(entity);
      }
      else
      {
        Debug.LogError("Prefab does not have a MovingEntity component.");
      }
    }
  }

  public MovingEntity GetObject()
  {
    if (pool.Count > 0)
    {
      MovingEntity entity = pool.Dequeue();
      entity.gameObject.SetActive(true);
      return entity;
    }

    return null;
  }

  public void ReturnObject(MovingEntity entity)
  {
    entity.gameObject.SetActive(false);
    pool.Enqueue(entity);
  }
}
