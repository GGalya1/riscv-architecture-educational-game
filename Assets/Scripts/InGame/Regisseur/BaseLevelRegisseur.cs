using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;
using DG.Tweening;

// Базовый класс для всех уровней, управляющий временем, историей и завершением.
public abstract class BaseLevelRegisseur : MonoBehaviour
{
    [Header("Level Content Configuration")]
    [TextArea(3, 10)]
    [SerializeField] protected string _levelTargetDescription;

    [Tooltip("Correct answer for this level")]
    [SerializeField] protected int _correctAnswer;
    protected virtual int RightAnswerValue => _correctAnswer;

    // --- CLOCK AND HISTORY CONTROL (Конфигурация) ---
    [Header("Clock Control")]
    [SerializeField] protected Button _nextClick;
    [SerializeField] protected Button _prevClick;
    [SerializeField] protected TMP_Text _currentTickText;

    [Tooltip("Максимальное количество тиков, доступное на уровне.")]
    [SerializeField] protected int _maxTickNumber = 3;

    // --- SOLUTION AND LEVEL MANAGEMENT (Конфигурация) ---
    [Header("Solution & Level Management")]
    [SerializeField] protected Button _checkSolutionButton;
    [SerializeField] protected LevelManager _levelManager;
    [SerializeField] protected TMP_Text _levelTargetText;

    // Условия для получения звезд
    [Header("Star Conditions (Number of failed tries)")]
    [SerializeField] protected int _threeStarCondition = 0;
    [SerializeField] protected int _twoStarCondition = 1;
    [SerializeField] protected int _oneStarCondition = 2;

    [Header("Feedback / Error Indicator")]
    [SerializeField] protected GameObject _crossIndicator;
    [SerializeField] protected float _crossDisplayDuration = 0.3f;

    
    // --- STATE ---
    protected int _tickCounter = 0;
    protected object[] _tickStateValues;
    public int falledTries = 0; // Public для отладки

    [Header("Bus Vizualization")]
    [SerializeField] protected BusController _busController;
    protected bool _isProcessing = false;
    public bool IsProcessing => _isProcessing;



    #region ABSTRACT METHODS (Уникальны для каждого уровня)
    /// <summary> Основной метод, выполняющий логику одного такта (PreClock/Clock). </summary>
    protected abstract void HandleClockUpdate();

    /// <summary> Должен вернуть текущее состояние в формате, уникальном для уровня (e.g., levelOneState). </summary>
    protected abstract object GetCurrentState();

    /// <summary> Применяет сохраненное состояние к внутренним компонентам уровня. </summary>
    protected abstract void ApplyState(object state);

    /// <summary> Сравнивает текущее состояние с сохраненным. </summary>
    //protected abstract bool IsStateEqual(object state);

    /// <summary> Визуально сигнализирует о срабатывании такта (например, мигание регистров). </summary>
    protected abstract void BlinkClockedComponents();

    /// <summary> Блокирует все интерактивные элементы чтобы избежать путаницы во время визуализации сигналов (WE-buttons etc). </summary>
    protected abstract void BlockIngameInteractables();

    protected abstract void ReleaseIngameInteractables();
    #endregion

    /// <summary> 
    /// Новый абстрактный метод: Запускает визуализацию шин, специфичную для текущего такта.
    /// Должен быть реализован в дочернем классе.
    /// </summary>
    protected abstract IEnumerator RunBusVisualizations();

    /// <summary> 
    /// Новый абстрактный метод: Останавливает и уничтожает все активные сигналы 
    /// (или откатывает их) при переходе к предыдущему такту.
    /// Должен быть реализован в дочернем классе.
    /// </summary>
    protected abstract IEnumerator ReverseBusVisualizations();

    // --- UNITY LIFECYCLE & CORE LOGIC (Общая реализация) ---

    protected virtual void Start()
    {
        OnLevelStart();

        // 1. Инициализация UI и подписки
        _nextClick.onClick.AddListener(HandleNextTick);
        _prevClick.onClick.AddListener(HandlePrevTick);
        _checkSolutionButton.onClick.AddListener(CheckSolution);
        _currentTickText.text = $"{_tickCounter}";

        // 2. Инициализация истории
        _tickStateValues = new object[_maxTickNumber]; // Может быть _maxTickNumber + 1
        saveCurrentStateAt(0);

        // 3. Дополнительные действия (визуализация, цели)
    }

