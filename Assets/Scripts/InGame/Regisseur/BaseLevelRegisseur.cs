using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;
using DG.Tweening;
using UnityEngine.Serialization;

// A base class for all levels that manages time, history, and completion.
public abstract class BaseLevelRegisseur : MonoBehaviour
{
    [FormerlySerializedAs("_levelTargetDescription")]
    [Header("Level Content Configuration")]
    [TextArea(3, 10)]
    [SerializeField] protected string levelTargetDescription;

    [FormerlySerializedAs("_correctAnswer")]
    [Tooltip("Correct answer for this level")]
    [SerializeField] protected int correctAnswer;
    protected virtual int RightAnswerValue => correctAnswer;

    // --- CLOCK AND HISTORY CONTROL ---
    [FormerlySerializedAs("_nextClick")]
    [Header("Clock Control")]
    [SerializeField] protected Button nextClick;
    [FormerlySerializedAs("_prevClick")] [SerializeField] protected Button prevClick;
    [FormerlySerializedAs("_currentTickText")] [SerializeField] protected TMP_Text currentTickText;

    [FormerlySerializedAs("_maxTickNumber")]
    [Tooltip("The maximum number of ticks available on level.")]
    [SerializeField] protected int maxTickNumber = 3;

    // --- SOLUTION AND LEVEL MANAGEMENT ---
    [FormerlySerializedAs("_checkSolutionButton")]
    [Header("Solution & Level Management")]
    [SerializeField] protected Button checkSolutionButton;
    [FormerlySerializedAs("_levelManager")] [SerializeField] protected LevelManager levelManager;
    [FormerlySerializedAs("_levelTargetText")] [SerializeField] protected TMP_Text levelTargetText;

    // Requirements for earning stars
    [FormerlySerializedAs("_threeStarCondition")]
    [Header("Star Conditions (Number of failed tries)")]
    [SerializeField] protected int threeStarCondition;
    [FormerlySerializedAs("_twoStarCondition")] [SerializeField] protected int twoStarCondition = 1;
    [FormerlySerializedAs("_oneStarCondition")] [SerializeField] protected int oneStarCondition = 2;

    [FormerlySerializedAs("_crossIndicator")]
    [Header("Feedback / Error Indicator")]
    [SerializeField] protected GameObject crossIndicator;
    [FormerlySerializedAs("_crossDisplayDuration")] [SerializeField] protected float crossDisplayDuration = 0.3f;

    
    // --- STATE ---
    protected int TickCounter;
    protected object[] TickStateValues;
    public int falledTries; // Public for debugging

    [FormerlySerializedAs("_busController")]
    [Header("Bus Vizualization")]
    [SerializeField] protected BusController busController;
    [FormerlySerializedAs("_isProcessing")] public bool isProcessing;


    #region ABSTRACT METHODS (Unique to each level)
    /// <summary> The main method that implements the logic of a single clock cycle (PreClock/Clock). </summary>
    protected abstract void HandleClockUpdate();

    /// <summary> Must return the current state in a format unique to the level (e.g., levelOneState). </summary>
    protected abstract object GetCurrentState();

    /// <summary> Applies the saved state to the level's internal components. </summary>
    protected abstract void ApplyState(object state);

    /// <summary> Visually indicates when a tick is triggered (e.g., flashing registers). </summary>
    protected abstract void BlinkClockedComponents();

    /// <summary> Blocks all interactive elements to avoid confusion during signal visualization (WE-buttons etc). </summary>
    protected abstract void BlockIngameInteractables();

    protected abstract void ReleaseIngameInteractables();
    #endregion

    /// <summary> 
    /// New abstract method: Runs a bus visualization specific to the current cycle.
    /// Must be implemented in a subclass.
    /// </summary>
    protected abstract IEnumerator RunBusVisualizations();

    /// <summary> 
    /// New abstract method: Stops and destroys all active signals 
    /// (or resets them) when transitioning to the previous bar.
    /// Must be implemented in a subclass.
    /// </summary>
    protected abstract IEnumerator ReverseBusVisualizations();

    // --- UNITY LIFECYCLE & CORE LOGIC ---

    protected virtual void Start()
    {
        OnLevelStart();

        // 1. Initializing the UI and subscriptions
        nextClick.onClick.AddListener(HandleNextTick);
        prevClick.onClick.AddListener(HandlePrevTick);
        checkSolutionButton.onClick.AddListener(CheckSolution);
        currentTickText.text = $"{TickCounter}";

        // 2. Initializing history
        TickStateValues = new object[maxTickNumber]; // Can be _maxTickNumber + 1
        SaveCurrentStateAt(0);
    }

