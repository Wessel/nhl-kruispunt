using UnityEngine;
using UnityEngine.Splines;

public class Road : MonoBehaviour
{
  public RoadType roadType;
  private SplineContainer spline;
  void Awake()
  {
    spline = GetComponent<SplineContainer>();
  }
  public float getLength()
  {
    return spline.Spline.GetLength();
  }

  public Vector3 getNewPosition(float distance)
  {
    return spline.EvaluatePosition(distance);
  }

  public Vector3 getNewTangent(float distance)
  {
    return spline.EvaluateTangent(distance);
  }

  public RoadType getRoadType()
  {
    return roadType;
  }
}
