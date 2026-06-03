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

public class LevelFiveExtended : BaseLevelRegisseur
{
    [FormerlySerializedAs("_registerSrcAVisualizer")]
    [Header("Level 5 (Extended) Specific Components")]
    [SerializeField] private RegisterVisualizer registerSrcAVisualizer;
    [FormerlySerializedAs("_registerOutputVisualizer")] [SerializeField] private RegisterVisualizer registerOutputVisualizer;
    [FormerlySerializedAs("_extenderVizualizer")] [SerializeField] private ExtenderVisualizer extenderVizualizer;

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
        _srcA = new Register((int)inputRegisterValue); _srcA.WriteEnable = true;
        _output = new Register(0); _output.WriteEnable = true;

        _infoSrcARegister = registerSrcAVisualizer.UIRegisterPanel;
        _infoOutputRegister = registerOutputVisualizer.UIRegisterPanel;

        if (levelTargetDescription == null || levelTargetDescription.Length == 0)
        {
            levelTargetText.text = $"Ziel: \r\nErweitere korrekt den Wert aus dem Register 1 und schreib den Wert in Register 3.";
        }
        else
        {
            levelTargetText.text = levelTargetDescription;
        }

        UpdateVizualizers();
    }

    protected override void ApplyState(object state)
    {
        var s = (LevelFiveExtendedState)state;

        _srcA = new Register(s.RegisterValue);
        _output = new Register(s.RegisterOutputValue);

        _srcA.WriteEnable = s.RegisterWe;
        _output.WriteEnable = s.RegisterOutputWe;

        extenderVizualizer.ChooseAluOperation(s.ExtenderOperation);
    }

    protected override void BlinkClockedComponents()
    {
        registerSrcAVisualizer.TriggerBlink();
        registerOutputVisualizer.TriggerBlink();
    }

    protected override void BlockIngameInteractables()
    {
        registerSrcAVisualizer.UIRegisterPanel.WeButton.interactable = false;
        registerOutputVisualizer.UIRegisterPanel.WeButton.interactable = false;

        extenderVizualizer.uiController.FirstOperationButton.interactable = false;
        extenderVizualizer.uiController.SecondOperationButton.interactable = false;
        extenderVizualizer.uiController.ThirdOperationButton.interactable = false;
        extenderVizualizer.uiController.FourthOperationButton.interactable = false;
    }

    protected override bool CheckWinCondition()
    {
        return _output.Output == RightAnswerValue;
    }

    protected override object GetCurrentState()
    {
        return new LevelFiveExtendedState
        {
            RegisterValue = _srcA.Output,
            RegisterOutputValue = _output.Output,

            RegisterWe = _srcA.WriteEnable,
            RegisterOutputWe = _output.WriteEnable,

            ExtenderOperation = extenderVizualizer.CurrentAluOperation,
        };
    }

    protected override void HandleClockUpdate()
    {
        // sinchronyse vizualisers and concrete objects
        _srcA.WriteEnable = registerSrcAVisualizer.isWriteEnabled;
        _output.WriteEnable = registerOutputVisualizer.isWriteEnabled;

        _output.Input = Extender.Evaluate(extenderVizualizer.CurrentAluOperation, (uint)_srcA.Output);


        _srcA.PreClockUpdate();
        _output.PreClockUpdate();

        _srcA.Clock();
        _output.Clock();
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
        registerSrcAVisualizer.UIRegisterPanel.WeButton.interactable = true;
        registerOutputVisualizer.UIRegisterPanel.WeButton.interactable = true;

        extenderVizualizer.uiController.FirstOperationButton.interactable = true;
        extenderVizualizer.uiController.SecondOperationButton.interactable = true;
        extenderVizualizer.uiController.ThirdOperationButton.interactable = true;
        extenderVizualizer.uiController.FourthOperationButton.interactable = true;
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

    protected override void UpdateVizualizers()
    {
        _infoSrcARegister.Display("Register 1", CommandBuilder((uint)_srcA.Output));
        _infoOutputRegister.Display("Register 2", $"{_output.Output}");
        registerSrcAVisualizer.ForceUpdateWriteEnableVisualization(_srcA.WriteEnable);
        registerOutputVisualizer.ForceUpdateWriteEnableVisualization(_output.WriteEnable);
    }
}
