using UnityEngine;
using DG.Tweening;
using System.Collections;

public class CreditsScroller : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private RectTransform textRect;
    [SerializeField] private CanvasGroup loadingOverlay;
    [SerializeField] private CanvasGroup creditsText;

    [Header("Settings")]
    [SerializeField] private float transitionDuration = 1f;
    [SerializeField] private float scrollDuration = 30f;
    [SerializeField] private float endYValue = 2500f;

    private Vector2 _startPosition;

    private void Awake()
    {
        _startPosition = textRect.anchoredPosition;
    }
    public void LaunchCredits() => StartCoroutine(CreditsRoutine());

    private IEnumerator CreditsRoutine()
    {
        if (loadingOverlay != null) 
        {
            loadingOverlay.blocksRaycasts = true;
        } 

        textRect.anchoredPosition = _startPosition;


        if (loadingOverlay != null)
        {
            loadingOverlay.DOFade(1f, transitionDuration).SetUpdate(true);
        }

        if (creditsText != null)
        {
            yield return creditsText.DOFade(1f, transitionDuration)
                .SetUpdate(true)
                .WaitForCompletion();
        }

        yield return textRect.DOAnchorPosY(endYValue, scrollDuration)
            .SetEase(Ease.Linear)
            .WaitForCompletion();

        if (creditsText != null)
        {
            creditsText.DOFade(0f, transitionDuration).SetUpdate(true);
        }

        if (loadingOverlay == null) yield break;
        yield return loadingOverlay.DOFade(0f, transitionDuration)
            .SetUpdate(true)
            .WaitForCompletion();

        loadingOverlay.blocksRaycasts = false;
        
        LevelEvents.RaiseCreditsWatched();
    }
}
