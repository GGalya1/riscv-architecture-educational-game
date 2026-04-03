using DG.Tweening;
using UnityEngine;

/// <summary>
/// Manages transitions between gameplay UI and dialogue panels.
/// Handles fading and movement for different UI groups.
/// </summary>
public class BeginnOfLevelAnimationManager : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private CanvasGroup oftenPanelGroup;
    [SerializeField] private RectTransform oftenPanelRectTransform;

    [SerializeField] private CanvasGroup mediumPanelGroup;
    [SerializeField] private RectTransform mediumPanelRectTransform;

    [SerializeField] private CanvasGroup dialoguePanelGroup;
    [SerializeField] private RectTransform dialoguePanelRectTransform;


    [Header("Animation Settings")]
    [SerializeField] private float _fadeTime = 0.3f;
    [SerializeField] private float _yOffset = 100f;
    [SerializeField] private Ease showEase = Ease.OutBack;
    [SerializeField] private Ease hideEase = Ease.InQuint;

    private Vector2 _oftenStartPos;
    private Vector2 _mediumStartPos;
    private Vector2 _dialogueStartPos;

    private void Awake()
    {
        // Cache starting positions
        _oftenStartPos = oftenPanelRectTransform.anchoredPosition;
        _mediumStartPos = mediumPanelRectTransform.anchoredPosition;
        _dialogueStartPos = dialoguePanelRectTransform.anchoredPosition;
    }

    /// <summary>
    /// Switches from Dialogue mode back to Gameplay UI.
    /// </summary>
    public void HideDialogue()
    {
        HideUI(dialoguePanelGroup, dialoguePanelRectTransform, _dialogueStartPos, _fadeTime);

        ShowInGameUI(oftenPanelGroup, oftenPanelRectTransform, _oftenStartPos);
        ShowInGameUI(mediumPanelGroup, mediumPanelRectTransform, _mediumStartPos);
    }

    /// <summary>
    /// Switches from Gameplay UI to Dialogue mode.
    /// </summary>
    public void HideUIAndShowDialogue() {
        HideUI(oftenPanelGroup, oftenPanelRectTransform, _oftenStartPos, 0);
        HideUI(mediumPanelGroup, mediumPanelRectTransform, _mediumStartPos, 0);

        ShowInGameUI(dialoguePanelGroup, dialoguePanelRectTransform, _dialogueStartPos);
    }

    private void HideUI(CanvasGroup panelGroup, RectTransform reactTransform, Vector2 startPos, float fadeTime) {
        panelGroup.DOKill();
        reactTransform.DOKill();

        panelGroup.interactable = false;
        panelGroup.blocksRaycasts = false;

        Vector2 hidePos = new Vector2(startPos.x, startPos.y - _yOffset);

        reactTransform.DOAnchorPos(hidePos, fadeTime).SetEase(hideEase);
        panelGroup.DOFade(0, fadeTime);
    }

    private void ShowInGameUI(CanvasGroup panelGroup, RectTransform reactTransform, Vector2 startPos) {
        panelGroup.DOKill();
        reactTransform.DOKill();

        panelGroup.interactable = true;
        panelGroup.blocksRaycasts = true;

        reactTransform.DOAnchorPos(startPos, _fadeTime).SetEase(showEase);
        panelGroup.DOFade(1, _fadeTime);
    }
}
