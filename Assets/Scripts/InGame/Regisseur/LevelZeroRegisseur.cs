using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

public struct LevelZeroState
{
    public int RegisterAValue;
    public int RegisterBValue;
    public int OutputRegisterValue;

    public bool RegisterAwe;
    public bool RegisterBwe;
    public bool OutputRegisterWe;
}

public class LevelZeroRegisseur : BaseLevelRegisseur<LevelZeroState>
{
    [FormerlySerializedAs("_registerSrcAVisualizer")]
    [Header("Level 0 Specific Components")]
    [SerializeField] private RegisterVisualizer registerSrcAVisualizer;
    [FormerlySerializedAs("_registerSrcBVisualizer")] [SerializeField] private RegisterVisualizer registerSrcBVisualizer;
    [FormerlySerializedAs("_registerOutputVisualizer")] [SerializeField] private RegisterVisualizer registerOutputVisualizer;

    [FormerlySerializedAs("_srcAValue")] [SerializeField] private int srcAValue;
    [FormerlySerializedAs("_srcBValue")] [SerializeField] private int srcBValue;

    #region CACHED UI REFERENCES
    private InfoPanelUI _infoSrcARegister;
    private InfoPanelUI _infoSrcBRegister;
    private InfoPanelUI _infoOutputRegister;
    #endregion

    // Intern components for computations
    private Register _srcA;
    private Register _srcB;
    private Register _output;

    //protected override int RightAnswerValue => 4;

    private int _currentBus; // [0, 2]

    protected override void OnLevelStart()
    {
        _srcA = new Register(srcAValue)
        {
            WriteEnable = true
        };
        _srcB = new Register(srcBValue)
        {
            WriteEnable = true
        };
        _output = new Register
        {
            WriteEnable = true
        };

        // Caching of UI panels for visualizers
        _infoSrcARegister = registerSrcAVisualizer.UIRegisterPanel;
        _infoSrcBRegister = registerSrcBVisualizer.UIRegisterPanel;
        _infoOutputRegister = registerOutputVisualizer.UIRegisterPanel;

        UpdateVisualizers();
    }

    protected override void ApplyState(LevelZeroState s)
    {
        _srcA = new Register(s.RegisterAValue);
        _srcB = new Register(s.RegisterBValue);
        _output = new Register(s.OutputRegisterValue);
        _srcA.WriteEnable = s.RegisterAwe;
        _srcB.WriteEnable = s.RegisterBwe;
        _output.WriteEnable = s.OutputRegisterWe;
    }

    protected override void BlinkClockedComponents()
    {
        registerSrcAVisualizer.TriggerBlink();
        registerSrcBVisualizer.TriggerBlink();
        registerOutputVisualizer.TriggerBlink();
    }

    protected override bool CheckWinCondition()
    {
        return (_output.Output == RightAnswerValue);
    }

    protected override LevelZeroState GetCurrentState()
    {
        return new LevelZeroState
        {
            RegisterAValue = _srcA.Output,
            RegisterBValue = _srcB.Output,
            OutputRegisterValue = _output.Output,
            RegisterAwe = _srcA.WriteEnable,
            RegisterBwe = _srcB.WriteEnable,
            OutputRegisterWe = _output.WriteEnable
        };
    }

    protected override void HandleClockUpdate()
    {
        // synchronize visualizers and concrete objects
        _srcA.WriteEnable = registerSrcAVisualizer.isWriteEnabled;
        _srcB.WriteEnable = registerSrcBVisualizer.isWriteEnabled;
        _output.WriteEnable = registerOutputVisualizer.isWriteEnabled;


        _srcB.Input = _srcA.Output;
        _output.Input = _srcB.Output;

        _srcA.PreClockUpdate();
        _srcB.PreClockUpdate();
        _output.PreClockUpdate();

        // Only if WriteEnable = true, call Clock
        _srcA.Clock();
        _srcB.Clock();
        _output.Clock();
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

    protected override void UpdateVisualizers()
    {
        _infoSrcARegister.Display("Register 1", $"{_srcA.Output}");
        _infoSrcBRegister.Display("Register 2", $"{_srcB.Output}");
        _infoOutputRegister.Display("Register 3", $"{_output.Output}");

        registerSrcAVisualizer.ForceUpdateWriteEnableVisualization(_srcA.WriteEnable);
        registerSrcBVisualizer.ForceUpdateWriteEnableVisualization(_srcB.WriteEnable);
        registerOutputVisualizer.ForceUpdateWriteEnableVisualization(_output.WriteEnable);
    }

    protected override IEnumerator RunBusVisualizations()
    {
        if (_currentBus == 0)
        {
            busController.StartBusSignal(busController.busSegments[0], _srcA.Output);
        }
        else if (_currentBus == 1)
        {
            busController.StartBusSignal(busController.busSegments[1], _srcB.Output);
        }

        _currentBus++;
        yield return new WaitUntil(() => busController.NoActiveSignals);
    }

    protected override IEnumerator ReverseBusVisualizations()
    {
        if (_currentBus == 2)
        {
            busController.StartBusSignal(busController.busSegments[1], _output.Input, true);
        }
        else if (_currentBus == 1)
        {
            busController.StartBusSignal(busController.busSegments[0], _srcB.Input, true);
        }

        _currentBus--;
        yield return new WaitUntil(() => busController.NoActiveSignals);
    }

    #region
    protected override void BlockInGameInteractable()
    {
        registerSrcAVisualizer.UIRegisterPanel.WeButton.interactable = false;
        registerSrcBVisualizer.UIRegisterPanel.WeButton.interactable = false;
        registerOutputVisualizer.UIRegisterPanel.WeButton.interactable = false;
    }

    protected override void ReleaseInGameInteractable()
    {
        registerSrcAVisualizer.UIRegisterPanel.WeButton.interactable = true;
        registerSrcBVisualizer.UIRegisterPanel.WeButton.interactable = true;
        registerOutputVisualizer.UIRegisterPanel.WeButton.interactable = true;
    }
    #endregion
}
