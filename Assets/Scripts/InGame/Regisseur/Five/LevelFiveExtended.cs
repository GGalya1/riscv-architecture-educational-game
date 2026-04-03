using System.Collections;
using UnityEngine;

public struct LevelFiveExtendedState
{
    public int RegisterValue;
    public int RegisterOutputValue;

    public bool RegisterWE;
    public bool RegisterOutputWE;

    public int ExtenderOperation;
}

public class LevelFiveExtended : BaseLevelRegisseur
{
    [Header("Level 5 (Extended) Specific Components")]
    [SerializeField] private RegisterVizualizer _registerSrcAVisualizer;
    [SerializeField] private RegisterVizualizer _registerOutputVisualizer;
    [SerializeField] private ExternderVizualizer _extenderVizualizer;

    [SerializeField] private uint InputRegisterValue;

    #region CACHED UI REFERENCES
    private InfoPanelUI _infoSrcARegister;
    private InfoPanelUI _infoOutputRegister;
    #endregion

    private Register srcA;
    private Register output;

    protected int _currentBus = 0;

    protected override void OnLevelStart()
    {
        srcA = new Register((int)InputRegisterValue); srcA.WriteEnable = true;
        output = new Register(0); output.WriteEnable = true;

        _infoSrcARegister = _registerSrcAVisualizer.UIRegisterPanel;
        _infoOutputRegister = _registerOutputVisualizer.UIRegisterPanel;

        if (_levelTargetDescription == null || _levelTargetDescription.Length == 0)
        {
            _levelTargetText.text = $"Ziel: \r\nErweitere korrekt den Wert aus dem Register 1 und schreib den Wert in Register 3.";
        }
        else
        {
            _levelTargetText.text = _levelTargetDescription;
        }

        UpdateVizualizers();
    }

    protected override void ApplyState(object state)
    {
        LevelFiveExtendedState s = (LevelFiveExtendedState)state;

        srcA = new Register(s.RegisterValue);
        output = new Register(s.RegisterOutputValue);

        srcA.WriteEnable = s.RegisterWE;
        output.WriteEnable = s.RegisterOutputWE;

        _extenderVizualizer.ChooseALUOperation(s.ExtenderOperation);
    }

    protected override void BlinkClockedComponents()
    {
        _registerSrcAVisualizer.TriggerBlink();
        _registerOutputVisualizer.TriggerBlink();
    }

    protected override void BlockIngameInteractables()
    {
        _registerSrcAVisualizer.UIRegisterPanel.WEButton.interactable = false;
        _registerOutputVisualizer.UIRegisterPanel.WEButton.interactable = false;

        _extenderVizualizer.UIController.FirstOperationButton.interactable = false;
        _extenderVizualizer.UIController.SecondOperationButton.interactable = false;
        _extenderVizualizer.UIController.ThirdOperationButton.interactable = false;
        _extenderVizualizer.UIController.FourthOperationButton.interactable = false;
    }

    protected override bool CheckWinCondition()
    {
        return output.Output == RightAnswerValue;
    }

    protected override object GetCurrentState()
    {
        return new LevelFiveExtendedState
        {
            RegisterValue = srcA.Output,
            RegisterOutputValue = output.Output,

            RegisterWE = srcA.WriteEnable,
            RegisterOutputWE = output.WriteEnable,

            ExtenderOperation = _extenderVizualizer.CurrentALUOperation,
        };
    }

    protected override void HandleClockUpdate()
    {
        // sinchronyse vizualisers and concrete objects
        srcA.WriteEnable = _registerSrcAVisualizer.isWriteEnabled;
        output.WriteEnable = _registerOutputVisualizer.isWriteEnabled;

        output.Input = Extender.Evaluate(_extenderVizualizer.CurrentALUOperation, (uint)srcA.Output);


        srcA.PreClockUpdate();
        output.PreClockUpdate();

        srcA.Clock();
        output.Clock();
    }

    /*protected override bool IsStateEqual(object state)
    {
        if (!(state is LevelFiveExtendedState s)) return false;

        return (s.RegisterValue == srcA.Output) &&
                (s.RegisterOutputValue == output.Output) &&

                (s.RegisterWE == srcA.WriteEnable) &&
                (s.RegisterOutputWE == output.WriteEnable) &&

                (s.ExtenderOperation == _extenderVizualizer.CurrentALUOperation);
    }*/

    protected override void ReleaseIngameInteractables()
    {
        _registerSrcAVisualizer.UIRegisterPanel.WEButton.interactable = true;
        _registerOutputVisualizer.UIRegisterPanel.WEButton.interactable = true;

        _extenderVizualizer.UIController.FirstOperationButton.interactable = true;
        _extenderVizualizer.UIController.SecondOperationButton.interactable = true;
        _extenderVizualizer.UIController.ThirdOperationButton.interactable = true;
        _extenderVizualizer.UIController.FourthOperationButton.interactable = true;
    }

    protected override IEnumerator ReverseBusVisualizations()
    {
        _busController.StartBusSignal(_busController.busSegments[0], srcA.Output, true);
        _busController.StartBusSignal(_busController.busSegments[1], output.Input, true);
        yield return new WaitUntil(() => _busController.NoActiveSignals);
    }

    protected override IEnumerator RunBusVisualizations()
    {
        _busController.StartBusSignal(_busController.busSegments[0], srcA.Output);
        _busController.StartBusSignal(_busController.busSegments[1], output.Output);

        yield return new WaitUntil(() => _busController.NoActiveSignals);
    }

    protected override void UpdateVizualizers()
    {
        _infoSrcARegister.Display("Register 1", commandBuilder((uint)srcA.Output));
        _infoOutputRegister.Display("Register 2", $"{output.Output}");
        _registerSrcAVisualizer.ForceUpdateWriteEnableVisualization(srcA.WriteEnable);
        _registerOutputVisualizer.ForceUpdateWriteEnableVisualization(output.WriteEnable);
    }
}
