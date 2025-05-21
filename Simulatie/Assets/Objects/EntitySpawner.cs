using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

[System.Serializable]
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
    // Zorg ervoor dat de spawnModeValues zijn ingesteld met standaardwaarden
    if (spawnModeValues.Count == 0)
    {
      InitializeDefaultSpawnModes();
    }

    var enumValues = (SpawnMode[])System.Enum.GetValues(typeof(SpawnMode));
    var existingModes = new HashSet<SpawnMode>(spawnModeValues.Select(x => x.mode));

    foreach (var mode in enumValues)
    {
      if (!existingModes.Contains(mode))
      {
        float defaultTimer = mode switch
        {
          SpawnMode.Easy => 11f,
          SpawnMode.Normal => 7f,
          SpawnMode.Hard => 4f,
          _ => 10f // Standaardwaarde voor onbekende modi
        };

        spawnModeValues.Add(new SpawnModeValue { mode = mode, spawntimer = defaultTimer });
      }
    }

    // Verwijder alle spawnModeValues die niet meer in de enum staan
    spawnModeValues.RemoveAll(x => !enumValues.Contains(x.mode));
    spawnModeValues.Sort((a, b) => a.mode.CompareTo(b.mode));
  }

  void Start()
  {
    road = GetComponent<Road>();      
    StartCoroutine(SpawnLoop());
    EventManager.Instance.SetSpawnMode.AddListener(OnSpawnModeChanged);
  }

  private void InitializeDefaultSpawnModes()
  {
    spawnModeValues.Add(new SpawnModeValue { mode = SpawnMode.Easy, spawntimer = 11f });
    spawnModeValues.Add(new SpawnModeValue { mode = SpawnMode.Normal, spawntimer = 7f });
    spawnModeValues.Add(new SpawnModeValue { mode = SpawnMode.Hard, spawntimer = 4f });
  }

  private void OnSpawnModeChanged(SpawnMode newMode)
  {
    spawnDelay = modeConfigs.GetValueOrDefault(newMode, 10f);
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
