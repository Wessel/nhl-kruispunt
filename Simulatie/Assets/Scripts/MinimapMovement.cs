using UnityEngine;

public class MinimapController : MonoBehaviour
{
  private Camera mainCamera;
  private BoxCollider2D boxCollider;
  private Camera minimapCam;
  private Bounds bounds;

  private float minimapHalfWidth, minimapHalfHeight;

  private void Start()
  {
    minimapCam = GetComponent<Camera>();
    mainCamera = transform.parent.GetComponentInChildren<Camera>();
    boxCollider = transform.parent.GetComponent<BoxCollider2D>();

    if (mainCamera == null)
    {
      mainCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
    }

    if (boxCollider != null)
    {
      bounds = boxCollider.bounds;
    }

    UpdateMinimapSize();
  }

  private void UpdateMinimapSize()
  {
    if (minimapCam != null)
    {
      minimapHalfHeight = minimapCam.orthographicSize;
      minimapHalfWidth = minimapHalfHeight * minimapCam.aspect;
    }
  }

  private void LateUpdate()
  {
    if (mainCamera == null || boxCollider == null)
      return;

    UpdateMinimapSize();

    Vector3 mainCamPos = mainCamera.transform.position;

    float xMin = bounds.min.x + minimapHalfWidth;
    float xMax = bounds.max.x - minimapHalfWidth;
    float yMin = bounds.min.y + minimapHalfHeight;
    float yMax = bounds.max.y - minimapHalfHeight;

    float clampedX = Mathf.Clamp(mainCamPos.x, xMin, xMax);
    float clampedY = Mathf.Clamp(mainCamPos.y, yMin, yMax);

    minimapCam.transform.position = new Vector3(clampedX, clampedY, mainCamPos.z);
  }
}
