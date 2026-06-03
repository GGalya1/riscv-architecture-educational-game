using DG.Tweening;
using UnityEngine;
using UnityEngine.Serialization;

public class SidePanelAnimator : MonoBehaviour
{
    [Header("Position Settings")]
    public float hiddenPosX = 1917f;
    public float visiblePosX = 657f;

    [Header("Animation Settings")]
    public float duration = 0.6f;
    [Range(0.5f, 3.0f)]
    public float bouncePower = 1.2f;

    [FormerlySerializedAs("_rectTransform")] [SerializeField] private RectTransform rectTransform;
    private bool _isOpen;

    private void Awake()
    {
        rectTransform.anchoredPosition = new Vector2(hiddenPosX, rectTransform.anchoredPosition.y);
    }

    public void TogglePanel()
    {
        rectTransform.DOKill();

        if (!_isOpen)
        {
            rectTransform.DOAnchorPosX(visiblePosX, duration)
                .SetEase(Ease.OutBack, bouncePower);
        }
        else
        {
            rectTransform.DOAnchorPosX(hiddenPosX, duration)
                .SetEase(Ease.InCubic);
        }

        _isOpen = !_isOpen;
    }
}
