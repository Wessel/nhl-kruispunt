using UnityEngine;
using System.Collections;

public class TrafficLight : MonoBehaviour
{
  public LightState currentLight;

  private SpriteRenderer spriteRenderer;
  public Sprite redLight;
  public Sprite yellowLight;
  public Sprite greenLight;

  private void Start()
  {
    spriteRenderer = GetComponent<SpriteRenderer>();
  }
  public void SetLight(LightState newState)
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
