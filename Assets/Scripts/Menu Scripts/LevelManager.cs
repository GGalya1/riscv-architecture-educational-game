using DG.Tweening;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Controls the level lifecycle, including dialogue triggers, 
/// end-of-level results, and scene transitions.
/// </summary>
public class LevelManager : MonoBehaviour
{
    [Header("Dependencies")]
    public GameObject endLevelMenuUI;
    [SerializeField] private BeginnOfLevelAnimationManager _animManager;
    [SerializeField] private DialogueManager _dialogueManager;

    [Header("Loading Screen")]
    [SerializeField] private CanvasGroup loadingOverlay;
    [SerializeField] private float transitionDuration = 1f;

    [Header("Results Panel Animation")]
    [SerializeField] private CanvasGroup BGGroup;
    [SerializeField] private CanvasGroup resultsPanelGroup;
    [SerializeField] private RectTransform resultsPanelRectTransform;
    [SerializeField] private float _fadeTime = 0.3f;

    [Header("Star Rating")]
    public Image[] stars;
    public Sprite gainedStar;
    [SerializeField] private float _starPopDuration = 1.8f;

    // for transition between processor levels
    [HideInInspector] public static Func<object> OnRequestNextLevelData;

    private const string UNLOCKED_LEVEL_KEY = "UnlockedLevelIndex";

    public void Awake()
    {
        // Subscribe to dialogue events to toggle UI visibility
        _dialogueManager.OnDialogueBegin += _animManager.HideUIAndShowDialogue;
        _dialogueManager.OnDialogueEnd += _animManager.HideDialogue;
        _dialogueManager.OnHintEnabled += _animManager.HideUIAndShowDialogue;

        InitializeResultsPanel();

        if (loadingOverlay != null)
        {
            loadingOverlay.alpha = 1f;
            loadingOverlay.blocksRaycasts = true;
        }
    }

    private void Start()
    {
        if (loadingOverlay != null)
        {
            Sequence fadeInSequence = DOTween.Sequence();
            fadeInSequence.AppendInterval(0.2f);
            fadeInSequence.Append(loadingOverlay.DOFade(0f, transitionDuration).SetUpdate(true));
            fadeInSequence.OnComplete(() => {
                loadingOverlay.blocksRaycasts = false;
            });
        }
    }

    public void SetLevelDialogue(DialogueGraph d) {
        _dialogueManager.SetActiveGraph(d);
    }

    /// <summary>
    /// Restarts the current active scene.
    /// </summary>
    public void RestartLevel()
    {
        /*int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(currentSceneIndex);*/

        StartCoroutine(LoadWithTransition(SceneManager.GetActiveScene().buildIndex));
    }

    /// <summary>
    /// Returns to the main menu.
    /// </summary>
    public void LoadMainMenu()
    {
        //SceneManager.LoadScene("MainMenu");
        StartCoroutine(LoadWithTransition(0));
    }

    /// <summary>
    /// Advances to the next level and saves progress.
    /// </summary>
    public void LoadNextLevel()
    {
        if (SceneManager.GetActiveScene().buildIndex == 20) {
            object nextData = OnRequestNextLevelData?.Invoke();

            if (nextData != null)
            {
                FullProcessorRegiseur._initial = (ProcessorInitialState)nextData;

                StartCoroutine(LoadWithTransition(20));
            }
            else
            {
                Debug.Log("No more processor sub-levels. Loading Main Menu.");
                StartCoroutine(LoadWithTransition(0));
            }
            return;
        }

        int nextLevelIndex = SceneManager.GetActiveScene().buildIndex + 1;

        // Save progress if this is a new level
        int savedProgress = PlayerPrefs.GetInt(UNLOCKED_LEVEL_KEY, 1);
        if (nextLevelIndex > savedProgress)
        {
            PlayerPrefs.SetInt(UNLOCKED_LEVEL_KEY, nextLevelIndex);
            PlayerPrefs.Save();
        }

        if (nextLevelIndex < SceneManager.sceneCountInBuildSettings)
        {
            //SceneManager.LoadScene(nextLevelIndex);
            StartCoroutine(LoadWithTransition(nextLevelIndex));
        }
        else
        {
            Debug.Log("All levels completed! Loading Main Menu (Index 0).");
            //SceneManager.LoadScene(0);
            StartCoroutine(LoadWithTransition(0));
        }
    }

    private IEnumerator LoadWithTransition(int targetIndex)
    {
        if (loadingOverlay == null)
        {
            SceneManager.LoadScene(targetIndex);
            yield break;
        }

        // FadeIn
        loadingOverlay.blocksRaycasts = true;
        loadingOverlay.interactable = false;

        yield return loadingOverlay.DOFade(1f, transitionDuration).SetUpdate(true).WaitForCompletion();

        // Loading next level
        AsyncOperation op = SceneManager.LoadSceneAsync(targetIndex);
        op.allowSceneActivation = false;

        while (op.progress < 0.9f)
        {
            yield return null;
        }

        // Scene activation
        op.allowSceneActivation = true;
    }

    #region stars counting and next level unlocking
    /// <summary>
    /// Displays the results panel with a smooth animation.
    /// </summary>
    public void OpenEndOfLevelMenu()
    {
        if (endLevelMenuUI != null)
        {
            BGGroup.DOKill();
            BGGroup.interactable = true;
            BGGroup.blocksRaycasts = true;
            BGGroup.DOFade(1, _fadeTime);


            resultsPanelRectTransform.DOKill();
            resultsPanelGroup.DOKill();

            resultsPanelGroup.interactable = true;
            resultsPanelGroup.blocksRaycasts = true;

            resultsPanelRectTransform.DOAnchorPos(Vector2.zero, _fadeTime).SetEase(Ease.OutBack);
            resultsPanelGroup.DOFade(1, _fadeTime);
        }
    }
    private void InitializeResultsPanel()
    {
        resultsPanelGroup.alpha = 0f;
        resultsPanelGroup.interactable = false;
        resultsPanelGroup.blocksRaycasts = false;
        resultsPanelRectTransform.anchoredPosition = new Vector2(0, -1000f);
    }

    /// <summary>
    /// Updates stars based on player performance with a pop animation.
    /// </summary>
    /// <param name="gainedStarsCount">Number of stars to fill (0 to 3).</param>
    public void setGainedStars(int gainedStarsCount) {
        int count = Mathf.Clamp(gainedStarsCount, 0, stars.Length);

        for (int i = 0; i < count; i++)
        {
            int index = i; // Closure safety
            stars[index].sprite = gainedStar;

            // Star animation
            stars[index].transform.localScale = Vector3.zero;
            stars[index].transform.DOScale(Vector3.one, _starPopDuration)
                .SetEase(Ease.OutBack)
                .SetDelay(index * 0.15f); // Stars pop one by one
        }
    }
    #endregion

    private void OnDestroy()
    {
        // Crucial: unsubscribe to prevent memory leaks and ghost calls
        if (_dialogueManager != null)
        {
            _dialogueManager.OnDialogueBegin -= _animManager.HideUIAndShowDialogue;
            _dialogueManager.OnDialogueEnd -= _animManager.HideDialogue;
            _dialogueManager.OnHintEnabled -= _animManager.HideUIAndShowDialogue;
        }
    }
}
