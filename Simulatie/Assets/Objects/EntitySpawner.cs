using UnityEngine;
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
  private float nextSpawnSimTime = 0f;

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
    Array enumValues = (SpawnMode[])Enum.GetValues(typeof(SpawnMode));

    foreach (SpawnMode mode in enumValues)
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
    EventManager.Instance.SetSpawnMode.AddListener(OnSpawnModeChanged);
    ResetSpawnTimer();
  }

  void Update()
  {
    if (SimulationManager.Instance.IsPaused()) return;

    float simTime = SimulationManager.Instance.GetSimulationTime();

    if (simTime >= nextSpawnSimTime && CanSpawnAtStart())
    {
      SpawnEntity();
      ResetSpawnTimer();
    }
  }

  private void OnSpawnModeChanged(SpawnMode newMode)
  {
    ResetSpawnTimer();
  }

  private void ResetSpawnTimer()
  {
    float simTime = SimulationManager.Instance.GetSimulationTime();
    float delay = modeConfigs.TryGetValue(SimulationManager.Instance.GetSpawnMode(), out float val) ? val : 10f;
    nextSpawnSimTime = simTime + delay;
  }

  private bool CanSpawnAtStart()
  {
    Vector3 spawnPosition = road.GetNewPosition(0f);
    Vector2 halfExtents = new Vector2(1f, 1f);
    LayerMask vehicleLayer = LayerMask.GetMask("Vehicles");

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
