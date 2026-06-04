using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

public struct LevelTwoState
{
    public int RegisterAValue;
    public bool RegisterAwe;
    public int AluOperation;
}

public class LevelTwoRegisseur : BaseLevelRegisseur
{
    [FormerlySerializedAs("_aluVizualizer")]
    [Header("Level 2 Specific Components")]
    [SerializeField] private AluVisualiser aluVizualizer;
    [FormerlySerializedAs("_registerSrcAVisualizer")] [SerializeField] private RegisterVisualizer registerSrcAVisualizer;
    [FormerlySerializedAs("_numberBlinker")] [SerializeField] private Blinker numberBlinker;

    #region CACHED UI REFERENCES
    private InfoPanelUI _infoSrcARegister;
    #endregion

    private Register _srcA;

    protected override int RightAnswerValue => 8;

    private int _currentBus; // [0, 1]

    protected override void OnLevelStart()
    {
        _srcA = new Register(1); _srcA.WriteEnable = true;
        // second argument must be 4

        _infoSrcARegister = registerSrcAVisualizer.UIRegisterPanel;


        SetLevelTargetText(levelTargetDescription);

        UpdateVizualizers();
    }

    protected override void ApplyState(object state)
    {
        var s = (LevelTwoState)state;

        _srcA = new Register(s.RegisterAValue);
        _srcA.WriteEnable = s.RegisterAwe;
        
        aluVizualizer.ChooseAluOperation(s.AluOperation);
    }

    protected override void BlinkClockedComponents()
    {
        registerSrcAVisualizer.TriggerBlink();
        numberBlinker.Trigger();
    }

    protected override bool CheckWinCondition()
    {
        return (_srcA.Output == RightAnswerValue);
    }

    protected override object GetCurrentState()
    {
        return new LevelTwoState
        {
            RegisterAValue = _srcA.Output,
            RegisterAwe = _srcA.WriteEnable,
            AluOperation = aluVizualizer.CurrentAluOperation,
        };
    }

    protected override void HandleClockUpdate()
    {
        // sinchronyse vizualisers and concrete objects
        _srcA.WriteEnable = registerSrcAVisualizer.isWriteEnabled;

        _srcA.Input = Alu.Calculate(_srcA.Output, 4, aluVizualizer.CurrentAluOperation);
        _srcA.PreClockUpdate();
        _srcA.Clock();
    }

    /*protected override bool IsStateEqual(object state)
    {
        if (!(state is LevelTwoState s)) return false;

        return (s.RegisterAValue == srcA.Output) && 
            (s.RegisterAWE == srcA.WriteEnable) &&
            (s.ALUOperation == _aluVizualizer.CurrentALUOperation);
    }*/

    protected override void UpdateVizualizers()
    {
        _infoSrcARegister.Display("Register 1", $"{_srcA.Output}");

        registerSrcAVisualizer.ForceUpdateWriteEnableVisualization(_srcA.WriteEnable);
    }

    #region busVizualisation
    protected override IEnumerator RunBusVisualizations()
    {
        if (_currentBus >= 0 && _currentBus < 5)
        {
            busController.StartBusSignal(busController.busSegments[0], _srcA.Output);
            busController.StartBusSignal(busController.busSegments[1], 4);

            yield return StartCoroutine(DelayedSignal(
                busController.busSegments[2],
                Alu.Calculate(_srcA.Output, 4, aluVizualizer.CurrentAluOperation)
            ));

            _currentBus++;
        }
        yield return new WaitUntil(() => busController.NoActiveSignals);
    }

    protected override IEnumerator ReverseBusVisualizations()
    {
        if (_currentBus is >= 1 and <= 5)
        {

            busController.StartBusSignal(busController.busSegments[2], _srcA.Input, true);

            var prevVal = TickStateValues[TickCounter] is LevelTwoState s ? s.RegisterAValue : 0;

            yield return StartCoroutine(DelayedSignals(
                busController.busSegments[0], prevVal,
                busController.busSegments[1], 4, true, true
            ));
            
            _currentBus--;
        }
        yield return new WaitUntil(() => busController.NoActiveSignals);
    }
    /*IEnumerator DelayedBusSignal(LineRenderer busToStart)
    {
        yield return new WaitUntil(() => busController.NoActiveSignals);

        busController.StartBusSignal(busToStart, Alu.Calculate(_srcA.Output, 4, aluVizualizer.CurrentAluOperation));
    }
    IEnumerator DelayedBusSignals(LineRenderer firstBusToStart, LineRenderer secondBusToStart)
    {
        yield return new WaitUntil(() => busController.NoActiveSignals);

        if (TickStateValues[TickCounter] is LevelTwoState s) {
            busController.StartBusSignal(firstBusToStart, s.RegisterAValue, true);
        }
        
        busController.StartBusSignal(secondBusToStart, 4, true);
    }*/
    #endregion

    #region
    protected override void BlockIngameInteractables()
    {
        registerSrcAVisualizer.UIRegisterPanel.WeButton.interactable = false;

        aluVizualizer.uiController.FirstOperationButton.interactable = false;
        aluVizualizer.uiController.SecondOperationButton.interactable = false;
        aluVizualizer.uiController.ThirdOperationButton.interactable = false;
        aluVizualizer.uiController.FourthOperationButton.interactable = false;
    }

    protected override void ReleaseIngameInteractables()
    {
        registerSrcAVisualizer.UIRegisterPanel.WeButton.interactable = true;

        aluVizualizer.uiController.FirstOperationButton.interactable = true;
        aluVizualizer.uiController.SecondOperationButton.interactable = true;
        aluVizualizer.uiController.ThirdOperationButton.interactable = true;
        aluVizualizer.uiController.FourthOperationButton.interactable = true;
    }
    #endregion
}
