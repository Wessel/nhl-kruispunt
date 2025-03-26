using UnityEngine;
using System.Collections;

public class TrafficLight : MonoBehaviour
{
  public enum LightState { Red, Yellow, Green }
  public LightState currentLight;

  private SpriteRenderer spriteRenderer;
  public Sprite redLight;
  public Sprite yellowLight;
  public Sprite greenLight;

  public float redTime = 5f;
  public float yellowTime = 1f;
  public float greenTime = 5f;

  private void Start()
  {
    spriteRenderer = GetComponent<SpriteRenderer>();
    StartCoroutine(TrafficCycle());
  }

  IEnumerator TrafficCycle()
  {
    while (true)
    {
      SetLight(LightState.Red);
      yield return new WaitForSeconds(redTime);

      SetLight(LightState.Yellow);
      yield return new WaitForSeconds(yellowTime);

      SetLight(LightState.Green);
      yield return new WaitForSeconds(greenTime);
    }
  }

  void SetLight(LightState newState)
  {
    currentLight = newState;
    switch (newState)
    {
      case LightState.Red:
        spriteRenderer.sprite = redLight;
        break;
      case LightState.Yellow:
        spriteRenderer.sprite = yellowLight;
        break;
      case LightState.Green:
        spriteRenderer.sprite = greenLight;
        break;
    }
  }
}
