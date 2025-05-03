using UnityEngine;
using System.Collections;

public class EntitySpawner : MonoBehaviour
{
  // Start is called once before the first execution of Update after the MonoBehaviour is created

  public ModeSettings[] modeSettings;
  public SpawnMode currentMode;

  public Transform spawnPoint;
  public EntityPool entityPool;
  [SerializeField] private Road[] availableRoads;

  void Start()
  {
    StartCoroutine(SpawnEntities());
  }

  // Update is called once per frame
  void Update()
  {
  }
  public void ChangeSpawnMode(SpawnMode newMode)
  {
    currentMode = newMode; // Wijzig huidige modus
  }

  IEnumerator SpawnEntities()
  {
    while (true) // Blijf entiteiten spawnen
    {
      int numberOfEntitiesToSpawn = GetNumberOfEntitiesToSpawn();
      for (int i = 0; i < numberOfEntitiesToSpawn; i++)
      {
        GameObject entity = entityPool.GetObject(); // Haal een object uit de pool
        if (entity != null)
        {
          entity.transform.position = spawnPoint.position; // Plaats op spawnpunt
          entity.transform.rotation = spawnPoint.rotation; 
          entity.transform.SetParent(spawnPoint); //entities worden kinderen spawnpunt voor overzicht

          // Initialiseer de MovingEntity met de object pool
          MovingEntity movingEntity = entity.GetComponent<MovingEntity>();

          if (movingEntity != null)
          {
            movingEntity.Initialize(entityPool); // Geef de object pool door
          }

          // Kies willekeurige weg uit wegen
          Road randomRoad = availableRoads[Random.Range(0, availableRoads.Length)];
          movingEntity.SetNewRoad(randomRoad); // Stel weg in 
        }
        else
        {
          yield return new WaitForSeconds(1f);
        }
      }

      float spawnInterval = GetSpawnInterval(); // Haal spawnInterval op
      yield return new WaitForSeconds(spawnInterval);
    }
  }

  private float GetSpawnInterval()
  {
    foreach (var settings in modeSettings)
    {
      if (settings.mode == currentMode)
      {
        return settings.spawnInterval;
      }
    }
    return 1f; //standaard als modus niet is gevonden
  }

  private int GetNumberOfEntitiesToSpawn()
  {
    foreach (var settings in modeSettings)
    {
      if (settings.mode == currentMode)
      {
        return settings.numberOfEntitiesToSpawn;
      }
    }
    return 1; // Standaard als modus niet is gevonden
  }
}
