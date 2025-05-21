using UnityEngine;
using System.Collections;

public class BridgeLight : TrafficLight
{
  public override void SetLight(LightState newState)
  {
    if (newState == LightState.Green && currentLight != LightState.Green)
    {
      StartCoroutine(ShowOrangeThenGreen());
    }
    else
    {
      currentLight = newState;
      switch (newState)
      {
        case LightState.Red:
          spriteRenderer.sprite = redLight;
          break;
        case LightState.Orange:
          spriteRenderer.sprite = orangeLight;
          break;
        case LightState.Green:
          spriteRenderer.sprite = greenLight;
          break;
      }
    }
  }

  private IEnumerator ShowOrangeThenGreen()
  {
    currentLight = LightState.Orange;
    spriteRenderer.sprite = orangeLight;
    yield return new WaitForSeconds(3f);
    currentLight = LightState.Green;
    spriteRenderer.sprite = greenLight;
  }
}
