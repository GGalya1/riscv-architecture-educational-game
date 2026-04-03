using DG.Tweening;
using UnityEngine;

public class SidePanelAnimator : MonoBehaviour
{
    [Header("Position Settings")]
    public float hiddenPosX = 1917f;
    public float visiblePosX = 657f;

    [Header("Animation Settings")]
    public float duration = 0.6f;
    [Range(0.5f, 3.0f)]
    public float bouncePower = 1.2f;

    [SerializeField] private RectTransform _rectTransform;
    private bool _isOpen = false;

    void Awake()
    {
        _rectTransform.anchoredPosition = new Vector2(hiddenPosX, _rectTransform.anchoredPosition.y);
    }

    public void TogglePanel()
    {
        _rectTransform.DOKill();

        if (!_isOpen)
        {
            _rectTransform.DOAnchorPosX(visiblePosX, duration)
                .SetEase(Ease.OutBack, bouncePower);
        }
        else
        {
            _rectTransform.DOAnchorPosX(hiddenPosX, duration)
                .SetEase(Ease.InCubic);
        }

        _isOpen = !_isOpen;
    }
}
