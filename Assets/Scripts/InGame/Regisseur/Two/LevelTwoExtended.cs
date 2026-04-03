using System.Collections;
using UnityEngine;

public struct LevelTwoExtendedState
{
    public int RegisterAValue;
    public bool RegisterAWE;
    public int RegisterBValue;
    public bool RegisterBWE;
    public int OutputValue;
    public bool OutputWE;
    public int ALUOperation;
}

public class LevelTwoExtended : BaseLevelRegisseur
{
    [Header("Level 2 (Extender) Specific Components")]
    [SerializeField] private ALUVizualiser _aluVizualizer;
    [SerializeField] private RegisterVizualizer _registerSrcAVizualizer;
    [SerializeField] private RegisterVizualizer _registerSrcBVizualizer;
    [SerializeField] private RegisterVizualizer _registerOutputVizualizer;

    [SerializeField] private int srcAValue;
    [SerializeField] private int srcBValue;

    #region CACHED UI REFERENCES
    private InfoPanelUI _infoSrcARegister;
    private InfoPanelUI _infoSrcBRegister;
    private InfoPanelUI _infoOutputRegister;
    #endregion

    private Register srcA;
    private Register srcB;
    private Register output;

    private int _currentBus = 0; // [0, 1]

    protected override void OnLevelStart()
    {
        srcA = new Register(srcAValue); srcA.WriteEnable = true;
        srcB = new Register(srcBValue); srcB.WriteEnable = true;
        output = new Register(0); output.WriteEnable = true;

        _infoSrcARegister = _registerSrcAVizualizer.UIRegisterPanel;
        _infoSrcBRegister = _registerSrcBVizualizer.UIRegisterPanel;
        _infoOutputRegister = _registerOutputVizualizer.UIRegisterPanel;


        if (_levelTargetDescription == null || _levelTargetDescription.Length == 0)
        {
            _levelTargetText.text = $"Ziel: \r\nSchreibe in Register 3 den Wert {RightAnswerValue}";
        }
        else
        {
            _levelTargetText.text = _levelTargetDescription;
        }

        UpdateVizualizers();
    }

    protected override void ApplyState(object state)
    {
        LevelTwoExtendedState s = (LevelTwoExtendedState)state;

        srcA = new Register(s.RegisterAValue);
        srcA.WriteEnable = s.RegisterAWE;

        srcB = new Register(s.RegisterBValue);
        srcB.WriteEnable = s.RegisterBWE;

        output = new Register(s.OutputValue);
        output.WriteEnable = s.OutputWE;

        _aluVizualizer.ChooseALUOperation(s.ALUOperation);
    }

    protected override void BlinkClockedComponents()
    {
        _registerSrcAVizualizer.TriggerBlink();
        _registerSrcBVizualizer.TriggerBlink();
        _registerOutputVizualizer.TriggerBlink();
    }

    protected override void BlockIngameInteractables()
    {
        _registerSrcAVizualizer.UIRegisterPanel.WEButton.interactable = false;
        _registerSrcBVizualizer.UIRegisterPanel.WEButton.interactable = false;
        _registerOutputVizualizer.UIRegisterPanel.WEButton.interactable = false;

        _aluVizualizer.UIController.FirstOperationButton.interactable = false;
        _aluVizualizer.UIController.SecondOperationButton.interactable = false;
        _aluVizualizer.UIController.ThirdOperationButton.interactable = false;
        _aluVizualizer.UIController.FourthOperationButton.interactable = false;
    }

    protected override bool CheckWinCondition()
    {
        return (output.Output == RightAnswerValue);
    }

    protected override object GetCurrentState()
    {
        return new LevelTwoExtendedState
        {
            RegisterAValue = srcA.Output,
            RegisterAWE = srcA.WriteEnable,
            RegisterBValue = srcB.Output,
            RegisterBWE = srcB.WriteEnable,
            OutputValue = output.Output,
            OutputWE = output.WriteEnable,
            ALUOperation = _aluVizualizer.CurrentALUOperation,
        };
    }

    protected override void HandleClockUpdate()
    {
        // sinchronyse vizualisers and concrete objects
        srcA.WriteEnable = _registerSrcAVizualizer.isWriteEnabled;
        srcB.WriteEnable = _registerSrcBVizualizer.isWriteEnabled;
        output.WriteEnable = _registerOutputVizualizer.isWriteEnabled;

        output.Input = ALU.calculate(srcA.Output, srcB.Output, _aluVizualizer.CurrentALUOperation);
        srcA.PreClockUpdate();
        srcB.PreClockUpdate();
        output.PreClockUpdate();

        srcA.Clock();
        srcB.Clock();
        output.Clock();
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
        _registerSrcAVizualizer.UIRegisterPanel.WEButton.interactable = true;
        _registerSrcBVizualizer.UIRegisterPanel.WEButton.interactable = true;
        _registerOutputVizualizer.UIRegisterPanel.WEButton.interactable = true;

        _aluVizualizer.UIController.FirstOperationButton.interactable = true;
        _aluVizualizer.UIController.SecondOperationButton.interactable = true;
        _aluVizualizer.UIController.ThirdOperationButton.interactable = true;
        _aluVizualizer.UIController.FourthOperationButton.interactable = true;
    }

    protected override IEnumerator ReverseBusVisualizations()
    {
        if (_currentBus >= 1 && _currentBus <= _maxTickNumber)
        {
            _busController.StartBusSignal(_busController.busSegments[2], output.Input, true);

            yield return new WaitUntil(() => _busController.NoActiveSignals);

            _busController.StartBusSignal(_busController.busSegments[0], srcA.Output, true);
            _busController.StartBusSignal(_busController.busSegments[1], srcB.Output, true);

            _currentBus--;
        }
        yield return new WaitUntil(() => _busController.NoActiveSignals);
    }

    protected override IEnumerator RunBusVisualizations()
    {
        if (_currentBus >= 0 && _currentBus < _maxTickNumber)
        {
            _busController.StartBusSignal(_busController.busSegments[0], srcA.Output);
            _busController.StartBusSignal(_busController.busSegments[1], srcB.Output);

            yield return new WaitUntil(() => _busController.NoActiveSignals);

            _busController.StartBusSignal(_busController.busSegments[2], ALU.calculate(srcA.Output, srcB.Output, _aluVizualizer.CurrentALUOperation));

            _currentBus++;
        }
        yield return new WaitUntil(() => _busController.NoActiveSignals);
    }

    protected override void UpdateVizualizers()
    {
        _infoSrcARegister.Display("Register 1", $"{srcA.Output}");
        _infoSrcBRegister.Display("Register 2", $"{srcB.Output}");
        _infoOutputRegister.Display("Register 3", $"{output.Output}");

        _registerSrcAVizualizer.ForceUpdateWriteEnableVisualization(srcA.WriteEnable);
        _registerSrcBVizualizer.ForceUpdateWriteEnableVisualization(srcB.WriteEnable);
        _registerOutputVizualizer.ForceUpdateWriteEnableVisualization(output.WriteEnable);
    }
}
