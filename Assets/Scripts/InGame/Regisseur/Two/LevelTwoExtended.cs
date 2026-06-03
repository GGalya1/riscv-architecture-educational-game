using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

public struct LevelTwoExtendedState
{
    public int RegisterAValue;
    public bool RegisterAwe;
    public int RegisterBValue;
    public bool RegisterBwe;
    public int OutputValue;
    public bool OutputWe;
    public int AluOperation;
}

public class LevelTwoExtended : BaseLevelRegisseur
{
    [FormerlySerializedAs("_aluVizualizer")]
    [Header("Level 2 (Extender) Specific Components")]
    [SerializeField] private AluVisualiser aluVizualizer;
    [FormerlySerializedAs("registerSrcAVizualizer")] [FormerlySerializedAs("_registerSrcAVizualizer")] [SerializeField] private RegisterVisualizer registerSrcAVisualizer;
    [FormerlySerializedAs("registerSrcBVizualizer")] [FormerlySerializedAs("_registerSrcBVizualizer")] [SerializeField] private RegisterVisualizer registerSrcBVisualizer;
    [FormerlySerializedAs("registerOutputVizualizer")] [FormerlySerializedAs("_registerOutputVizualizer")] [SerializeField] private RegisterVisualizer registerOutputVisualizer;

    [SerializeField] private int srcAValue;
    [SerializeField] private int srcBValue;

    #region CACHED UI REFERENCES
    private InfoPanelUI _infoSrcARegister;
    private InfoPanelUI _infoSrcBRegister;
    private InfoPanelUI _infoOutputRegister;
    #endregion

    private Register _srcA;
    private Register _srcB;
    private Register _output;

    private int _currentBus; // [0, 1]

    protected override void OnLevelStart()
    {
        _srcA = new Register(srcAValue); _srcA.WriteEnable = true;
        _srcB = new Register(srcBValue); _srcB.WriteEnable = true;
        _output = new Register(0); _output.WriteEnable = true;

        _infoSrcARegister = registerSrcAVisualizer.UIRegisterPanel;
        _infoSrcBRegister = registerSrcBVisualizer.UIRegisterPanel;
        _infoOutputRegister = registerOutputVisualizer.UIRegisterPanel;


        if (levelTargetDescription == null || levelTargetDescription.Length == 0)
        {
            levelTargetText.text = $"Ziel: \r\nSchreibe in Register 3 den Wert {RightAnswerValue}";
        }
        else
        {
            levelTargetText.text = levelTargetDescription;
        }

        UpdateVizualizers();
    }

    protected override void ApplyState(object state)
    {
        var s = (LevelTwoExtendedState)state;

        _srcA = new Register(s.RegisterAValue);
        _srcA.WriteEnable = s.RegisterAwe;

        _srcB = new Register(s.RegisterBValue);
        _srcB.WriteEnable = s.RegisterBwe;

        _output = new Register(s.OutputValue);
        _output.WriteEnable = s.OutputWe;

        aluVizualizer.ChooseAluOperation(s.AluOperation);
    }

    protected override void BlinkClockedComponents()
    {
        registerSrcAVisualizer.TriggerBlink();
        registerSrcBVisualizer.TriggerBlink();
        registerOutputVisualizer.TriggerBlink();
    }

    protected override void BlockIngameInteractables()
    {
        registerSrcAVisualizer.UIRegisterPanel.WeButton.interactable = false;
        registerSrcBVisualizer.UIRegisterPanel.WeButton.interactable = false;
        registerOutputVisualizer.UIRegisterPanel.WeButton.interactable = false;

        aluVizualizer.uiController.FirstOperationButton.interactable = false;
        aluVizualizer.uiController.SecondOperationButton.interactable = false;
        aluVizualizer.uiController.ThirdOperationButton.interactable = false;
        aluVizualizer.uiController.FourthOperationButton.interactable = false;
    }

    protected override bool CheckWinCondition()
    {
        return (_output.Output == RightAnswerValue);
    }

