using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Collections;

public struct LevelOneState
{
    public int RegisterAValue;
    public int RegisterBValue;
    public int OutputRegisterValue;

    public bool RegisterAWE;
    public bool RegisterBWE;
    public bool OutputRegisterWE;

    public int CurrentChoosenMuxPath;
}

public class LevelOneRegisseur : BaseLevelRegisseur
{
    [Header("Level 1 Specific Components")]
    [SerializeField] private MuiltiplexerVizualizer _multiplexerVisualizer;
    [SerializeField] private RegisterVizualizer _registerSrcAVisualizer;
    [SerializeField] private RegisterVizualizer _registerSrcBVisualizer;
    [SerializeField] private RegisterVizualizer _registerOutputVisualizer;

    #region CACHED UI REFERENCES
    private InfoPanelUI _infoSrcARegister;
    private InfoPanelUI _infoSrcBRegister;
    private InfoPanelUI _infoOutputRegister;
    #endregion

    // Intern components for computations
    private Register srcA;
    private Register srcB;
    private Register output;

    // protected override int RightAnswerValue => 4;

    private int _currentBus = 0; // [0, 1]

    protected override void OnLevelStart()
    {
        // Initialization of logical components
        srcA = new Register(4); srcA.WriteEnable = true;
        srcB = new Register(2); srcB.WriteEnable = true;
        output = new Register(0); output.WriteEnable = true;

        // Кэширование UI-панелей визуализаторов
        _infoSrcARegister = _registerSrcAVisualizer.UIRegisterPanel;
        _infoSrcBRegister = _registerSrcBVisualizer.UIRegisterPanel;
        _infoOutputRegister = _registerOutputVisualizer.UIRegisterPanel;


        if (_levelTargetDescription == null || _levelTargetDescription.Length == 0)
        {
            _levelTargetText.text = "Ziel: \r\nSchreibe in Register 3 den Wert 4";
        }
        else
        {
            _levelTargetText.text = _levelTargetDescription;
        }

        UpdateVizualizers();
    }

    protected override void BlinkClockedComponents()
    {
        _registerSrcAVisualizer.TriggerBlink();
        _registerSrcBVisualizer.TriggerBlink();
        _registerOutputVisualizer.TriggerBlink();
    }
    protected override object GetCurrentState()
    {
        return new LevelOneState
        {
            RegisterAValue = srcA.Output,
            RegisterBValue = srcB.Output,
            OutputRegisterValue = output.Output,
            CurrentChoosenMuxPath = _multiplexerVisualizer.CurrentChoosenMuxPath,
            RegisterAWE = srcA.WriteEnable,
            RegisterBWE = srcB.WriteEnable,
            OutputRegisterWE = output.WriteEnable
        };
    }
    protected override void ApplyState(object state)
    {
        LevelOneState s = (LevelOneState)state;

        srcA = new Register(s.RegisterAValue);
        srcB = new Register(s.RegisterBValue);
        output = new Register(s.OutputRegisterValue);
        srcA.WriteEnable = s.RegisterAWE;
        srcB.WriteEnable = s.RegisterBWE;
        output.WriteEnable = s.OutputRegisterWE;

        int temp = s.CurrentChoosenMuxPath;
        if (temp == -1)
        {
            _multiplexerVisualizer.ResetVizualization();
        }
        else if (temp == 0)
        {
            _multiplexerVisualizer.SelectPath(0);
        }
        else if (temp == 1)
        {
            _multiplexerVisualizer.SelectPath(1);
        }
        else if (temp == 2)
        {
            _multiplexerVisualizer.SelectPath(2);
        }
        else
        {
            Debug.LogError($"Saved multiplexer value {temp} is not in [0, 2]");
        }
    }
    protected override void UpdateVizualizers()
    {
        _infoSrcARegister.Display("Register 1", $"{srcA.Output}");
        _infoSrcBRegister.Display("Register 2", $"{srcB.Output}");
        _infoOutputRegister.Display("Register 3", $"{output.Output}");

        _registerSrcAVisualizer.ForceUpdateWriteEnableVisualization(srcA.WriteEnable);
        _registerSrcBVisualizer.ForceUpdateWriteEnableVisualization(srcB.WriteEnable);
        _registerOutputVisualizer.ForceUpdateWriteEnableVisualization(output.WriteEnable);
    }

