using UnityEngine;

public class SimulationManager : MonoBehaviour
{
	public static SimulationManager Instance;

  private PriorityVehicleQueue priorityVehicleQueue = new PriorityVehicleQueue();
  private float simulationTime; // In seconds
  private float timeScale = 1f;
	private bool isPaused = false;
	private SpawnMode spawnMode = SpawnMode.Easy;
	private float nextEventTime = 0f;

  private void Awake()
	{
		if (Instance == null) Instance = this;
		else Destroy(gameObject);

		ZeroMQConfig.LoadConfig();
  }

  private void Start()
  {
    EventManager.Instance.EnqueuePriorityVehicle.AddListener(priorityVehicleQueue.Enqueue);
    EventManager.Instance.DequeuePriorityVehicle.AddListener(priorityVehicleQueue.Dequeue);
  }


  private void Update()
	{
		Time.timeScale = isPaused ? 0f : timeScale;

		if (!isPaused)
		{
			simulationTime += Time.deltaTime;

			if (simulationTime >= nextEventTime)
			{
				nextEventTime += 0.08f; // Schedule the next event at 90ms intervals
				EventManager.Instance.SendSimulationTime.Invoke(simulationTime);
			}
		}
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

	public SpawnMode GetSpawnMode() => spawnMode;

	public void SetSpawnMode(SpawnMode mode)
	{
		spawnMode = mode;
    EventManager.Instance.SetSpawnMode.Invoke(mode);
  }

	public void UpdateTimeScale(float newTimeScale)
	{
		timeScale = newTimeScale;
	}

	public float GetTimeScale() => timeScale;

	public void ResetSimulation()
	{
		simulationTime = 0f;
		nextEventTime = 0f;
    EventManager.Instance.Reset.Invoke();
  }

	public bool IsPaused() => isPaused;

	public void PauseSimulation()
	{
		isPaused = !isPaused;
	}

  public float GetSimulationTime() => simulationTime;
}
