using System.Collections.Generic;
using UnityEngine;

public class EntityPool : MonoBehaviour
{
  public GameObject prefab;
  public int poolSize;
  //public ModeSettings[] modeSettings; // Array van modusinstellingen
  //public SimulationMode currentMode; // Huidige modus

  private Queue<GameObject> pool = new Queue<GameObject>();

  void Start()
  {
    for (int i = 0; i < poolSize; i++)
    {
      GameObject entity = Instantiate(prefab);
      entity.SetActive(false);
      pool.Enqueue(entity); // Voeg entity toe aan queue
    }
  }

  public GameObject GetObject()
  {
    if (pool.Count > 0)
    {
      GameObject entity = pool.Dequeue();
      entity.SetActive(true); // Zet het object actief
      return entity;
    }
    else
    {
      return null;
    }
  }

  public void ReturnObject(GameObject entity)
  {
    entity.SetActive(false);
    pool.Enqueue(entity); //entity teruggeven aan pool
  }
}
