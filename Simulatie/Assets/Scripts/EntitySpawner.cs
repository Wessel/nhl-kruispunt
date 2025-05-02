using UnityEngine;
using System.Collections;

public class EntitySpawner : MonoBehaviour
{
  // Start is called once before the first execution of Update after the MonoBehaviour is created

  //public ModeSettings[] modeSettings;
  //public SimulationMode currentMode;

  public Transform spawnPoint;
  public EntityPool entityPool;
  //[SerializeField] private int numberOfEntitiesToSpawn;
  [SerializeField] private Road[] availableRoads;

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
    while (true) // Blijf entiteiten spawnen
    {
      //for (int i = 0; i < numberOfEntitiesToSpawn; i++)
      //{
        GameObject entity = entityPool.GetObject(); // Haal een object uit de pool
        if (entity != null)
        {
          entity.transform.position = spawnPoint.position; // Plaats het object op het spawnpunt
          entity.transform.rotation = spawnPoint.rotation; // Stel de rotatie in
          entity.transform.SetParent(spawnPoint);

          // Initialiseer de MovingEntity met de object pool
          MovingEntity movingEntity = entity.GetComponent<MovingEntity>();

          if (movingEntity != null)
          {
            movingEntity.Initialize(entityPool); // Geef de object pool door
          }

          //Kies een willekeurige weg uit de beschikbare wegen
          Road randomRoad = availableRoads[Random.Range(0, availableRoads.Length)];
          movingEntity.SetNewRoad(randomRoad); // Stel de weg in voor de MovingEntity
        }
        else
        {
          yield return new WaitForSeconds(1f); // Wacht een seconde voordat je het opnieuw probeert
        }
      //}

      yield return new WaitForSeconds(1f); // Wacht een seconde tussen spawns
    }
  }
}
