using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

[Serializable]
public class SpawnModeValue
{
  public SpawnMode mode;
  public float spawntimer;
}

public class EntitySpawner : MonoBehaviour
{
  public List<SpawnModeValue> spawnModeValues;
  public EntityPool entityPool;          

  private Road road;                   
  private Dictionary<SpawnMode, float> modeConfigs;
  private float spawnDelay = 0f;

  private void Awake()
  {
    modeConfigs = new Dictionary<SpawnMode, float>();
    foreach (SpawnModeValue entry in spawnModeValues)
    {
      modeConfigs[entry.mode] = entry.spawntimer;
    }
  }

  private void OnValidate()
  {
    var enumValues = (SpawnMode[])Enum.GetValues(typeof(SpawnMode));

    foreach (var mode in enumValues)
    {
      if (!spawnModeValues.Exists(x => x.mode == mode))
      {
        spawnModeValues.Add(new SpawnModeValue { mode = mode });
      }
    }

    spawnModeValues.RemoveAll(x => Array.IndexOf(enumValues, x.mode) == -1);
    spawnModeValues.Sort((a, b) => a.mode.CompareTo(b.mode));
  }

  void Start()
  {
    road = GetComponent<Road>();      
    StartCoroutine(SpawnLoop());
    EventManager.Instance.SetSpawnMode.AddListener(OnSpawnModeChanged);
  }

  private void OnSpawnModeChanged(SpawnMode newMode)
  {
    spawnDelay = modeConfigs.TryGetValue(SimulationManager.Instance.GetSpawnMode(), out float value) ? value : 10f;
  }

  IEnumerator SpawnLoop()
  {
    float spawnTimer = 0f;
    while (!SimulationManager.Instance.IsPaused())
    {
      spawnTimer -= Time.deltaTime;
      if (spawnTimer <= 0f && CanSpawnAtStart())
      {
        SpawnEntity();
        spawnTimer = spawnDelay;
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