    protected virtual void OnLevelStart()
    {
        // A method for overriding in a subclass (e.g., setting initial values for registers or strings)
    }

    private void HandleNextTick()
    {
        if (TickCounter >= maxTickNumber || isProcessing) { return; }
        StartCoroutine(NextTickSequence());
    }
    private IEnumerator NextTickSequence()
    {
        isProcessing = true;
        nextClick.interactable = false; // Visually disable the buttons
        prevClick.interactable = false;
        BlockIngameInteractables();

        BlinkClockedComponents();

        // We are waiting for the entire rendering to complete in the child class
        yield return StartCoroutine(RunBusVisualizations());

        HandleClockUpdate();

        TickCounter++;
        currentTickText.text = $"{TickCounter}";
        UpdateVizualizers();

        // The Logic of History
        var idx = TickCounter;
        if (idx >= 0 && idx < TickStateValues.Length)
        {
            SaveCurrentStateAt(idx);
            //if (_tickStateValues[idx] == null) saveCurrentStateAt(idx);
            //else if (!IsStateEqual(_tickStateValues[idx])) refreshStateMemoryFromCurrentStep();
        }

        isProcessing = false;
        nextClick.interactable = true;
        prevClick.interactable = true;
        ReleaseIngameInteractables();
    }

    private void HandlePrevTick()
    {
        if (TickCounter <= 0 || isProcessing) { return; }
        StartCoroutine(PrevTickSequence());
    }
    private IEnumerator PrevTickSequence()
    {
        isProcessing = true;
        nextClick.interactable = false;
        prevClick.interactable = false;
        BlockIngameInteractables();

        TickCounter--;
        currentTickText.text = $"{TickCounter}";

        // We are waiting for the reverse rendering to finish
        yield return StartCoroutine(ReverseBusVisualizations());

        ApplyState(TickStateValues[TickCounter]);
        UpdateVizualizers();

        isProcessing = false;
        nextClick.interactable = true;
        prevClick.interactable = true;
        ReleaseIngameInteractables();
    }

