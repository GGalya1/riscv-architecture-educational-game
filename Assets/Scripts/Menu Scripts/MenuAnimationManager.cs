using UnityEngine;
using DG.Tweening;
using UnityEngine.Serialization;

/// <summary>
/// Handles smooth transitions for menu panels using CanvasGroup and RectTransform.
/// Manages fading, movement, and interaction states.
/// </summary>
public class MenuAnimationManager : MonoBehaviour
{
    [FormerlySerializedAs("_fadeTime")]
    [Header("Animation Settings")]
    [Tooltip("Duration of the show/hide animations.")]
    [SerializeField] private float fadeTime = 0.3f;

    [FormerlySerializedAs("_hiddenPosition")]
    [Tooltip("The vertical offset for the panel's hidden state.")]
    [SerializeField] private Vector2 hiddenPosition = new Vector2(0f, -1000f);

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
    private void InitPanel(RectTransform panelTransform, CanvasGroup group)
    {
        group.alpha = 0f;
        group.interactable = false;
        group.blocksRaycasts = false;
        panelTransform.anchoredPosition = hiddenPosition;
    }

    /// <summary>
    /// Animates the panel into view and enables interactions.
    /// </summary>
    private void ShowPanel(RectTransform panelTransform, CanvasGroup group) 
    {
        // Cancel any ongoing animations to prevent flickering
        panelTransform.DOKill();
        group.DOKill();

        // Enable interactions immediately when showing starts
        group.interactable = true;
        group.blocksRaycasts = true;

        panelTransform.DOAnchorPos(Vector2.zero, fadeTime).SetEase(showEase);
        group.DOFade(1, fadeTime);
    }

    public void ShowLevelPanel()
    {
        ShowBg();
        ShowPanel(levelPanelRectTransform, levelPanelGroup);
    }
    public void ShowOptionsPanel() {
        ShowBg();
        ShowPanel(optionsPanelRectTransform, optionsPanelGroup);
    }

    private void ShowBg() {
        bgGroup.DOKill();

        bgGroup.interactable = true;
        bgGroup.blocksRaycasts = true;

        bgGroup.DOFade(1, fadeTime);
    }
    private void HideBg()
    {
        bgGroup.DOKill();

        bgGroup.interactable = false;

        bgGroup.DOFade(0f, fadeTime).OnComplete(() =>
        {
            bgGroup.blocksRaycasts = false;
            bgGroup.interactable = false;
        });
    }

    /// <summary>
    /// Animates the panel out of view and disables interactions.
    /// </summary>
    private void HidePanel(RectTransform panelTransform, CanvasGroup group) {
        panelTransform.DOKill();
        group.DOKill();

        group.interactable = false;

        panelTransform.DOAnchorPos(hiddenPosition, fadeTime).SetEase(hideEase);

        // Fade out and disable blocksRaycasts only when finished
        group.DOFade(0f, fadeTime).OnComplete(() =>
        {
            group.blocksRaycasts = false;
        });
    }
    public void HideLevelPanel()
    {
        HideBg();
        HidePanel(levelPanelRectTransform, levelPanelGroup);
    }
    public void HideOptionsPanel() {
        HideBg();
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
