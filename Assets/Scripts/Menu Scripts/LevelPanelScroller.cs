using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;


/// <summary>
/// Handles page-based navigation for a ScrollRect.
/// Supports smooth snapping to pages and manual button-based navigation.
/// </summary>
public class LevelPanelScroller : MonoBehaviour
{
    [Header("References")]
    public ScrollRect scrollRect;
    [FormerlySerializedAs("_contentContainer")] [SerializeField] private RectTransform contentContainer;

    [Header("UI Text Settings")]
    public TMP_Text chapterTitleText;
    public string[] chapterNames;
    public float fadeDuration = 0.3f;

    [Header("Animation Settings")]
    public int totalPages = 2;
    public float duration = 0.5f;
    public Ease animEase = Ease.OutCubic;

    private int _currentPage;

    // Minimum drag distance to trigger page swap
    public float dragThreshold;


    private void Awake()
    {
        // Calculate threshold based on screen width (roughly 6% of screen)
        dragThreshold = Screen.width / 15f;

        // Optional: Auto-calculate pages based on children count
        if (contentContainer != null && contentContainer.childCount > 0)
        {
            totalPages = contentContainer.childCount;
        }

        if (chapterTitleText != null && chapterNames.Length > 0)
        {
            chapterTitleText.text = chapterNames[0];
        }
    }

    /// <summary>
    /// Navigates to the next page if available.
    /// </summary>
    public void NextPage()
    {
        if (_currentPage >= totalPages - 1) return;
        _currentPage++;
        ScrollToPage(_currentPage);
        UpdateChapterTitle(_currentPage);
    }

    /// <summary>
    /// Navigates to the previous page if available.
    /// </summary>
    public void PreviousPage()
    {
        if (_currentPage <= 0) return;
        _currentPage--;
        ScrollToPage(_currentPage);
        UpdateChapterTitle(_currentPage);
    }

    /// <summary>
    /// Snaps the ScrollRect to a specific page index.
    /// </summary>
    private void ScrollToPage(int pageIndex)
    {
        var targetPos = totalPages > 1 ? (float)pageIndex / (totalPages - 1) : 0;

        scrollRect.DOHorizontalNormalizedPos(targetPos, duration)
                  .SetEase(animEase)
                  .SetUpdate(true);
    }

    private void UpdateChapterTitle(int pageIndex)
    {
        if (chapterTitleText == null || chapterNames.Length <= pageIndex) return;

        var titleSequence = DOTween.Sequence();

        titleSequence.Append(chapterTitleText.DOFade(0, fadeDuration))
                     .AppendCallback(() => {
                         chapterTitleText.text = chapterNames[pageIndex];
                     })
                     .Append(chapterTitleText.DOFade(1, fadeDuration));
    }
}
