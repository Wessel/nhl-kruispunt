using UnityEngine;
using UnityEngine.EventSystems;  // Needed for checking UI interactions

public class CameraMovement : MonoBehaviour
{
  public float moveSpeed = 5f;  // Speed of camera movement
  public float zoomSpeed = 2f;  // Speed of zooming
  public float minZoom = 2f;    // Minimum zoom level
  private float maxZoom;        // Maximum zoom level (dynamic)

  private Bounds parentBounds;
  private Camera cam;
  private float camHeight, camWidth;
  private Vector3 dragOrigin;

  private void Start()
  {
    cam = GetComponent<Camera>();
    if (cam == null || !cam.orthographic) return;
    if (transform.parent == null) return;

    BoxCollider2D parentCollider = transform.parent.GetComponent<BoxCollider2D>();
    if (parentCollider != null)
    {
      parentBounds = parentCollider.bounds;
      maxZoom = Mathf.Min(parentBounds.size.x / (2 * cam.aspect), parentBounds.size.y / 2); // Prevents zooming too far out
      cam.orthographicSize = Mathf.Clamp(cam.orthographicSize, minZoom, maxZoom);
      UpdateCameraSize();
    }
  }

  private void UpdateCameraSize()
  {
    camHeight = cam.orthographicSize;
    camWidth = camHeight * cam.aspect;
  }

  private void Update()
  {
    if (transform.parent == null) return;

    // Don't move the camera if the mouse is over UI elements
    if (EventSystem.current.IsPointerOverGameObject()) return;

    // Handle Zooming (Mouse Scroll)
    float scroll = Input.GetAxis("Mouse ScrollWheel");
    if (scroll != 0)
    {
      cam.orthographicSize -= scroll * zoomSpeed;
      cam.orthographicSize = Mathf.Clamp(cam.orthographicSize, minZoom, maxZoom);
      UpdateCameraSize();
    }

    // Handle Mouse Dragging
    if (Input.GetMouseButtonDown(0))
    {
      dragOrigin = cam.ScreenToWorldPoint(Input.mousePosition);
    }
    if (Input.GetMouseButton(0))
    {
      Vector3 difference = dragOrigin - cam.ScreenToWorldPoint(Input.mousePosition);
      transform.position += difference;
    }

    // Handle WASD Movement
    float moveX = Input.GetAxis("Horizontal");
    float moveY = Input.GetAxis("Vertical");
    Vector3 move = new Vector3(moveX, moveY, 0) * moveSpeed * Time.deltaTime;
    transform.position += move;

    // Clamp position so the whole camera stays inside bounds
    Vector3 clampedPosition = transform.position;
    clampedPosition.x = Mathf.Clamp(clampedPosition.x, parentBounds.min.x + camWidth, parentBounds.max.x - camWidth);
    clampedPosition.y = Mathf.Clamp(clampedPosition.y, parentBounds.min.y + camHeight, parentBounds.max.y - camHeight);

    transform.position = clampedPosition;
  }
}
