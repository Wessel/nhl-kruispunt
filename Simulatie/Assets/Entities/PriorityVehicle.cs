using NetMQ.Sockets;
using UnityEngine;

public class PriorityVehicle: Vehicle
{
    private int priorityLevel = 1;

    public int PriorityLevel
    {
        get { return priorityLevel; }
        set { priorityLevel = value; }
    }
}
