using UnityEngine;
using System.Collections;

public class EntitySpawner : MonoBehaviour
{
  public RoadConfig config;             
  public EntityPool entityPool;          

  private Road road;                     
  private float spawnTimer;              

  void Start()
  {
    road = GetComponent<Road>();      
    spawnTimer = config.spawnInterval;
    config = new RoadConfig()
    {
      mode = SpawnMode.Easy,
      spawnInterval = 3.0f,
    }; 

    StartCoroutine(SpawnLoop());
  }

  IEnumerator SpawnLoop()
  {
    while (!SimulationManager.Instance.IsPaused())
    {
      spawnTimer -= Time.deltaTime;
      if (spawnTimer <= 0f && CanSpawnAtStart())
      {
        SpawnEntity();
        spawnTimer = config.spawnInterval;
      }

      yield return null;
    }
  }

  private bool CanSpawnAtStart()
  {
    Vector3 spawnPosition = road.GetNewPosition(0f);

    Vector2 halfExtents = new Vector2(0.1f, 0.1f);

    LayerMask vehicleLayer = LayerMask.GetMask("Vehicles");

    // Check if any collider overlaps the spawn area
    Collider2D hit = Physics2D.OverlapBox(spawnPosition, halfExtents, 0f, vehicleLayer);
    return hit == null;
  }

  private void SpawnEntity()
  {
    MovingEntity entity = entityPool.GetObject();
    if (entity != null)
    {
      entity.Initialize(entityPool);
      entity.SwitchToRoad(road);
    }
  }
}
