using UnityEngine;

public class TacticalCamera : MonoBehaviour
{
    [Header("Рух камери (WASD + Перетягування мишкою)")]
    public float keyboardPanSpeed = 10f;
    public float mouseDragSensitivity = 1f; // Чутливість перетягування мишкою
    public float smoothTime = 0.12f;        // Плавність інерції

    [Header("Межі карти (Рамка в яких ходить камера)")]
    public Vector2 mapLimitsX = new Vector2(-12f, 12f);
    public Vector2 mapLimitsY = new Vector2(-8f, 8f);

    [Header("Зум коліщатком (Множники від стартового розміру)")]
    public float minZoomMult = 0.8f; // 0.8x (Приближення)
    public float maxZoomMult = 1.2f; // 1.2x (Віддалення)
    public float zoomStep = 2f;
    public float zoomSmoothness = 8f;

    private Camera cam;
    private float baseOrthoSize;
    private float targetZoom;

    private Vector3 targetPosition;
    private Vector3 moveVelocity;
    private Vector3 lastMouseScreenPos; // Зберігаємо екранні пікселі замість світових координат!
    private bool isDraggingMap = false;

    private void Start()
    {
        cam = GetComponent<Camera>();
        baseOrthoSize = cam.orthographicSize;
        targetZoom = baseOrthoSize;
        targetPosition = transform.position;
    }

    private void LateUpdate()
    {
        HandleZoom();
        HandleMovement();
        ClampTargetToBounds();

        // Плавне доведення камери до цільової точки та зуму
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref moveVelocity, smoothTime);
        cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, targetZoom, Time.deltaTime * zoomSmoothness);
    }

    private void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.001f)
        {
            targetZoom -= scroll * zoomStep;
            float minSize = baseOrthoSize * minZoomMult;
            float maxSize = baseOrthoSize * maxZoomMult;
            targetZoom = Mathf.Clamp(targetZoom, minSize, maxSize);
        }
    }

    private void HandleMovement()
    {
        // 1. Клавіатура (WASD або стрілочки)
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");
        if (moveX != 0 || moveY != 0)
        {
            targetPosition += new Vector3(moveX, moveY, 0).normalized * keyboardPanSpeed * Time.deltaTime;
        }

        // 2. Перетягування карти мишкою (без тремтіння!)
        if (Input.GetMouseButtonDown(2) || (Input.GetMouseButtonDown(0) && !IsMouseOverNode()))
        {
            lastMouseScreenPos = Input.mousePosition;
            isDraggingMap = true;
        }

        if (Input.GetMouseButton(2) || (Input.GetMouseButton(0) && isDraggingMap))
        {
            Vector3 mouseDelta = lastMouseScreenPos - Input.mousePosition;
            lastMouseScreenPos = Input.mousePosition;

            // Переводимо пікселі екрана в одиниці ігрового світу пропорційно поточному зуму
            float worldUnitsPerPixel = (cam.orthographicSize * 2f) / Screen.height;
            targetPosition += new Vector3(mouseDelta.x, mouseDelta.y, 0) * worldUnitsPerPixel * mouseDragSensitivity;
        }

        if (Input.GetMouseButtonUp(2) || Input.GetMouseButtonUp(0))
        {
            isDraggingMap = false;
        }
    }

    private bool IsMouseOverNode()
    {
        Vector2 mouseWorldPos = cam.ScreenToWorldPoint(Input.mousePosition);
        Collider2D hit = Physics2D.OverlapPoint(mouseWorldPos);
        return hit != null && hit.GetComponent<AltarNode>() != null;
    }

    private void ClampTargetToBounds()
    {
        float camHalfHeight = cam.orthographicSize;
        float camHalfWidth = camHalfHeight * cam.aspect;

        float minX = mapLimitsX.x + camHalfWidth;
        float maxX = mapLimitsX.y - camHalfWidth;
        float minY = mapLimitsY.x + camHalfHeight;
        float maxY = mapLimitsY.y - camHalfHeight;

        targetPosition.x = (minX < maxX) ? Mathf.Clamp(targetPosition.x, minX, maxX) : (mapLimitsX.x + mapLimitsX.y) * 0.5f;
        targetPosition.y = (minY < maxY) ? Mathf.Clamp(targetPosition.y, minY, maxY) : (mapLimitsY.x + mapLimitsY.y) * 0.5f;
        targetPosition.z = transform.position.z;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Vector3 center = new Vector3((mapLimitsX.x + mapLimitsX.y) * 0.5f, (mapLimitsY.x + mapLimitsY.y) * 0.5f, 0);
        Vector3 size = new Vector3(mapLimitsX.y - mapLimitsX.x, mapLimitsY.y - mapLimitsY.x, 0);
        Gizmos.DrawWireCube(center, size);
    }
}