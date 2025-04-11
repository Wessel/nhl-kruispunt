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

  public GameObject minimap;

  private Color normalColor = Color.white;
  private Color selectedColor = new Color(0.6f, 0.9f, 1f);

  void Start()
  {
    easyButton.onClick.AddListener(() => SetSpawnMode(SpawnMode.Easy));
    normalButton.onClick.AddListener(() => SetSpawnMode(SpawnMode.Normal));
    hardButton.onClick.AddListener(() => SetSpawnMode(SpawnMode.Hard));

    timeSlider.onValueChanged.AddListener(UpdateTimeScale);

    pauseButton.onClick.AddListener(SimulationManager.Instance.PauseSimulation);
    resetButton.onClick.AddListener(SimulationManager.Instance.ResetSimulation);

    minimapToggle.onValueChanged.AddListener(ToggleMinimap);


    // Set values to current state
    UpdateButtonVisuals();
    UpdateTimeScale(SimulationManager.Instance.GetTimeScale());
    ToggleMinimap(minimapToggle.isOn);
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
    // Reset kleuren
    easyButton.GetComponent<Image>().color = normalColor;
    normalButton.GetComponent<Image>().color = normalColor;
    hardButton.GetComponent<Image>().color = normalColor;

    // Highlight active button
    switch (SimulationManager.Instance.GetSpawnMode())
    {
      case SpawnMode.Easy:
        easyButton.GetComponent<Image>().color = selectedColor;
        break;
      case SpawnMode.Normal:
        normalButton.GetComponent<Image>().color = selectedColor;
        break;
      case SpawnMode.Hard:
        hardButton.GetComponent<Image>().color = selectedColor;
        break;
    }
  }
}
