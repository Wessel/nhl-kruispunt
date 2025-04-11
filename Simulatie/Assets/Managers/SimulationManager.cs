using UnityEngine;



public class SimulationManager : MonoBehaviour
{
  public static SimulationManager Instance;
  public float simulationTime; // In seconds

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
    simulationTime += Time.deltaTime * timeScale;
  }

  public string GetFormattedSimTime()
  {
    int totalMilliseconds = Mathf.FloorToInt(simulationTime * 1000);

    int hours = totalMilliseconds / (3600 * 1000);
    int minutes = (totalMilliseconds % (3600 * 1000)) / (60 * 1000);
    int seconds = (totalMilliseconds % (60 * 1000)) / 1000;
    int milliseconds = totalMilliseconds % 1000;

    return $"{hours:D2}:{minutes:D2}:{seconds:D2}.{milliseconds:D3}";
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
    simulationTime = 0f;
  }
  public void PauseSimulation()
  {
    isPaused = !isPaused;
  }
}
