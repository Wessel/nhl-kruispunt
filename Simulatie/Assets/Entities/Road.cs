using UnityEngine;
using UnityEngine.Splines;
using Unity.Mathematics;
using System.Collections.Generic;

public class Road : MonoBehaviour
{
  private SplineContainer splineContainer;
  [SerializeField] private List<VehicleType> supportedTypes;

  private float cachedLength = -1f;

  private void Awake()
  {
    if (splineContainer == null)
    {
      splineContainer = GetComponent<SplineContainer>();
    }
  }

  public Vector3 GetNewPosition(float t)
  {
    return splineContainer.EvaluatePosition(t);
  }

  public float3 GetNewTangent(float t)
  {
    return splineContainer.EvaluateTangent(t);
  }

  public float GetLength()
  {
    if (cachedLength < 0f)
    {
      cachedLength = splineContainer.Spline.GetLength();
    }
    return cachedLength;
  }

  public List<VehicleType> GetVehicleTypes()
  {
    return supportedTypes;
  }


  public float GetClosestDistanceOnSpline()
  {
    Vector3 localPosition = splineContainer.transform.InverseTransformPoint(transform.position);
    SplineUtility.GetNearestPoint(splineContainer.Spline, localPosition, out _, out float t);
    return t;
  }


  public Vector3 GetPointOnSpline(float t)
  {
    Vector3 localPoint = splineContainer.Spline.EvaluatePosition(t);
    return splineContainer.transform.TransformPoint(localPoint);
  }

  public string GetName()
  {
    return gameObject.name;
  }
}
