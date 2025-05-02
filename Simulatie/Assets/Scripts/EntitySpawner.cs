using UnityEngine;
using System.Collections;

public class EntitySpawner : MonoBehaviour
{
  // Start is called once before the first execution of Update after the MonoBehaviour is created

  //public ModeSettings[] modeSettings;
  //public SimulationMode currentMode;

  public GameObject[] entityPrefabs;
  public Transform spawnPoint;
  [SerializeField] private float spawnInterval;
  [SerializeField] private int numberOfEntitiesToSpawn;

  public Road[] availableRoads;
  void Start()
  {
    //SetSpawnParameters();
    StartCoroutine(SpawnEntities());
  }

  // Update is called once per frame
  void Update()
  {

  }
  //void SetSpawnParameters()
  //{
  //  foreach (var settings in modeSettings)
  //  {
  //    if (settings.mode == currentMode)
  //    {
  //      spawnInterval = settings.spawnInterval;
  //      numberOfEntitiesToSpawn = settings.numberOfEntitiesToSpawn;
  //      break;
  //    }
  //  }
  //}

  IEnumerator SpawnEntities()
  {
    for (int i = 0; i < numberOfEntitiesToSpawn; i++)
    {
      SpawnEntity();
      yield return new WaitForSeconds(spawnInterval); // Wacht voor de gespecificeerde tijd
    }
  }
  void SpawnEntity()
  {
    int randomIndex = Random.Range(0, entityPrefabs.Length);
    GameObject newEntity = Instantiate(entityPrefabs[randomIndex], spawnPoint.position, spawnPoint.rotation, spawnPoint);

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
