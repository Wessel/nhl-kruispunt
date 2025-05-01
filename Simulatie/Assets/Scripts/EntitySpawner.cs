using UnityEngine;
using System.Collections;

public class EntitySpawner : MonoBehaviour
{
  // Start is called once before the first execution of Update after the MonoBehaviour is created

  public GameObject[] entityPrefabs;
  public Transform spawnPoint;
  public Transform parentTransform;
  public Road[] availableRoads; 
  void Start()
  {
    SpawnEntity();
  }

    // Update is called once per frame
  void Update()
  {
    
  }

  void SpawnEntity()
  {
    int randomIndex = Random.Range(0, entityPrefabs.Length);
    GameObject newEntity = Instantiate(entityPrefabs[randomIndex], spawnPoint.position, spawnPoint.rotation, parentTransform);

    // Verkrijg de MovingEntity component van de nieuw gespawnde entiteit
    MovingEntity movingEntity = newEntity.GetComponent<MovingEntity>();
    if (movingEntity != null && availableRoads.Length > 0)
    {
      // Kies een willekeurige weg uit de beschikbare wegen
      Road randomRoad = availableRoads[Random.Range(0, availableRoads.Length)];
      movingEntity.SetNewRoad(randomRoad); // Stel de weg in voor de MovingEntity
    }
  }
}
