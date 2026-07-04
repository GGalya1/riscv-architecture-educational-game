using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;


/// <summary>
/// Provides juicy visual feedback when hovering over UI buttons.
/// Uses DOTween for scaling and rotation effects.
/// </summary>
public class UIButtonEffects : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
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
    [SerializeField] private float hoverDuration = 0.2f;
    
    [Header("Click - Squish animation (all platforms)")]
    [SerializeField] private float clickSquish   = 0.12f;
    [SerializeField] private float clickPressDur = 0.1f; // mobile
    [SerializeField] private float clickReleaseDur = 0.25f; // mobile
    // [SerializeField] private float clickPunchDur = 0.22f; // desktop
    
    private Vector3 _initialScale;
    private Vector3 _initialRotation;
    private RectTransform _rectTransform;
    
    private static bool HasCursor => !Application.isMobilePlatform;
    // private bool _isHovered;

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
        if (!HasCursor) return;
        
        // _isHovered = true;
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
        if (!HasCursor) return;
        // _isHovered = false;
        
        _rectTransform.DOKill();

        _rectTransform.DOScale(_initialScale, hoverDuration).SetEase(onExitEase);
        _rectTransform.DOLocalRotate(_initialRotation, hoverDuration).SetEase(onExitEase);
    }
    
    # region On Click (all platforms)
    public void OnPointerDown(PointerEventData eventData)
    {
        _rectTransform.DOKill();
        // _rectTransform.DOPunchScale(Vector3.one * -clickSquish, clickDuration, 5, 0.5f);
        _rectTransform.DOScale(_initialScale * (1f - clickSquish), clickPressDur).SetEase(Ease.OutCubic);
    }
    
    public void OnPointerUp(PointerEventData eventData)
    {
        // var target = (HasCursor && _isHovered) ? _initialScale * hoverScale : _initialScale;
 
        // _rectTransform.DOScale(target, hoverDuration * 0.5f).SetEase(onEnterEase);
        _rectTransform.DOScale(_initialScale, clickReleaseDur).SetEase(Ease.OutBack);
    }
    # endregion

    private void OnDisable()
    {
        // Kill animations if the object is disabled to prevent ghost tweens
        _rectTransform.DOKill();
        
        _rectTransform.localScale       = _initialScale;
        _rectTransform.localEulerAngles = _initialRotation;
    }
}