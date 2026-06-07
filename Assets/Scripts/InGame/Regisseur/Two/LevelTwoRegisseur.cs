using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

public struct LevelTwoState
{
    public int RegisterAValue;
    public bool RegisterAwe;
    public int AluOperation;
}

public class LevelTwoRegisseur : BaseLevelRegisseur<LevelTwoState>
{
    [FormerlySerializedAs("_aluVizualizer")]
    [Header("Level 2 Specific Components")]
    [SerializeField] private AluVisualiser aluVisualizer;
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
        _srcA = new Register(1)
        {
            WriteEnable = true
        };
        // second argument must be 4

        _infoSrcARegister = registerSrcAVisualizer.UIRegisterPanel;

        UpdateVisualizers();
    }

    protected override void ApplyState(LevelTwoState s)
    {
        _srcA.Reset(s.RegisterAValue);
        _srcA.WriteEnable = s.RegisterAwe;

        aluVisualizer.ChooseAluOperation(s.AluOperation);
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

    protected override LevelTwoState GetCurrentState()
    {
        return new LevelTwoState
        {
            RegisterAValue = _srcA.Output,
            RegisterAwe = _srcA.WriteEnable,
            AluOperation = aluVisualizer.CurrentAluOperation,
        };
    }

    protected override void HandleClockUpdate()
    {
        // synchronize visualizers and concrete objects
        _srcA.WriteEnable = registerSrcAVisualizer.isWriteEnabled;

        _srcA.Input = Alu.Calculate(_srcA.Output, 4, aluVisualizer.CurrentAluOperation);
        _srcA.PreClockUpdate();
        _srcA.Clock();
    }

    protected override void UpdateVisualizers()
    {
        _infoSrcARegister.Display("Register 1", $"{_srcA.Output}");

        registerSrcAVisualizer.ForceUpdateWriteEnableVisualization(_srcA.WriteEnable);
    }

    #region busVizualisation
    protected override IEnumerator RunBusVisualizations()
    {
        if (_currentBus is >= 0 and < 5)
        {
            busController.StartBusSignal(busController.busSegments[0], _srcA.Output);
            busController.StartBusSignal(busController.busSegments[1], 4);

            yield return StartCoroutine(DelayedSignal(
                busController.busSegments[2],
                Alu.Calculate(_srcA.Output, 4, aluVisualizer.CurrentAluOperation)
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

            var prevVal = TickStateValues[TickCounter] is var s ? s.RegisterAValue : 0;

            yield return StartCoroutine(DelayedSignals(
                busController.busSegments[0], prevVal,
                busController.busSegments[1], 4, true, true
            ));
            
            _currentBus--;
        }
        yield return new WaitUntil(() => busController.NoActiveSignals);
    }
    #endregion
}