    protected virtual void CheckSolution()
    {
        if (CheckWinCondition())
        {
            Debug.Log("Level is solved!");
            var nextLevelToUnlockIndex = SceneManager.GetActiveScene().buildIndex + 1;
            var highestUnlockedIndex = PlayerPrefs.GetInt("UnlockedLevelIndex", 1);

            if (nextLevelToUnlockIndex > highestUnlockedIndex)
            {
                PlayerPrefs.SetInt("UnlockedLevelIndex", nextLevelToUnlockIndex);
                PlayerPrefs.Save(); // Saving data to disk
                Debug.Log($"New level unlocked: Scene Index {nextLevelToUnlockIndex}");
            }

            var earnedStars = CalculateStars(falledTries);
            levelManager.SetGainedStars(earnedStars);
            levelManager.OpenEndOfLevelMenu();
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
        if (crossIndicator == null) yield break;

        // 1. Preparation
        crossIndicator.SetActive(true);
        crossIndicator.transform.localScale = Vector3.zero; // Beginning with 0

        // 2. Creating a Sequence
        var errorSequence = DOTween.Sequence();

        // A sudden appearance followed by a jolt
        errorSequence.Append(crossIndicator.transform.DOScale(1.2f, 0.15f).SetEase(Ease.OutBack));
        errorSequence.Join(crossIndicator.transform.DOShakePosition(0.3f, strength: 15f, vibrato: 15));

        // A brief pause to give the player time to realize their mistake
        errorSequence.AppendInterval(crossDisplayDuration);

        // Fade: Quickly fade to zero with a “collapse” effect
        errorSequence.Append(crossIndicator.transform.DOScale(0f, 0.15f).SetEase(Ease.InBack));

        // We wait for the entire animation sequence to complete before proceeding further through the coroutine
        yield return errorSequence.WaitForCompletion();

        crossIndicator.SetActive(false);
    }
    protected abstract bool CheckWinCondition();

    #region HISTORY METHODS
    protected void SaveCurrentStateAt(int idx)
    {
        TickStateValues[idx] = GetCurrentState();
    }

    protected void RefreshStateMemoryFromCurrentStep()
    {
        for (var i = TickCounter; i < TickStateValues.Length; i++)
        {
            TickStateValues[i] = null;
        }
    }

    protected abstract void UpdateVizualizers();

    protected int CalculateStars(int tries)
    {
        if (tries <= threeStarCondition) return 3;
        if (tries <= twoStarCondition) return 2;
        if (tries <= oneStarCondition) return 1;
        return 0;
    }
    #endregion

    #region command builder
    protected string CommandBuilder(uint val)
    {
        if (val < 1000000) {
            return $"{val}";
        }

        var opcode = val & 0x7F;
        var rd = (val >> 7) & 0x1F;
        var funct3 = (val >> 12) & 0x7;
        var rs1 = (val >> 15) & 0x1F;
        var rs2 = (val >> 20) & 0x1F;
        var funct7 = (val >> 25) & 0x7F;

        return opcode switch
        {
            0x33 => DecodeRType(funct3, funct7, rd, rs1, rs2),
            0x13 => DecodeITypeAlu(funct3, rd, rs1, (int)val >> 20),
            0x03 => $"lw x{rd}, {(int)val >> 20}(x{rs1})",
            0x23 => $"sw x{rs2}, {Extender.Evaluate(1, val)}(x{rs1})",
            0x63 => $"beq x{rs1}, x{rs2}, {Extender.Evaluate(2, val)}",
            0x6F => $"jal x{rd}, {Extender.Evaluate(3, val)}",
            0x67 => $"jalr x{rd}, {(int)val >> 20}(x{rs1})",
            _ => $"unknown (0x{val:X8})"
        };
    }

    private string DecodeRType(uint f3, uint f7, uint rd, uint rs1, uint rs2)
    {
        var op = (f3, f7) switch
        {
            (0x0, 0x00) => "add",
            (0x0, 0x20) => "sub",
            (0x7, 0x00) => "and",
            _ => "unknown_R"
        };
        return $"{op} x{rd}, x{rs1}, x{rs2}";
    }

    private string DecodeITypeAlu(uint f3, uint rd, uint rs1, int imm)
    {
        // For shift instructions (SLLI, SRLI, SRAI), only the lower 5 or 6 bits of the imm
        // and the 30th bit of the instruction are used to distinguish between SRLI and SRAI.
        var shamt = (uint)imm & 0x1F; // shift amount
        var bit30 = ((imm >> 10) & 0x1) == 1; // The 30th bit of the entire instruction

        return f3 switch
        {
            0x0 => $"addi x{rd}, x{rs1}, {imm}",      // Add Immediate
            0x1 => $"slli x{rd}, x{rs1}, {shamt}",    // Shift Left Logical Imm
            0x2 => $"slti x{rd}, x{rs1}, {imm}",      // Set Less Than Imm
            0x3 => $"sltiu x{rd}, x{rs1}, {(uint)imm}", // Set Less Than Imm Unsigned
            0x4 => $"xori x{rd}, x{rs1}, {imm}",      // Xor Immediate
            0x5 => bit30                              // Shift Right
                    ? $"srai x{rd}, x{rs1}, {shamt}"  // Arithmetic (saving sign)
                    : $"srli x{rd}, x{rs1}, {shamt}", // Logical (filling with zeros)
            0x6 => $"ori x{rd}, x{rs1}, {imm}",       // Or Immediate
            0x7 => $"andi x{rd}, x{rs1}, {imm}",      // And Immediate
            _ => $"unknown_I x{rd}, x{rs1}, {imm}"
        };
    }
    #endregion
    
    # region STATIC HELPER METHODS
    
    protected static void ApplyMuxState(int path, MultiplexerVisualizer mux)
    {
        switch (path)
        {
            case -1:
                mux.ResetVisualisation();
                break;
            case >= 0 and <= 2:
                mux.SelectPath(path);
                break;
        }
    }
    
    protected static int EvaluateMux(int path, int v0, int v1, int v2) => path switch
    {
        0 => v0, 1 => v1, 2 => v2,
        _ => 0
    };
    
    protected void SetLevelTargetText(string fallback)
        => levelTargetText.text = string.IsNullOrEmpty(levelTargetDescription) ? fallback : levelTargetDescription;
    
    protected IEnumerator DelayedSignal(LineRenderer seg, int value, bool reverse = false)
    {
        yield return new WaitUntil(() => busController.NoActiveSignals);
        busController.StartBusSignal(seg, value, reverse);
    }

    protected IEnumerator DelayedSignals(
        LineRenderer seg1, int val1,
        LineRenderer seg2, int val2,
        bool rev1 = false, bool rev2 = false)
    {
        yield return new WaitUntil(() => busController.NoActiveSignals);
        busController.StartBusSignal(seg1, val1, rev1);
        busController.StartBusSignal(seg2, val2, rev2);
    }
    # endregion
}