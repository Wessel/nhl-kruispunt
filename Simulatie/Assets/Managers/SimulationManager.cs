using UnityEngine;



public class SimulationManager : MonoBehaviour
{
  public static SimulationManager Instance;

  private float timeScale = 1f;
  private bool isPaused = false;
  private SpawnMode spawnMode = SpawnMode.Easy;

  void Awake()
  {
    if (Instance == null) Instance = this;
    else Destroy(gameObject);
  }

  void Update()
  {
    Time.timeScale = isPaused ? 0f : timeScale;
  }

  public SpawnMode GetSpawnMode()
  {
    return spawnMode;
  }

  public void SetSpawnMode(SpawnMode mode)
  {
    spawnMode = mode;
  }

  public float GetCurrentSpawnRate()
  {
    return 0f;
  }

  public void UpdateTimeScale(float newTimeScale)
  {
    timeScale = newTimeScale;
    Debug.Log("Time scale updated to: " + timeScale);
  }

  public float GetTimeScale()
  {
    return timeScale;
  }

  public void ResetSimulation()
  {
    Debug.Log("Simulation Reset!");
  }
  public void PauseSimulation()
  {
    Debug.Log("Simulation paused!");
  }
}