    protected virtual void OnLevelStart()
    {
        // Метод для переопределения в дочернем классе (например, установка начальных значений регистров, текстов)
    }

    public void HandleNextTick()
    {
        if (_tickCounter >= _maxTickNumber || _isProcessing) { return; }
        StartCoroutine(NextTickSequence());
    }
    private IEnumerator NextTickSequence()
    {
        _isProcessing = true;
        _nextClick.interactable = false; // Блокируем кнопки визуально
        _prevClick.interactable = false;
        BlockIngameInteractables();

        BlinkClockedComponents();

        // Ждем завершения всей визуализации в дочернем классе
        yield return StartCoroutine(RunBusVisualizations());

        HandleClockUpdate();

        _tickCounter++;
        _currentTickText.text = $"{_tickCounter}";
        UpdateVizualizers();

        // Логика истории
        int idx = _tickCounter;
        if (idx >= 0 && idx < _tickStateValues.Length)
        {
            saveCurrentStateAt(idx);
            //if (_tickStateValues[idx] == null) saveCurrentStateAt(idx);
            //else if (!IsStateEqual(_tickStateValues[idx])) refreshStateMemoryFromCurrentStep();
        }

        _isProcessing = false;
        _nextClick.interactable = true;
        _prevClick.interactable = true;
        ReleaseIngameInteractables();
    }

    public void HandlePrevTick()
    {
        if (_tickCounter <= 0 || _isProcessing) { return; }
        StartCoroutine(PrevTickSequence());
    }
    private IEnumerator PrevTickSequence()
    {
        _isProcessing = true;
        _nextClick.interactable = false;
        _prevClick.interactable = false;
        BlockIngameInteractables();

        _tickCounter--;
        _currentTickText.text = $"{_tickCounter}";

        // Ждем завершения обратной визуализации
        yield return StartCoroutine(ReverseBusVisualizations());

        ApplyState(_tickStateValues[_tickCounter]);
        UpdateVizualizers();

        _isProcessing = false;
        _nextClick.interactable = true;
        _prevClick.interactable = true;
        ReleaseIngameInteractables();
    }

    protected virtual void CheckSolution()
    {
        if (CheckWinCondition())
        {
            Debug.Log("Level is solved!");
            int nextLevelToUnlockIndex = SceneManager.GetActiveScene().buildIndex + 1;
            int highestUnlockedIndex = PlayerPrefs.GetInt("UnlockedLevelIndex", 1);

            if (nextLevelToUnlockIndex > highestUnlockedIndex)
            {
                PlayerPrefs.SetInt("UnlockedLevelIndex", nextLevelToUnlockIndex);
                PlayerPrefs.Save(); // Сохраняем данные на диск
                Debug.Log($"New level unlocked: Scene Index {nextLevelToUnlockIndex}");
            }

            int earnedStars = CalculateStars(falledTries);
            _levelManager.setGainedStars(earnedStars);
            _levelManager.OpenEndOfLevelMenu();
        }
        else
        {
            falledTries++;
            Debug.LogError("Level is not solved! Failed tries: " + falledTries);

            StartCoroutine(ShowIncorrectIndicator());
        }
    }
    private IEnumerator ShowIncorrectIndicator()
    {
        if (_crossIndicator == null) yield break;

        // 1. Подготовка
        _crossIndicator.SetActive(true);
        _crossIndicator.transform.localScale = Vector3.zero; // Начинаем с нуля

        // 2. Создаем Sequence (последовательность)
        Sequence errorSequence = DOTween.Sequence();

        // Появление с "отскоком" и одновременная тряска
        errorSequence.Append(_crossIndicator.transform.DOScale(1.2f, 0.15f).SetEase(Ease.OutBack));
        errorSequence.Join(_crossIndicator.transform.DOShakePosition(0.3f, strength: 15f, vibrato: 15));

        // Небольшая пауза, чтобы игрок успел осознать ошибку
        errorSequence.AppendInterval(_crossDisplayDuration);

        // Исчезновение: быстро уменьшаем в ноль с эффектом "схлопывания"
        errorSequence.Append(_crossIndicator.transform.DOScale(0f, 0.15f).SetEase(Ease.InBack));

        // Ждем, пока вся цепочка анимаций выполнится, прежде чем идти дальше по корутину
        yield return errorSequence.WaitForCompletion();

        _crossIndicator.SetActive(false);
    }
    protected abstract bool CheckWinCondition();