    protected override object GetCurrentState()
    {
        return new LevelTwoExtendedState
        {
            RegisterAValue = _srcA.Output,
            RegisterAwe = _srcA.WriteEnable,
            RegisterBValue = _srcB.Output,
            RegisterBwe = _srcB.WriteEnable,
            OutputValue = _output.Output,
            OutputWe = _output.WriteEnable,
            AluOperation = aluVizualizer.CurrentAluOperation,
        };
    }

    protected override void HandleClockUpdate()
    {
        // sinchronyse vizualisers and concrete objects
        _srcA.WriteEnable = registerSrcAVisualizer.isWriteEnabled;
        _srcB.WriteEnable = registerSrcBVisualizer.isWriteEnabled;
        _output.WriteEnable = registerOutputVisualizer.isWriteEnabled;

        _output.Input = Alu.Calculate(_srcA.Output, _srcB.Output, aluVizualizer.CurrentAluOperation);
        _srcA.PreClockUpdate();
        _srcB.PreClockUpdate();
        _output.PreClockUpdate();

        _srcA.Clock();
        _srcB.Clock();
        _output.Clock();
    }

    /*protected override bool IsStateEqual(object state)
    {
        if (!(state is LevelTwoExtendedState s)) return false;

        return (s.RegisterAValue == srcA.Output) &&
            (s.RegisterAWE == srcA.WriteEnable) &&
            (s.RegisterBValue == srcB.Output) &&
            (s.RegisterBWE == srcB.WriteEnable) &&
            (s.OutputValue == output.Output) &&
            (s.OutputWE == output.WriteEnable) &&
            (s.ALUOperation == _aluVizualizer.CurrentALUOperation);
    }*/

    protected override void ReleaseIngameInteractables()
    {
        registerSrcAVisualizer.UIRegisterPanel.WeButton.interactable = true;
        registerSrcBVisualizer.UIRegisterPanel.WeButton.interactable = true;
        registerOutputVisualizer.UIRegisterPanel.WeButton.interactable = true;

        aluVizualizer.uiController.FirstOperationButton.interactable = true;
        aluVizualizer.uiController.SecondOperationButton.interactable = true;
        aluVizualizer.uiController.ThirdOperationButton.interactable = true;
        aluVizualizer.uiController.FourthOperationButton.interactable = true;
    }

    protected override IEnumerator ReverseBusVisualizations()
    {
        if (_currentBus >= 1 && _currentBus <= maxTickNumber)
        {
            busController.StartBusSignal(busController.busSegments[2], _output.Input, true);

            yield return new WaitUntil(() => busController.NoActiveSignals);

            busController.StartBusSignal(busController.busSegments[0], _srcA.Output, true);
            busController.StartBusSignal(busController.busSegments[1], _srcB.Output, true);

            _currentBus--;
        }
        yield return new WaitUntil(() => busController.NoActiveSignals);
    }

    protected override IEnumerator RunBusVisualizations()
    {
        if (_currentBus >= 0 && _currentBus < maxTickNumber)
        {
            busController.StartBusSignal(busController.busSegments[0], _srcA.Output);
            busController.StartBusSignal(busController.busSegments[1], _srcB.Output);

            yield return new WaitUntil(() => busController.NoActiveSignals);

            busController.StartBusSignal(busController.busSegments[2], Alu.Calculate(_srcA.Output, _srcB.Output, aluVizualizer.CurrentAluOperation));

            _currentBus++;
        }
        yield return new WaitUntil(() => busController.NoActiveSignals);
    }

    protected override void UpdateVizualizers()
    {
        _infoSrcARegister.Display("Register 1", $"{_srcA.Output}");
        _infoSrcBRegister.Display("Register 2", $"{_srcB.Output}");
        _infoOutputRegister.Display("Register 3", $"{_output.Output}");

        registerSrcAVisualizer.ForceUpdateWriteEnableVisualization(_srcA.WriteEnable);
        registerSrcBVisualizer.ForceUpdateWriteEnableVisualization(_srcB.WriteEnable);
        registerOutputVisualizer.ForceUpdateWriteEnableVisualization(_output.WriteEnable);
    }
}
