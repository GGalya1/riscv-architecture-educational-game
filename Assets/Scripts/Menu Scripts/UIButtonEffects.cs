using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;


/// <summary>
/// Provides juicy visual feedback when hovering over UI buttons.
/// Uses DOTween for scaling and rotation effects.
/// </summary>
public class UIButtonEffects : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Shake Settings")]
    [SerializeField] private float shakeDuration = 0.3f;
    [SerializeField] private float shakeStrength = 5f;
    [SerializeField] private int vibrato = 10;

    [Header("Scale Settings")]
    [SerializeField] private float hoverScale = 1.05f;

    [Header("Anim Settings")]
    [SerializeField] private Ease onEnterEase = Ease.OutQuad;
    [SerializeField] private Ease onExitEase = Ease.OutQuad;

    private Vector3 _initialScale;
    private Vector3 _initialRotation;
    private RectTransform _rectTransform;

    private void Awake()
    {
        // Cache RectTransform for better performance in UI operations
        _rectTransform = GetComponent<RectTransform>();

        _initialScale = _rectTransform.localScale;
        _initialRotation = _rectTransform.localEulerAngles;
    }

    /// <summary>
    /// Triggered when the pointer starts hovering over the button.
    /// </summary>
    public void OnPointerEnter(PointerEventData eventData)
    {
        // Kill any active tweens on this object to prevent conflict
        _rectTransform.DOKill();

        // Subtle punch rotation effect for 'juice'
        _rectTransform.DOPunchRotation(new Vector3(0, 0, shakeStrength), shakeDuration, vibrato);

        // Smooth scale up
        _rectTransform.DOScale(_initialScale * hoverScale, 0.2f).SetEase(onEnterEase);
    }

    /// <summary>
    /// Triggered when the pointer leaves the button.
    /// </summary>
    public void OnPointerExit(PointerEventData eventData)
    {
        _rectTransform.DOKill();

        _rectTransform.DOScale(_initialScale, 0.2f).SetEase(onExitEase);
        _rectTransform.DOLocalRotate(_initialRotation, 0.2f).SetEase(onExitEase);
    }

    private void OnDisable()
    {
        // Kill animations if the object is disabled to prevent ghost tweens
        _rectTransform.DOKill();
    }
}