using DG.Tweening;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages the visual representation of the dialogue system, 
/// handling text display, branching buttons, and character animations.
/// </summary>
public class DialogueUI : MonoBehaviour
{
    [Header("Main Content")]
    [SerializeField] private TMP_Text _textField;
    [SerializeField] private Image _charackterImage;
    [SerializeField] private Button _goNextQuoteButton;

    [Header("Answer Options")]
    [SerializeField] private Transform buttonContainer;
    [SerializeField] private Button _firstAnswerButton;
    [SerializeField] private Button _secondAnswerButton;
    [SerializeField] private Button _thirdAnswerButton;

    [SerializeField] private TMP_Text _firstAnswerText;
    [SerializeField] private TMP_Text _secondAnswerText;
    [SerializeField] private TMP_Text _thirdAnswerText;

    [Header("Settings & Data")]
    [SerializeField] private EmotionControler currentEmotion;
    private Vector2 _basePortraitPos;

    public event Action OnNextRequested;
    public event Action<int> OnSpecificPathRequested;

    // text animation variables
    private DialogueVertexAnimator _vertexAnimator;
    private Coroutine _textRoutine;

    private void Awake()
    {
        _basePortraitPos = _charackterImage.rectTransform.anchoredPosition;
        _vertexAnimator = new DialogueVertexAnimator(_textField);
    }

    private void Start()
    {
        // Subscribe to buttons
        _goNextQuoteButton.onClick.AddListener(() => OnNextRequested?.Invoke());
        _firstAnswerButton.onClick.AddListener(() => OnSpecificPathRequested?.Invoke(1));
        _secondAnswerButton.onClick.AddListener(() => OnSpecificPathRequested?.Invoke(2));
        _thirdAnswerButton.onClick.AddListener(() => OnSpecificPathRequested?.Invoke(3));
    }


    /// <summary>
    /// Refreshes the UI with data from the provided DialogueNode.
    /// </summary>
    /// <param name="node">The data container for the current dialogue step.</param>
    public void UpdateVisuals(DialogueNode node) {
        if (_textRoutine != null) StopCoroutine(_textRoutine);
        _textField.DOKill();

        List<DialogueCommand> commands = DialogueUtility.ProcessInputString(node.DialogueText, out string cleanText);

        _textRoutine = StartCoroutine(_vertexAnimator.AnimateTextIn(commands, cleanText, null, () => {
            Debug.Log("Печать завершена!");
        }));


        if (!string.IsNullOrEmpty(node.firstAnswer)) {
            DecorateSelection(node);
        }
        else if (buttonContainer.gameObject.activeSelf) {
            buttonContainer.gameObject.SetActive(false);
            _goNextQuoteButton.gameObject.SetActive(true);
        }

        int emotionIndex = (int)node.emotionIndex;
        if (emotionIndex < 0 || emotionIndex >= currentEmotion.emotions.Count)
        {
            Debug.LogError($"Index of emotion {node.emotionIndex} is out of bounds !");
            return;
        }

        Sprite newSprite = currentEmotion.emotions[emotionIndex];

        // Play animation only if sprite changes
        if (_charackterImage.sprite != newSprite)
        {
            PlayBounceAndChangeSprite(newSprite);
        }
    }
    public void StopAnimatingText() {
        if (_textRoutine != null) StopCoroutine(_textRoutine);
    }
    public void SkipAnimationOfTyping() {
        _vertexAnimator.SkipToEndOfCurrentMessage();
    }

    /// <summary>
    /// Configures the visibility and text of the response buttons.
    /// </summary>
    public void DecorateSelection(DialogueNode node) 
    {
        if (!string.IsNullOrEmpty(node.firstAnswer)) {
            buttonContainer.gameObject.SetActive(true);
            _goNextQuoteButton.gameObject.SetActive(false);
            _firstAnswerText.text = node.firstAnswer;
        }
        if (!string.IsNullOrEmpty(node.secondAnswer))
        {
            _secondAnswerButton.gameObject.SetActive(true);
            _secondAnswerText.text = node.secondAnswer;
        }
        else { 
            _secondAnswerButton.gameObject.SetActive(false);
        }

        if (!string.IsNullOrEmpty(node.thirdAnswer))
        {
            _thirdAnswerButton.gameObject.SetActive(true);
            _thirdAnswerText.text = node.thirdAnswer;
        }
        else
        {
            _thirdAnswerButton.gameObject.SetActive(false);
        }

    }

    /// <summary>
    /// Plays a 'jump' animation using DOTween and swaps the sprite at the peak.
    /// </summary>
    private void PlayBounceAndChangeSprite(Sprite nextSprite)
    {
        _charackterImage.rectTransform.DOKill();
        _charackterImage.rectTransform.anchoredPosition = _basePortraitPos; // Reset position before animation

        Vector2 jumpTarget = _basePortraitPos + new Vector2(0, 45f);

        // Sequence: Jump up -> Swap Sprite -> Bounce down
        _charackterImage.rectTransform.DOAnchorPos(jumpTarget, 0.15f)
        .SetEase(Ease.OutQuad)
        .OnComplete(() =>
        {
            _charackterImage.sprite = nextSprite;
            _charackterImage.rectTransform.DOAnchorPos(_basePortraitPos, 0.35f)
                .SetEase(Ease.OutBounce);
        });
    }

    private void OnDestroy()
    {
        // kill all animations
        //_meshUpdateTween?.Kill();

        // Clean up listeners to prevent memory leaks
        _goNextQuoteButton.onClick.RemoveAllListeners();
        _firstAnswerButton.onClick.RemoveAllListeners();
        _secondAnswerButton.onClick.RemoveAllListeners();
        _thirdAnswerButton.onClick.RemoveAllListeners();
    }
}