    protected override void HandleClockUpdate() {
        
        int path = _multiplexerVisualizer.CurrentChoosenMuxPath;
        int[] inputs = { srcA.Output, srcB.Output };
        int res = 0;
        
        if (path == -1)
        {
            Debug.LogError("Multiplexer path not selected (-1). Data will be lost.");
        }
        else if (path >= 0 && path <= 1)
        {
            res = Multiplexer.SelectNto1(inputs, path);
        }
        else {
            Debug.LogError($"Multiplexer path {path} is an invalid value!");
        }

        // sinchronyse vizualisers and concrete objects
        srcA.WriteEnable = _registerSrcAVisualizer.isWriteEnabled;
        srcB.WriteEnable = _registerSrcBVisualizer.isWriteEnabled;
        output.WriteEnable = _registerOutputVisualizer.isWriteEnabled;

        srcA.PreClockUpdate();
        srcB.PreClockUpdate();

        output.Input = res;
        output.PreClockUpdate();

        // Only if WriteEnable = true, call Clock
        srcA.Clock();
        srcB.Clock();
        output.Clock();

    }

    /*protected override bool IsStateEqual(object state) {
        if (!(state is LevelOneState s)) return false;

        return (s.RegisterAValue == srcA.Output) &&
                (s.RegisterBValue == srcB.Output) &&
                (s.OutputRegisterValue == output.Output) &&
                (s.CurrentChoosenMuxPath == _multiplexerVisualizer.CurrentChoosenMuxPath) &&
                (s.RegisterAWE == srcA.WriteEnable) &&
                (s.RegisterBWE == srcB.WriteEnable) &&
                (s.OutputRegisterWE == output.WriteEnable);
    }*/

    protected override bool CheckWinCondition() { 
        return (output.Output == RightAnswerValue);
    }

    protected override IEnumerator RunBusVisualizations()
    {
        if (_currentBus == 0)
        {
            _busController.StartBusSignal(_busController.busSegments[0], srcA.Output);
            _busController.StartBusSignal(_busController.busSegments[1], srcB.Output);

            if(_multiplexerVisualizer.CurrentChoosenMuxPath != -1)
            {
                yield return StartCoroutine(DelayedBusSignal(_busController.busSegments[2])); // ???
            }

            _currentBus++;

        }
        yield return new WaitUntil(() => _busController.NoActiveSignals);

        //_busController.ResetFastForward();
    }

    protected override IEnumerator ReverseBusVisualizations()
    {
        if (_currentBus == 1)
        {
            if (_multiplexerVisualizer.CurrentChoosenMuxPath == -1)
            {
                yield return StartCoroutine(DelayedBusSignals(_busController.busSegments[0], _busController.busSegments[1]));
            }
            else {
                _busController.StartBusSignal(_busController.busSegments[2], output.Input, true);

                yield return StartCoroutine(DelayedBusSignals(_busController.busSegments[0], _busController.busSegments[1]));
            }
            
            _currentBus--;
        }

        yield return new WaitUntil(() => _busController.NoActiveSignals);

    }
    IEnumerator DelayedBusSignal(LineRenderer busToStart)
    {
        yield return new WaitUntil(() => _busController.NoActiveSignals);

        // Ssending the third signal
        int propagationVal = 0;
        if (_multiplexerVisualizer.CurrentChoosenMuxPath == 0)
        {
            propagationVal = srcA.Output;
        }
        else if (_multiplexerVisualizer.CurrentChoosenMuxPath == 1)
        {
            propagationVal = srcB.Output;
        }
        else {
            Debug.LogError($"Unexpected MUX path {_multiplexerVisualizer.CurrentChoosenMuxPath}");
        }

        _busController.StartBusSignal(busToStart, propagationVal);
    }
    IEnumerator DelayedBusSignals(LineRenderer firstBusToStart, LineRenderer secondBusToStart)
    {
        yield return new WaitUntil(() => _busController.NoActiveSignals);

        _busController.StartBusSignal(firstBusToStart, 4, true);
        _busController.StartBusSignal(secondBusToStart, 2, true);
    }

    #region
    protected override void BlockIngameInteractables()
    {
        _registerSrcAVisualizer.UIRegisterPanel.WEButton.interactable = false;
        _registerSrcBVisualizer.UIRegisterPanel.WEButton.interactable = false;
        _registerOutputVisualizer.UIRegisterPanel.WEButton.interactable = false;


        _multiplexerVisualizer.UIController.FirstWayButton.interactable = false;
        _multiplexerVisualizer.UIController.SecondWayButton.interactable = false;
    }

    protected override void ReleaseIngameInteractables()
    {
        _registerSrcAVisualizer.UIRegisterPanel.WEButton.interactable = true;
        _registerSrcBVisualizer.UIRegisterPanel.WEButton.interactable = true;
        _registerOutputVisualizer.UIRegisterPanel.WEButton.interactable = true;

        _multiplexerVisualizer.UIController.FirstWayButton.interactable = true;
        _multiplexerVisualizer.UIController.SecondWayButton.interactable = true;
    }
    #endregion
}
