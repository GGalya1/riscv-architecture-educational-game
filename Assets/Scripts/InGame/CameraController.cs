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
    // [SerializeField] private float zoomSmoothness = 0.1f;

    [Header("Bounds (Horizontal)")]
    public float minX;
    public float maxX;
    public float minZ;
    public float maxZ;

    [Header("References")]
    [SerializeField] private Camera cam;

    private Vector2 _lastInputPosition;
    private bool _isDraggingCamera;
    private Vector3 _targetPosition;
    private Vector3 _currentVelocity;
    private Transform _cachedTransform;
    private bool _currentNull;


    private void Start()
    {
        _currentNull = EventSystem.current == null;
    }

    private void Awake()
    {
        // Cache transform for performance and sync targetX with initial position
        _cachedTransform = transform;
        _targetPosition = _cachedTransform.position;
        if (cam == null) cam = Camera.main;
    }

    private void Update()
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
        var currentPos = GetPointerPosition();

        // Pressing
        if (GetPointerDown())
        {
            if (IsPointerOverUI() || IsClickingInteractiveObject(currentPos))
            {
                _isDraggingCamera = false;
                return;
            }

            _isDraggingCamera = true;
            _lastInputPosition = currentPos;
            _targetPosition = _cachedTransform.position; // Сбрасываем таргет на текущую позицию
        }

        // Holding
        if (_isDraggingCamera && GetPointerHeld())
        {
            var delta = (currentPos - _lastInputPosition) * sensitivity;

            var moveX = delta.x;
            var moveZ = delta.y;

            _targetPosition.x -= moveX;
            _targetPosition.z -= moveZ;

            // Ограничиваем движение
            _targetPosition.x = Mathf.Clamp(_targetPosition.x, minX, maxX);
            _targetPosition.z = Mathf.Clamp(_targetPosition.z, minZ, maxZ);

            _lastInputPosition = currentPos;
        }

        // Releasing
        if (GetPointerUp())
        {
            _isDraggingCamera = false;
        }
    }

    /// <summary>
    /// Smoothly interpolates the camera position towards the targetX coordinate.
    /// </summary>
    private void ApplyMovement()
    {
        _cachedTransform.position = Vector3.SmoothDamp(_cachedTransform.position, _targetPosition, ref _currentVelocity, smoothness);
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

    private static bool GetPointerDown() =>
      (Mouse.current?.leftButton.wasPressedThisFrame ?? false) ||
      (Touchscreen.current?.primaryTouch.press.wasPressedThisFrame ?? false);

    private static bool GetPointerHeld() =>
      (Mouse.current?.leftButton.isPressed ?? false) ||
      (Touchscreen.current?.primaryTouch.press.isPressed ?? false);

    private static bool GetPointerUp() =>
      (Mouse.current?.leftButton.wasReleasedThisFrame ?? false) ||
      (Touchscreen.current?.primaryTouch.press.wasReleasedThisFrame ?? false);


    /// <summary>
    /// Checks if the pointer is over a specific interactive 3D object to prevent camera dragging.
    /// </summary>
    private bool IsClickingInteractiveObject(Vector2 screenPos)
    {
        var ray = cam.ScreenPointToRay(screenPos);
        return Physics.Raycast(ray, out var hit) && hit.collider.TryGetComponent<ClickableObject>(out _);
    }

    /// <summary>
    /// Prevents camera movement when interacting with Unity UI elements.
    /// </summary>
    private bool IsPointerOverUI()
    {
        if (_currentNull) return false;

        if (EventSystem.current.IsPointerOverGameObject()) return true;

        if (Touchscreen.current == null || !Touchscreen.current.primaryTouch.press.isPressed) return false;
        var id = Touchscreen.current.primaryTouch.touchId.ReadValue();
        return EventSystem.current.IsPointerOverGameObject(id);

    }

    private void HandleZoomInput()
    {
        float zoomAmount = 0;

        // For PC (Mouse wheel)
        if (Mouse.current != null)
        {
            var scroll = Mouse.current.scroll.ReadValue().y;
            if (Mathf.Abs(scroll) > 0.1f)
            {
                zoomAmount = scroll * scrollSensitivity * 0.01f;
            }
        }

        // For Phones (Pinch Zoom)
        if (Touchscreen.current != null)
        {
            var touches = Touchscreen.current.touches;

            // We check that at least two fingers are active
            if (touches[0].isInProgress && touches[1].isInProgress)
            {
                var t1 = touches[0];
                var t2 = touches[1];

                var t1Pos = t1.position.ReadValue();
                var t2Pos = t2.position.ReadValue();
                var t1Delta = t1.delta.ReadValue();
                var t2Delta = t2.delta.ReadValue();

                // Positions in the previous frame
                var t1PrevPos = t1Pos - t1Delta;
                var t2PrevPos = t2Pos - t2Delta;

                var prevDist = Vector2.Distance(t1PrevPos, t2PrevPos);
                var curDist = Vector2.Distance(t1Pos, t2Pos);

                // We use zoomSensitivity for touch input (you usually need a higher value than for a mouse)
                zoomAmount = (curDist - prevDist) * zoomSensitivity;

                // Important: When zooming with two fingers, disable camera movement
                _isDraggingCamera = false;
            }
        }

        if (!(Mathf.Abs(zoomAmount) > 0.001f)) return;
        var moveDirection = cam.transform.forward;
        var newTarget = _targetPosition + moveDirection * zoomAmount;

        // Height restriction
        if (newTarget.y >= minHeight && newTarget.y <= maxHeight)
        {
            _targetPosition = newTarget;
        }
    }
}
