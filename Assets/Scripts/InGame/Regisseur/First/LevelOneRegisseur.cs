using UnityEngine;
using System.Collections;
using UnityEngine.Serialization;

public struct LevelOneState
{
    public int RegisterAValue;
    public int RegisterBValue;
    public int OutputRegisterValue;

    public bool RegisterAwe;
    public bool RegisterBwe;
    public bool OutputRegisterWe;

    public int CurrentChosenMuxPath;
}

public class LevelOneRegisseur : BaseLevelRegisseur<LevelOneState>
{
    [FormerlySerializedAs("_multiplexerVisualizer")]
    [Header("Level 1 Specific Components")]
    [SerializeField] private MultiplexerVisualizer multiplexerVisualizer;
    [FormerlySerializedAs("_registerSrcAVisualizer")] [SerializeField] private RegisterVisualizer registerSrcAVisualizer;
    [FormerlySerializedAs("_registerSrcBVisualizer")] [SerializeField] private RegisterVisualizer registerSrcBVisualizer;
    [FormerlySerializedAs("_registerOutputVisualizer")] [SerializeField] private RegisterVisualizer registerOutputVisualizer;

    #region CACHED UI REFERENCES
    private InfoPanelUI _infoSrcARegister;
    private InfoPanelUI _infoSrcBRegister;
    private InfoPanelUI _infoOutputRegister;
    #endregion

    // Intern components for computations
    private Register _srcA;
    private Register _srcB;
    private Register _output;

    // protected override int RightAnswerValue => 4;

    private int _currentBus; // [0, 1]

    protected override void OnLevelStart()
    {
        // Initialization of logical components
        _srcA = new Register(4)
        {
            WriteEnable = true
        };
        _srcB = new Register(2)
        {
            WriteEnable = true
        };
        _output = new Register()
        {
            WriteEnable = true
        };

        // Кэширование UI-панелей визуализаторов
        _infoSrcARegister = registerSrcAVisualizer.UIRegisterPanel;
        _infoSrcBRegister = registerSrcBVisualizer.UIRegisterPanel;
        _infoOutputRegister = registerOutputVisualizer.UIRegisterPanel;

        UpdateVisualizers();
    }

    protected override void BlinkClockedComponents()
    {
        registerSrcAVisualizer.TriggerBlink();
        registerSrcBVisualizer.TriggerBlink();
        registerOutputVisualizer.TriggerBlink();
    }
    protected override LevelOneState GetCurrentState()
    {
        return new LevelOneState
        {
            RegisterAValue = _srcA.Output,
            RegisterBValue = _srcB.Output,
            OutputRegisterValue = _output.Output,
            CurrentChosenMuxPath = multiplexerVisualizer.CurrentChosenMuxPath,
            RegisterAwe = _srcA.WriteEnable,
            RegisterBwe = _srcB.WriteEnable,
            OutputRegisterWe = _output.WriteEnable
        };
    }
    protected override void ApplyState(LevelOneState s)
    {
        _srcA = new Register(s.RegisterAValue);
        _srcB = new Register(s.RegisterBValue);
        _output = new Register(s.OutputRegisterValue);
        _srcA.WriteEnable = s.RegisterAwe;
        _srcB.WriteEnable = s.RegisterBwe;
        _output.WriteEnable = s.OutputRegisterWe;
        
        ApplyMuxState(s.CurrentChosenMuxPath, multiplexerVisualizer);
    }
    protected override void UpdateVisualizers()
    {
        _infoSrcARegister.Display("Register 1", $"{_srcA.Output}");
        _infoSrcBRegister.Display("Register 2", $"{_srcB.Output}");
        _infoOutputRegister.Display("Register 3", $"{_output.Output}");

        registerSrcAVisualizer.ForceUpdateWriteEnableVisualization(_srcA.WriteEnable);
        registerSrcBVisualizer.ForceUpdateWriteEnableVisualization(_srcB.WriteEnable);
        registerOutputVisualizer.ForceUpdateWriteEnableVisualization(_output.WriteEnable);
    }

