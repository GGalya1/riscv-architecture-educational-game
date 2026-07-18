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
    [SerializeField] private Vector2 hiddenPosition = new (0f, -1000f);
    
    [Tooltip("Target position for the Options panel when shown.")]
    [SerializeField] private Vector2 optionsPanelTargetPosition = new Vector2(0f, 200f);

    [Header("References")]
    [SerializeField] private CanvasGroup bgGroup;
    [SerializeField] private CanvasGroup levelPanelGroup;
    [SerializeField] private RectTransform levelPanelRectTransform;
    [SerializeField] private CanvasGroup optionsPanelGroup;
    [SerializeField] private RectTransform optionsPanelRectTransform;

    [Header("Easing")]
    [SerializeField] private Ease showEase = Ease.OutBack;
    [SerializeField] private Ease hideEase = Ease.InQuint;
    
    [Header("Show animation")]
    [SerializeField] private float showStartScale = 0.92f;
    [SerializeField] private float panelShowDelay = 0.08f;

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
        
        panelTransform.localScale = Vector3.one * showStartScale;

        DOTween.Sequence()
            .SetDelay(panelShowDelay)
            .Append(panelTransform.DOAnchorPos(Vector2.zero, fadeTime).SetEase(showEase))
            .Join(panelTransform.DOScale(1f, fadeTime).SetEase(showEase))
            .Join(group.DOFade(1f, fadeTime));
    }
    private void ShowPanel(RectTransform panelTransform, CanvasGroup group, Vector2 targetPosition) 
    {
        // Cancel any ongoing animations to prevent flickering
        panelTransform.DOKill();
        group.DOKill();

        // Enable interactions immediately when showing starts
        group.interactable = true;
        group.blocksRaycasts = true;
        
        panelTransform.localScale = Vector3.one * showStartScale;

        DOTween.Sequence()
            .SetDelay(panelShowDelay)
            .Append(panelTransform.DOAnchorPos(targetPosition, fadeTime).SetEase(showEase))
            .Join(panelTransform.DOScale(1f, fadeTime).SetEase(showEase))
            .Join(group.DOFade(1f, fadeTime));
    }

    public void ShowLevelPanel()
    {
        ShowBg();
        ShowPanel(levelPanelRectTransform, levelPanelGroup);
    }
    public void ShowOptionsPanel() {
        ShowBg();
        ShowPanel(optionsPanelRectTransform, optionsPanelGroup, optionsPanelTargetPosition);
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
        levelPanelRectTransform?.DOKill();
        levelPanelGroup?.DOKill();

        optionsPanelRectTransform?.DOKill();
        optionsPanelGroup?.DOKill();
    }
}
