using UnityEngine;
using DG.Tweening;

/// <summary>
/// Handles smooth transitions for menu panels using CanvasGroup and RectTransform.
/// Manages fading, movement, and interaction states.
/// </summary>
public class MenuAnimationManager : MonoBehaviour
{
    [Header("Animation Settings")]
    [Tooltip("Duration of the show/hide animations.")]
    [SerializeField] private float _fadeTime = 0.3f;

    [Tooltip("The vertical offset for the panel's hidden state.")]
    [SerializeField] private Vector2 _hiddenPosition = new Vector2(0f, -1000f);

    [Header("References")]
    [SerializeField] private CanvasGroup bgGroup;
    [SerializeField] private CanvasGroup levelPanelGroup;
    [SerializeField] private RectTransform levelPanelRectTransform;
    [SerializeField] private CanvasGroup optionsPanelGroup;
    [SerializeField] private RectTransform optionsPanelRectTransform;

    [Header("Easing")]
    [SerializeField] private Ease showEase = Ease.OutBack;
    [SerializeField] private Ease hideEase = Ease.InQuint;

    private void Awake()
    {
        InitPanel();
    }

    /// <summary>
    /// Sets the panel to its initial hidden state without animations.
    /// </summary>
    private void InitPanel()
    {
        InitPanel(levelPanelRectTransform, levelPanelGroup);

        if (optionsPanelRectTransform != null) {
            InitPanel(optionsPanelRectTransform, optionsPanelGroup);
        }
    }
    private void InitPanel(RectTransform transform, CanvasGroup group)
    {
        group.alpha = 0f;
        group.interactable = false;
        group.blocksRaycasts = false;
        transform.anchoredPosition = _hiddenPosition;
    }

    /// <summary>
    /// Animates the panel into view and enables interactions.
    /// </summary>
    public void ShowPanel(RectTransform transform, CanvasGroup group) 
    {
        // Cancel any ongoing animations to prevent flickering
        transform.DOKill();
        group.DOKill();

        // Enable interactions immediately when showing starts
        group.interactable = true;
        group.blocksRaycasts = true;

        transform.DOAnchorPos(Vector2.zero, _fadeTime).SetEase(showEase);
        group.DOFade(1, _fadeTime);
    }

    public void ShowLevelPanel()
    {
        ShowBG();
        ShowPanel(levelPanelRectTransform, levelPanelGroup);
    }
    public void ShowOptionsPanel() {
        ShowBG();
        ShowPanel(optionsPanelRectTransform, optionsPanelGroup);
    }

    public void ShowBG() {
        bgGroup.DOKill();

        bgGroup.interactable = true;
        bgGroup.blocksRaycasts = true;

        bgGroup.DOFade(1, _fadeTime);
    }
    public void HideBG()
    {
        bgGroup.DOKill();

        bgGroup.interactable = false;

        bgGroup.DOFade(0f, _fadeTime).OnComplete(() =>
        {
            bgGroup.blocksRaycasts = false;
            bgGroup.interactable = false;
        });
    }

    /// <summary>
    /// Animates the panel out of view and disables interactions.
    /// </summary>
    public void HidePanel(RectTransform transform, CanvasGroup group) {
        transform.DOKill();
        group.DOKill();

        group.interactable = false;

        transform.DOAnchorPos(_hiddenPosition, _fadeTime).SetEase(hideEase);

        // Fade out and disable blocksRaycasts only when finished
        group.DOFade(0f, _fadeTime).OnComplete(() =>
        {
            group.blocksRaycasts = false;
        });
    }
    public void HideLevelPanel()
    {
        HideBG();
        HidePanel(levelPanelRectTransform, levelPanelGroup);
    }
    public void HideOptionsPanel() {
        HideBG();
        HidePanel(optionsPanelRectTransform, optionsPanelGroup);
    }

    private void OnDestroy()
    {
        // Clean up tweens to prevent memory leaks if the object is destroyed mid-animation
        levelPanelRectTransform.DOKill();
        levelPanelGroup.DOKill();

        optionsPanelRectTransform.DOKill();
        optionsPanelGroup.DOKill();
    }
}
