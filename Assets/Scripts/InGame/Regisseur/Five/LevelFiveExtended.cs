using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

public struct LevelFiveExtendedState
{
    public int RegisterValue;
    public int RegisterOutputValue;

    public bool RegisterWe;
    public bool RegisterOutputWe;

    public int ExtenderOperation;
}

public class LevelFiveExtended : BaseLevelRegisseur<LevelFiveExtendedState>
{
    [FormerlySerializedAs("_registerSrcAVisualizer")]
    [Header("Level 5 (Extended) Specific Components")]
    [SerializeField] private RegisterVisualizer registerSrcAVisualizer;
    [FormerlySerializedAs("_registerOutputVisualizer")] [SerializeField] private RegisterVisualizer registerOutputVisualizer;
    [FormerlySerializedAs("extenderVizualizer")] [FormerlySerializedAs("_extenderVizualizer")] [SerializeField] private ExtenderVisualizer extenderVisualizer;

    [FormerlySerializedAs("InputRegisterValue")] [SerializeField] private uint inputRegisterValue;

    #region CACHED UI REFERENCES
    private InfoPanelUI _infoSrcARegister;
    private InfoPanelUI _infoOutputRegister;
    #endregion

    private Register _srcA;
    private Register _output;

    protected int CurrentBus = 0;

    protected override void OnLevelStart()
    {
        _srcA = new Register((int)inputRegisterValue)
        {
            WriteEnable = true
        };
        _output = new Register()
        {
            WriteEnable = true
        };

        _infoSrcARegister = registerSrcAVisualizer.UIRegisterPanel;
        _infoOutputRegister = registerOutputVisualizer.UIRegisterPanel;

        UpdateVisualizers();
    }

    protected override void ApplyState(LevelFiveExtendedState s)
    {
        _srcA = new Register(s.RegisterValue);
        _output = new Register(s.RegisterOutputValue);

        _srcA.WriteEnable = s.RegisterWe;
        _output.WriteEnable = s.RegisterOutputWe;

        extenderVisualizer.ChooseAluOperation(s.ExtenderOperation);
    }

    protected override void BlinkClockedComponents()
    {
        registerSrcAVisualizer.TriggerBlink();
        registerOutputVisualizer.TriggerBlink();
    }

    protected override bool CheckWinCondition()
    {
        return _output.Output == RightAnswerValue;
    }

    protected override LevelFiveExtendedState GetCurrentState()
    {
        return new LevelFiveExtendedState
        {
            RegisterValue = _srcA.Output,
            RegisterOutputValue = _output.Output,

            RegisterWe = _srcA.WriteEnable,
            RegisterOutputWe = _output.WriteEnable,

            ExtenderOperation = extenderVisualizer.CurrentAluOperation,
        };
    }

    protected override void HandleClockUpdate()
    {
        // sinchronyse vizualisers and concrete objects
        _srcA.WriteEnable = registerSrcAVisualizer.isWriteEnabled;
        _output.WriteEnable = registerOutputVisualizer.isWriteEnabled;

        _output.Input = Extender.Evaluate(extenderVisualizer.CurrentAluOperation, (uint)_srcA.Output);


        _srcA.PreClockUpdate();
        _output.PreClockUpdate();

        _srcA.Clock();
        _output.Clock();
    }

    protected override IEnumerator ReverseBusVisualizations()
    {
        busController.StartBusSignal(busController.busSegments[0], _srcA.Output, true);
        busController.StartBusSignal(busController.busSegments[1], _output.Input, true);
        yield return new WaitUntil(() => busController.NoActiveSignals);
    }

    protected override IEnumerator RunBusVisualizations()
    {
        busController.StartBusSignal(busController.busSegments[0], _srcA.Output);
        busController.StartBusSignal(busController.busSegments[1], _output.Output);

        yield return new WaitUntil(() => busController.NoActiveSignals);
    }

    protected override void UpdateVisualizers()
    {
        _infoSrcARegister.Display("Register 1", RiscVDecoder.CommandBuilder((uint)_srcA.Output));
        _infoOutputRegister.Display("Register 2", $"{_output.Output}");
        registerSrcAVisualizer.ForceUpdateWriteEnableVisualization(_srcA.WriteEnable);
        registerOutputVisualizer.ForceUpdateWriteEnableVisualization(_output.WriteEnable);
    }
}
