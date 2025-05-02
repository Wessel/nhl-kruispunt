using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
  public GameObject prefab; // Het prefab dat we willen poolen
  public int poolSize = 10; // Aantal objecten in de pool

  private Queue<GameObject> pool = new Queue<GameObject>();

  void Start()
  {
    // Vul de pool met objecten
    for (int i = 0; i < poolSize; i++)
    {
      GameObject obj = Instantiate(prefab);
      obj.SetActive(false); // Zet het object inactief
      pool.Enqueue(obj); // Voeg het object toe aan de pool
    }
  }

  public GameObject GetObject()
  {
    if (pool.Count > 0)
    {
      GameObject obj = pool.Dequeue(); // Haal een object uit de pool
      obj.SetActive(true); // Zet het object actief
      return obj;
    }
    else
    {
      // Optioneel: maak een nieuw object aan als de pool leeg is
      GameObject obj = Instantiate(prefab);
      return obj;
    }
  }

  public void ReturnObject(GameObject obj)
  {
    obj.SetActive(false); // Zet het object inactief
    pool.Enqueue(obj); // Voeg het object terug aan de pool
  }
}
