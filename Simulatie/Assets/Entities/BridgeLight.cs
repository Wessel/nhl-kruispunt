using UnityEngine;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;

public class BridgeLight : TrafficLight
{
  BridgeState bridgeState;

  protected override void Start()
  {
    base.Start();
    EventManager.Instance.BridgeStateChanged.AddListener(OnBridgeStateChanged);
  }

  public override void SetLight(LightState newState)
  {
    if (bridgeState == BridgeState.Unknown) return;

    // Only allow green if the bridge is closed
    if (newState == LightState.Green)
    {
      if (bridgeState == BridgeState.Closed)
      {
        currentLight = LightState.Green;
        spriteRenderer.sprite = greenLight;
      }
      else
      {
        currentLight = LightState.Orange;
        spriteRenderer.sprite = orangeLight;
      }
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
      }
    }
  }

  private void OnBridgeStateChanged(BridgeState bridgeState)
  {
    this.bridgeState = bridgeState;
    if (bridgeState == BridgeState.Closed)
    {
      StopAllCoroutines();
      StartCoroutine(TransitionToGreenAfterClosed());
    }
    else if (bridgeState == BridgeState.Open)
    {
      SetLight(LightState.Red);
    }
  }


  private System.Collections.IEnumerator TransitionToGreenAfterClosed()
  {
    SetLight(LightState.Orange);
    yield return new WaitForSeconds(3f);
    Debug.Log($"Transitioning to green after closed {bridgeState}");
    SetLight(LightState.Green);
  }
}
