using System.Collections;
using System.Security.Cryptography;
using UnityEngine;

public struct LevelZeroState
{
    public int RegisterAValue;
    public int RegisterBValue;
    public int OutputRegisterValue;

    public bool RegisterAWE;
    public bool RegisterBWE;
    public bool OutputRegisterWE;
}

public class LevelZeroRegisseur : BaseLevelRegisseur
{
    [Header("Level 0 Specific Components")]
    [SerializeField] private RegisterVizualizer _registerSrcAVisualizer;
    [SerializeField] private RegisterVizualizer _registerSrcBVisualizer;
    [SerializeField] private RegisterVizualizer _registerOutputVisualizer;

    [SerializeField] private int _srcAValue;
    [SerializeField] private int _srcBValue;

    #region CACHED UI REFERENCES
    private InfoPanelUI _infoSrcARegister;
    private InfoPanelUI _infoSrcBRegister;
    private InfoPanelUI _infoOutputRegister;
    #endregion

    // Intern components for computations
    private Register srcA;
    private Register srcB;
    private Register output;

    //protected override int RightAnswerValue => 4;

    private int _currentBus = 0; // [0, 2]

    protected override void OnLevelStart()
    {
        srcA = new Register(_srcAValue); srcA.WriteEnable = true;
        srcB = new Register(_srcBValue); srcB.WriteEnable = true;
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

    protected override void ApplyState(object state)
    {
        LevelZeroState s = (LevelZeroState)state;

        srcA = new Register(s.RegisterAValue);
        srcB = new Register(s.RegisterBValue);
        output = new Register(s.OutputRegisterValue);
        srcA.WriteEnable = s.RegisterAWE;
        srcB.WriteEnable = s.RegisterBWE;
        output.WriteEnable = s.OutputRegisterWE;
    }

    protected override void BlinkClockedComponents()
    {
        _registerSrcAVisualizer.TriggerBlink();
        _registerSrcBVisualizer.TriggerBlink();
        _registerOutputVisualizer.TriggerBlink();
    }

    protected override bool CheckWinCondition()
    {
        return (output.Output == RightAnswerValue);
    }

    protected override object GetCurrentState()
    {
        return new LevelZeroState
        {
            RegisterAValue = srcA.Output,
            RegisterBValue = srcB.Output,
            OutputRegisterValue = output.Output,
            RegisterAWE = srcA.WriteEnable,
            RegisterBWE = srcB.WriteEnable,
            OutputRegisterWE = output.WriteEnable
        };
    }

    protected override void HandleClockUpdate()
    {
        // sinchronyse vizualisers and concrete objects
        srcA.WriteEnable = _registerSrcAVisualizer.isWriteEnabled;
        srcB.WriteEnable = _registerSrcBVisualizer.isWriteEnabled;
        output.WriteEnable = _registerOutputVisualizer.isWriteEnabled;


        srcB.Input = srcA.Output;
        output.Input = srcB.Output;

        srcA.PreClockUpdate();
        srcB.PreClockUpdate();
        output.PreClockUpdate();

        // Only if WriteEnable = true, call Clock
        srcA.Clock();
        srcB.Clock();
        output.Clock();
    }

    /*protected override bool IsStateEqual(object state)
    {
        if (!(state is LevelZeroState s)) return false;

        return (s.RegisterAValue == srcA.Output) &&
                (s.RegisterBValue == srcB.Output) &&
                (s.OutputRegisterValue == output.Output) &&
                (s.RegisterAWE == srcA.WriteEnable) &&
                (s.RegisterBWE == srcB.WriteEnable) &&
                (s.OutputRegisterWE == output.WriteEnable);
    }*/

    protected override void UpdateVizualizers()
    {
        _infoSrcARegister.Display("Register 1", $"{srcA.Output}");
        _infoSrcBRegister.Display("Register 2", $"{srcB.Output}");
        _infoOutputRegister.Display("Register 3", $"{output.Output}");

        _registerSrcAVisualizer.ForceUpdateWriteEnableVisualization(srcA.WriteEnable);
        _registerSrcBVisualizer.ForceUpdateWriteEnableVisualization(srcB.WriteEnable);
        _registerOutputVisualizer.ForceUpdateWriteEnableVisualization(output.WriteEnable);
    }

    protected override IEnumerator RunBusVisualizations()
    {
        if (_currentBus == 0)
        {
            _busController.StartBusSignal(_busController.busSegments[0], srcA.Output);
        }
        else if (_currentBus == 1)
        {
            _busController.StartBusSignal(_busController.busSegments[1], srcB.Output);
        }

        _currentBus++;
        yield return new WaitUntil(() => _busController.NoActiveSignals);
    }

    protected override IEnumerator ReverseBusVisualizations()
    {
        if (_currentBus == 2)
        {
            _busController.StartBusSignal(_busController.busSegments[1], output.Input, true);
        }
        else if (_currentBus == 1)
        {
            _busController.StartBusSignal(_busController.busSegments[0], srcB.Input, true);
        }

        _currentBus--;
        yield return new WaitUntil(() => _busController.NoActiveSignals);
    }

    #region
    protected override void BlockIngameInteractables()
    {
        _registerSrcAVisualizer.UIRegisterPanel.WEButton.interactable = false;
        _registerSrcBVisualizer.UIRegisterPanel.WEButton.interactable = false;
        _registerOutputVisualizer.UIRegisterPanel.WEButton.interactable = false;
    }

    protected override void ReleaseIngameInteractables()
    {
        _registerSrcAVisualizer.UIRegisterPanel.WEButton.interactable = true;
        _registerSrcBVisualizer.UIRegisterPanel.WEButton.interactable = true;
        _registerOutputVisualizer.UIRegisterPanel.WEButton.interactable = true;
    }
    #endregion
}
