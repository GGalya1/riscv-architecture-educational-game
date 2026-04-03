using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

/// <summary>
/// Manages horizontal camera movement with smoothing, boundaries, and UI/Interaction checks.
/// Supports both Mouse and Touchscreen input.
/// </summary>
public class CameraController : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("Sensitivity of the drag movement.")]
    [SerializeField] private float sensitivity = 0.05f;

    [Tooltip("Time to reach the target position (smaller = faster).")]
    [SerializeField] private float smoothness = 0.1f;

    [Header("Zoom Settings (Physical)")]
    [SerializeField] private float zoomSensitivity = 0.05f;     // for Touchscreen
    [SerializeField] private float scrollSensitivity = 2.0f;    // for Mouse
    [SerializeField] private float minHeight = 5f;
    [SerializeField] private float maxHeight = 20f;
    [SerializeField] private float zoomSmoothness = 0.1f;

    [Header("Bounds (Horizontal)")]
    public float minX;
    public float maxX;
    public float minZ;
    public float maxZ;

    [Header("References")]
    [SerializeField] private Camera cam;

    private Vector2 lastInputPosition;
    private bool isDraggingCamera = false;
    private Vector3 targetPosition;
    private Vector3 currentVelocity;
    private Transform cachedTransform;


    void Awake()
    {
        // Cache transform for performance and sync targetX with initial position
        cachedTransform = transform;
        targetPosition = cachedTransform.position;
        if (cam == null) cam = Camera.main;
    }

    void Update()
    {
        HandleInput();
        ApplyMovement();
        HandleZoomInput();
    }

    /// <summary>
    /// Processes input detection, including UI blocking and drag calculations.
    /// </summary>
    private void HandleInput()
    {
        Vector2 currentPos = GetPointerPosition();

        // Pressing
        if (GetPointerDown())
        {
            if (IsPointerOverUI() || IsClickingInteractiveObject(currentPos))
            {
                isDraggingCamera = false;
                return;
            }

            isDraggingCamera = true;
            lastInputPosition = currentPos;
            targetPosition = cachedTransform.position; // Сбрасываем таргет на текущую позицию
        }

        // Holding
        if (isDraggingCamera && GetPointerHeld())
        {
            Vector2 delta = (currentPos - lastInputPosition) * sensitivity;

            float moveX = delta.x;
            float moveZ = delta.y;

            targetPosition.x -= moveX;
            targetPosition.z -= moveZ;

            // Ограничиваем движение
            targetPosition.x = Mathf.Clamp(targetPosition.x, minX, maxX);
            targetPosition.z = Mathf.Clamp(targetPosition.z, minZ, maxZ);

            lastInputPosition = currentPos;
        }

        // Releasing
        if (GetPointerUp())
        {
            isDraggingCamera = false;
        }
    }

    /// <summary>
    /// Smoothly interpolates the camera position towards the targetX coordinate.
    /// </summary>
    private void ApplyMovement()
    {
        cachedTransform.position = Vector3.SmoothDamp(cachedTransform.position, targetPosition, ref currentVelocity, smoothness);
    }

    /// <summary>
    /// Returns the current screen position of the mouse or primary touch.
    /// </summary>
    private Vector2 GetPointerPosition()
    {
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
            return Touchscreen.current.primaryTouch.position.ReadValue();

        return Mouse.current != null ? Mouse.current.position.ReadValue() : Vector2.zero;
    }

    private bool GetPointerDown() =>
      (Mouse.current?.leftButton.wasPressedThisFrame ?? false) ||
      (Touchscreen.current?.primaryTouch.press.wasPressedThisFrame ?? false);

    private bool GetPointerHeld() =>
      (Mouse.current?.leftButton.isPressed ?? false) ||
      (Touchscreen.current?.primaryTouch.press.isPressed ?? false);

    private bool GetPointerUp() =>
      (Mouse.current?.leftButton.wasReleasedThisFrame ?? false) ||
      (Touchscreen.current?.primaryTouch.press.wasReleasedThisFrame ?? false);


    /// <summary>
    /// Checks if the pointer is over a specific interactive 3D object to prevent camera dragging.
    /// </summary>
    private bool IsClickingInteractiveObject(Vector2 screenPos)
    {
        Ray ray = cam.ScreenPointToRay(screenPos);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            return hit.collider.TryGetComponent<ClickableObject>(out _);
        }
        return false;
    }

    /// <summary>
    /// Prevents camera movement when interacting with Unity UI elements.
    /// </summary>
    private bool IsPointerOverUI()
    {
        if (EventSystem.current == null) return false;

        if (EventSystem.current.IsPointerOverGameObject()) return true;

        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
        {
            int id = Touchscreen.current.primaryTouch.touchId.ReadValue();
            return EventSystem.current.IsPointerOverGameObject(id);
        }

        return false;
    }

    private void HandleZoomInput()
    {
        float zoomAmount = 0;

        // 1. Для ПК (Колесо мыши)
        if (Mouse.current != null)
        {
            float scroll = Mouse.current.scroll.ReadValue().y;
            if (Mathf.Abs(scroll) > 0.1f)
            {
                zoomAmount = scroll * scrollSensitivity * 0.01f;
            }
        }

        // 2. Для Телефонов (Pinch Zoom)
        if (Touchscreen.current != null)
        {
            var touches = Touchscreen.current.touches;

            // Проверяем, что как минимум два пальца активны
            if (touches[0].isInProgress && touches[1].isInProgress)
            {
                var t1 = touches[0];
                var t2 = touches[1];

                Vector2 t1Pos = t1.position.ReadValue();
                Vector2 t2Pos = t2.position.ReadValue();
                Vector2 t1Delta = t1.delta.ReadValue();
                Vector2 t2Delta = t2.delta.ReadValue();

                // Позиции в предыдущем кадре
                Vector2 t1PrevPos = t1Pos - t1Delta;
                Vector2 t2PrevPos = t2Pos - t2Delta;

                float prevDist = Vector2.Distance(t1PrevPos, t2PrevPos);
                float curDist = Vector2.Distance(t1Pos, t2Pos);

                // Используем zoomSensitivity для тача (обычно нужно значение побольше, чем для мыши)
                zoomAmount = (curDist - prevDist) * zoomSensitivity;

                // Важно: когда зумим двумя пальцами, отключаем перемещение камеры
                isDraggingCamera = false;
            }
        }

        if (Mathf.Abs(zoomAmount) > 0.001f)
        {
            Vector3 moveDirection = cam.transform.forward;
            Vector3 newTarget = targetPosition + moveDirection * zoomAmount;

            // Ограничение по высоте
            if (newTarget.y >= minHeight && newTarget.y <= maxHeight)
            {
                targetPosition = newTarget;
            }
        }
    }
}
