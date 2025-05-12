using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SpawnChance
{
  public GameObject prefab;
  public int chance;
}

public class EntityPool : MonoBehaviour
{
  public List<SpawnChance> prefabs;
  public int poolSize;

  private Queue<MovingEntity> pool = new Queue<MovingEntity>();
  private List<MovingEntity> activeEntities = new List<MovingEntity>();

  void Start()
  {
    EventManager.Instance.Reset.AddListener(ResetPool);
    int totalWeight = 0;
    foreach (SpawnChance entry in prefabs)
    {
      totalWeight += entry.chance;
    }

    for (int i = 0; i < poolSize; i++)
    {
      int rand = UnityEngine.Random.Range(1, totalWeight + 1);
      int cumulative = 0;
      GameObject selectedPrefab = null;

      foreach (SpawnChance entry in prefabs)
      {
        cumulative += entry.chance;
        if (rand <= cumulative)
        {
          selectedPrefab = entry.prefab;
          break;
        }
      }

      if (selectedPrefab == null) continue;

      GameObject obj = Instantiate(selectedPrefab, transform);
      obj.SetActive(false);

      MovingEntity entity = obj.GetComponent<MovingEntity>();
      if (entity != null)
      {
        pool.Enqueue(entity);
      }
    }
  }

  public MovingEntity GetObject()
  {
    if (pool.Count > 0)
    {
      MovingEntity entity = pool.Dequeue();
      entity.gameObject.SetActive(true);
      activeEntities.Add(entity);
      return entity;
    }

    return null;
  }

  public void ReturnObject(MovingEntity entity)
  {
    entity.gameObject.SetActive(false);
    activeEntities.Remove(entity);
    pool.Enqueue(entity);
  }

  public void ResetPool()
  {
    foreach (MovingEntity entity in activeEntities.ToArray())
    {
      ReturnObject(entity);
    }
  }
}
