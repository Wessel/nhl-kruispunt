using NetMQ.Sockets;
using UnityEngine;

public class PriorityVehicle: Vehicle
{
  [SerializeField] private int priorityLevel = 1;
  private int simulationTimeMs = 0;

  public int PriorityLevel
  {
    get { return priorityLevel; }
    set { priorityLevel = value; }
  }

  public int SimulationTimeMs
  {
    get { return simulationTimeMs; }
    set { simulationTimeMs = value; }
  }

  public override void SwitchToRoad(Road newRoad, float startDistance = 0)
  {
    base.SwitchToRoad(newRoad, startDistance);
    simulationTimeMs = (int)(SimulationManager.Instance.GetSimulationTime() * 1000);
    EventManager.Instance.EnqueuePriorityVehicle.Invoke(this);
  }
}
