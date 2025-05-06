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

  void Start()
  {
    if (prefabs == null || prefabs.Count == 0)
    {
      Debug.LogError("No prefabs defined in the pool.");
      return;
    }

    // Maak een lijst met cumulatieve kansen
    int totalWeight = 0;
    foreach (SpawnChance entry in prefabs)
    {
      totalWeight += entry.chance;
    }

    for (int i = 0; i < poolSize; i++)
    {
      // Genereer een willekeurig getal tussen 1 en totalWeight
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

      if (selectedPrefab == null)
      {
        Debug.LogError("Failed to select a prefab.");
        continue;
      }

      GameObject obj = Instantiate(selectedPrefab, transform);
      obj.SetActive(false);

      MovingEntity entity = obj.GetComponent<MovingEntity>();
      if (entity != null)
      {
        pool.Enqueue(entity);
      }
      else
      {
        Debug.LogError("Selected prefab does not have a MovingEntity component.");
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
