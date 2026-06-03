using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

/// <summary>
/// Creates a parallax effect for UI elements based on the mouse position.
/// Smoothly offsets the RectTransform within a specified range.
/// </summary>
public class MenuParallax : MonoBehaviour
{
    [FormerlySerializedAs("_amount")]
    [Header("Parallax Settings")]
    [Tooltip("Maximum displacement distance from the starting position.")]
    [SerializeField] float amount = 20f;

    [FormerlySerializedAs("_smoothTime")]
    [Tooltip("How smoothly the element follows the mouse (lower is faster).")]
    [SerializeField] float smoothTime = 0.3f;

#if UNITY_ANDROID || UNITY_IOS
    [Header("Mobile Settings")]
    [SerializeField] float _gyroSensitivity = 8.0f;
    private Vector2 _gyroPercent;
#endif

    private RectTransform _rectTransform;
    private Vector2 _startPos;
    private Vector2 _velocity;
    private Mouse _mouse;

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        _startPos = _rectTransform.anchoredPosition;

        _mouse = Mouse.current;

#if UNITY_ANDROID || UNITY_IOS
        if (GravitySensor.current != null)
            InputSystem.EnableDevice(GravitySensor.current);

        if (Gyroscope.current != null)
            InputSystem.EnableDevice(Gyroscope.current);
#endif
    }

    private void Update()
    {
#if UNITY_ANDROID || UNITY_IOS
        if (GravitySensor.current != null)
        {
            Vector3 gravity = GravitySensor.current.gravity.ReadValue();

            _gyroPercent.x = Mathf.Clamp(gravity.x * _gyroSensitivity, -1f, 1f);
            _gyroPercent.y = Mathf.Clamp(gravity.y * _gyroSensitivity, -1f, 1f);
        }

        Vector2 targetPos = _startPos + (_gyroPercent * _amount);
#else
        if (_mouse == null) return;

        var mousePos = _mouse.position.ReadValue();
        
        // Convert mouse position to a normalized range: [-0.5, 0.5]
        var mousePercent = new Vector2(
            (mousePos.x / Screen.width) - 0.5f,
            (mousePos.y / Screen.height) - 0.5f
        );

        // Calculate the target position relative to the starting anchor
        var targetPos = _startPos + (mousePercent * amount);
#endif


        // Smoothly move the UI element towards the target position
        // Vector2.SmoothDamp is frame-rate independent and prevents jittering
        _rectTransform.anchoredPosition = Vector2.SmoothDamp(
            _rectTransform.anchoredPosition,
            targetPos,
            ref _velocity,
            smoothTime
        );
    }
}