    #region HISTORY METHODS
    protected void saveCurrentStateAt(int idx)
    {
        _tickStateValues[idx] = GetCurrentState();
    }

    protected void refreshStateMemoryFromCurrentStep()
    {
        for (int i = _tickCounter; i < _tickStateValues.Length; i++)
        {
            _tickStateValues[i] = null;
        }
    }

    protected abstract void UpdateVizualizers();

    protected int CalculateStars(int tries)
    {
        if (tries <= _threeStarCondition) return 3;
        if (tries <= _twoStarCondition) return 2;
        if (tries <= _oneStarCondition) return 1;
        return 0;
    }
    #endregion

    #region command builder
    protected string commandBuilder(uint val)
    {
        if (val < 1000000) {
            return $"{val}";
        }

        uint opcode = val & 0x7F;
        uint rd = (val >> 7) & 0x1F;
        uint funct3 = (val >> 12) & 0x7;
        uint rs1 = (val >> 15) & 0x1F;
        uint rs2 = (val >> 20) & 0x1F;
        uint funct7 = (val >> 25) & 0x7F;

        return opcode switch
        {
            0x33 => decodeRType(funct3, funct7, rd, rs1, rs2),
            0x13 => decodeITypeALU(funct3, rd, rs1, (int)val >> 20),
            0x03 => $"lw x{rd}, {(int)val >> 20}(x{rs1})",
            0x23 => $"sw x{rs2}, {Extender.Evaluate(1, val)}(x{rs1})",
            0x63 => $"beq x{rs1}, x{rs2}, {Extender.Evaluate(2, val)}",
            0x6F => $"jal x{rd}, {Extender.Evaluate(3, val)}",
            0x67 => $"jalr x{rd}, {(int)val >> 20}(x{rs1})",
            _ => $"unknown (0x{val:X8})"
        };
    }

    private string decodeRType(uint f3, uint f7, uint rd, uint rs1, uint rs2)
    {
        string op = (f3, f7) switch
        {
            (0x0, 0x00) => "add",
            (0x0, 0x20) => "sub",
            (0x7, 0x00) => "and",
            _ => "unknown_R"
        };
        return $"{op} x{rd}, x{rs1}, x{rs2}";
    }

    private string decodeITypeALU(uint f3, uint rd, uint rs1, int imm)
    {
        // Для команд сдвига (SLLI, SRLI, SRAI) используются только младшие 5 или 6 бит imm
        // и 30-й бит инструкции для различения SRLI и SRAI.
        uint shamt = (uint)imm & 0x1F; // shift amount
        bool bit30 = ((imm >> 10) & 0x1) == 1; // 30-й бит всей инструкции

        return f3 switch
        {
            0x0 => $"addi x{rd}, x{rs1}, {imm}",      // Add Immediate
            0x1 => $"slli x{rd}, x{rs1}, {shamt}",    // Shift Left Logical Imm
            0x2 => $"slti x{rd}, x{rs1}, {imm}",      // Set Less Than Imm
            0x3 => $"sltiu x{rd}, x{rs1}, {(uint)imm}", // Set Less Than Imm Unsigned
            0x4 => $"xori x{rd}, x{rs1}, {imm}",      // Xor Immediate
            0x5 => bit30                              // Shift Right
                    ? $"srai x{rd}, x{rs1}, {shamt}"  // Arithmetic (сохранение знака)
                    : $"srli x{rd}, x{rs1}, {shamt}", // Logical (заполнение нулями)
            0x6 => $"ori x{rd}, x{rs1}, {imm}",       // Or Immediate
            0x7 => $"andi x{rd}, x{rs1}, {imm}",      // And Immediate
            _ => $"unknown_I x{rd}, x{rs1}, {imm}"
        };
    }
    #endregion
}