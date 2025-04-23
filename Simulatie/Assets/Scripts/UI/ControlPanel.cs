using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ControlPanel : MonoBehaviour
{
  public Button easyButton;
  public Button normalButton;
  public Button hardButton;

  public Slider timeSlider;
  public TMP_Text timeScaleText;

  public Button pauseButton;
  public Button resetButton;

  public Toggle minimapToggle;

  public TMP_Text timeText;

  public GameObject minimap;

  private Color normalColor = Color.white;
  private Color selectedColor = new Color(.8f, .75f, .75f);

  void Start()
  {
    easyButton.onClick.AddListener(() => SetSpawnMode(SpawnMode.Easy));
    normalButton.onClick.AddListener(() => SetSpawnMode(SpawnMode.Normal));
    hardButton.onClick.AddListener(() => SetSpawnMode(SpawnMode.Hard));

    timeSlider.onValueChanged.AddListener(UpdateTimeScale);

    pauseButton.onClick.AddListener(PauseSimulation);
    resetButton.onClick.AddListener(SimulationManager.Instance.ResetSimulation);

    minimapToggle.onValueChanged.AddListener(ToggleMinimap);


    // Set values to current state
    UpdateButtonVisuals();
    UpdateTimeScale(SimulationManager.Instance.GetTimeScale());
    ToggleMinimap(minimapToggle.isOn);
  }
  void Update()
  {
    timeText.text = SimulationManager.Instance.GetFormattedSimTime();
  }

  private void PauseSimulation()
  {
    SimulationManager.Instance.PauseSimulation();
    bool isPaused = SimulationManager.Instance.IsPaused();
    pauseButton.GetComponentInChildren<TMP_Text>().text = isPaused ? "Play" : "Pause";
  }

  private void UpdateTimeScale(float timescale)
	{
		SimulationManager.Instance.UpdateTimeScale(timescale);
		timeScaleText.text = Mathf.RoundToInt(timescale) + "x";
	}

  private void SetSpawnMode(SpawnMode mode)
  {
    SimulationManager.Instance.SetSpawnMode(mode);
    UpdateButtonVisuals();
  }

	private void ToggleMinimap(bool show)
	{
		minimap.SetActive(show);
	}

  private void UpdateButtonVisuals()
  {
    // Reset colors to normal and highlight active button
    Button[] buttons = { easyButton, normalButton, hardButton };
    SpawnMode currentMode = SimulationManager.Instance.GetSpawnMode();

    for (int i = 0; i < buttons.Length; i++)
    {
      buttons[i].GetComponent<Image>().color = (SpawnMode)i == currentMode ? selectedColor : normalColor;
    }
  }
}