    protected override void HandleClockUpdate() {
        
        var path = multiplexerVisualizer.CurrentChosenMuxPath;
        int[] inputs = { _srcA.Output, _srcB.Output };
        var res = 0;
        
        switch (path)
        {
            case -1:
                Debug.LogError("Multiplexer path not selected (-1). Data will be lost.");
                break;
            case >= 0 and <= 1:
                res = Multiplexer.SelectNto1(inputs, path);
                break;
            default:
                Debug.LogError($"Multiplexer path {path} is an invalid value!");
                break;
        }

        // sinchronyse vizualisers and concrete objects
        _srcA.WriteEnable = registerSrcAVisualizer.isWriteEnabled;
        _srcB.WriteEnable = registerSrcBVisualizer.isWriteEnabled;
        _output.WriteEnable = registerOutputVisualizer.isWriteEnabled;

        _srcA.PreClockUpdate();
        _srcB.PreClockUpdate();

        _output.Input = res;
        _output.PreClockUpdate();

        // Only if WriteEnable = true, call Clock
        _srcA.Clock();
        _srcB.Clock();
        _output.Clock();

    }

    /*protected override bool IsStateEqual(object state) {
        if (!(state is LevelOneState s)) return false;

        return (s.RegisterAValue == srcA.Output) &&
                (s.RegisterBValue == srcB.Output) &&
                (s.OutputRegisterValue == output.Output) &&
                (s.CurrentChosenMuxPath == _multiplexerVisualizer.CurrentChosenMuxPath) &&
                (s.RegisterAWE == srcA.WriteEnable) &&
                (s.RegisterBWE == srcB.WriteEnable) &&
                (s.OutputRegisterWE == output.WriteEnable);
    }*/

    protected override bool CheckWinCondition() { 
        return (_output.Output == RightAnswerValue);
    }

    protected override IEnumerator RunBusVisualizations()
    {
        if (_currentBus == 0)
        {
            busController.StartBusSignal(busController.busSegments[0], _srcA.Output);
            busController.StartBusSignal(busController.busSegments[1], _srcB.Output);

            if(multiplexerVisualizer.CurrentChosenMuxPath != -1)
            {
                yield return StartCoroutine(DelayedBusSignal(busController.busSegments[2])); // ???
            }

            _currentBus++;

        }
        yield return new WaitUntil(() => busController.NoActiveSignals);

        //_busController.ResetFastForward();
    }

    protected override IEnumerator ReverseBusVisualizations()
    {
        if (_currentBus == 1)
        {
            if (multiplexerVisualizer.CurrentChosenMuxPath == -1)
            {
                yield return StartCoroutine(DelayedBusSignals(busController.busSegments[0], busController.busSegments[1]));
            }
            else {
                busController.StartBusSignal(busController.busSegments[2], _output.Input, true);

                yield return StartCoroutine(DelayedBusSignals(busController.busSegments[0], busController.busSegments[1]));
            }
            
            _currentBus--;
        }

        yield return new WaitUntil(() => busController.NoActiveSignals);

    }
    private IEnumerator DelayedBusSignal(LineRenderer busToStart)
    {
        yield return new WaitUntil(() => busController.NoActiveSignals);

        // sending the third signal
        var propagationVal = 0;
        if (multiplexerVisualizer.CurrentChosenMuxPath == 0)
        {
            propagationVal = _srcA.Output;
        }
        else if (multiplexerVisualizer.CurrentChosenMuxPath == 1)
        {
            propagationVal = _srcB.Output;
        }
        else {
            Debug.LogError($"Unexpected MUX path {multiplexerVisualizer.CurrentChosenMuxPath}");
        }

        busController.StartBusSignal(busToStart, propagationVal);
    }
    IEnumerator DelayedBusSignals(LineRenderer firstBusToStart, LineRenderer secondBusToStart)
    {
        yield return new WaitUntil(() => busController.NoActiveSignals);

        busController.StartBusSignal(firstBusToStart, 4, true);
        busController.StartBusSignal(secondBusToStart, 2, true);
    }

    #region
    protected override void BlockInGameInteractable()
    {
        registerSrcAVisualizer.UIRegisterPanel.WeButton.interactable = false;
        registerSrcBVisualizer.UIRegisterPanel.WeButton.interactable = false;
        registerOutputVisualizer.UIRegisterPanel.WeButton.interactable = false;


        multiplexerVisualizer.UIController.FirstWayButton.interactable = false;
        multiplexerVisualizer.UIController.SecondWayButton.interactable = false;
    }

    protected override void ReleaseInGameInteractable()
    {
        registerSrcAVisualizer.UIRegisterPanel.WeButton.interactable = true;
        registerSrcBVisualizer.UIRegisterPanel.WeButton.interactable = true;
        registerOutputVisualizer.UIRegisterPanel.WeButton.interactable = true;

        multiplexerVisualizer.UIController.FirstWayButton.interactable = true;
        multiplexerVisualizer.UIController.SecondWayButton.interactable = true;
    }
    #endregion
}
