using System.Collections;
using UnityEngine;

public struct LevelTwoState
{
    public int RegisterAValue;
    public bool RegisterAWE;
    public int ALUOperation;
}

public class LevelTwoRegisseur : BaseLevelRegisseur
{
    [Header("Level 2 Specific Components")]
    [SerializeField] private ALUVizualiser _aluVizualizer;
    [SerializeField] private RegisterVizualizer _registerSrcAVisualizer;
    [SerializeField] private Blinker _numberBlinker;

    #region CACHED UI REFERENCES
    private InfoPanelUI _infoSrcARegister;
    #endregion

    private Register srcA;

    protected override int RightAnswerValue => 8;

    private int _currentBus = 0; // [0, 1]

    protected override void OnLevelStart()
    {
        srcA = new Register(1); srcA.WriteEnable = true;
        // second argument must be 4

        _infoSrcARegister = _registerSrcAVisualizer.UIRegisterPanel;


        if (_levelTargetDescription == null || _levelTargetDescription.Length == 0)
        {
            _levelTargetText.text = $"Ziel: \r\nSchreibe in Register 1 den Wert {RightAnswerValue}";
        }
        else
        {
            _levelTargetText.text = _levelTargetDescription;
        }

        UpdateVizualizers();
    }

    protected override void ApplyState(object state)
    {
        LevelTwoState s = (LevelTwoState)state;

        srcA = new Register(s.RegisterAValue);
        srcA.WriteEnable = s.RegisterAWE;
        
        _aluVizualizer.ChooseALUOperation(s.ALUOperation);
    }

    protected override void BlinkClockedComponents()
    {
        _registerSrcAVisualizer.TriggerBlink();
        _numberBlinker.Trigger();
    }

    protected override bool CheckWinCondition()
    {
        return (srcA.Output == RightAnswerValue);
    }

    protected override object GetCurrentState()
    {
        return new LevelTwoState
        {
            RegisterAValue = srcA.Output,
            RegisterAWE = srcA.WriteEnable,
            ALUOperation = _aluVizualizer.CurrentALUOperation,
        };
    }

    protected override void HandleClockUpdate()
    {
        // sinchronyse vizualisers and concrete objects
        srcA.WriteEnable = _registerSrcAVisualizer.isWriteEnabled;

        srcA.Input = ALU.calculate(srcA.Output, 4, _aluVizualizer.CurrentALUOperation);
        srcA.PreClockUpdate();
        srcA.Clock();
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
        _infoSrcARegister.Display("Register 1", $"{srcA.Output}");

        _registerSrcAVisualizer.ForceUpdateWriteEnableVisualization(srcA.WriteEnable);
    }

    #region busVizualisation
    protected override IEnumerator RunBusVisualizations()
    {
        if (_currentBus >= 0 && _currentBus < 5)
        {
            _busController.StartBusSignal(_busController.busSegments[0], srcA.Output);
            _busController.StartBusSignal(_busController.busSegments[1], 4);

            yield return StartCoroutine(DelayedBusSignal(_busController.busSegments[2]));

            _currentBus++;
        }
        yield return new WaitUntil(() => _busController.NoActiveSignals);
    }

    protected override IEnumerator ReverseBusVisualizations()
    {
        if (_currentBus >= 1 && _currentBus <= 5)
        {

            _busController.StartBusSignal(_busController.busSegments[2], srcA.Input, true);

             yield return StartCoroutine(DelayedBusSignals(_busController.busSegments[0], _busController.busSegments[1]));
            
            _currentBus--;
        }
        yield return new WaitUntil(() => _busController.NoActiveSignals);
    }
    IEnumerator DelayedBusSignal(LineRenderer busToStart)
    {
        yield return new WaitUntil(() => _busController.NoActiveSignals);

        _busController.StartBusSignal(busToStart, ALU.calculate(srcA.Output, 4, _aluVizualizer.CurrentALUOperation));
    }
    IEnumerator DelayedBusSignals(LineRenderer firstBusToStart, LineRenderer secondBusToStart)
    {
        yield return new WaitUntil(() => _busController.NoActiveSignals);

        if (_tickStateValues[_tickCounter] is LevelTwoState s) {
            _busController.StartBusSignal(firstBusToStart, s.RegisterAValue, true);
        }
        
        _busController.StartBusSignal(secondBusToStart, 4, true);
    }
    #endregion

    #region
    protected override void BlockIngameInteractables()
    {
        _registerSrcAVisualizer.UIRegisterPanel.WEButton.interactable = false;

        _aluVizualizer.UIController.FirstOperationButton.interactable = false;
        _aluVizualizer.UIController.SecondOperationButton.interactable = false;
        _aluVizualizer.UIController.ThirdOperationButton.interactable = false;
        _aluVizualizer.UIController.FourthOperationButton.interactable = false;
    }

    protected override void ReleaseIngameInteractables()
    {
        _registerSrcAVisualizer.UIRegisterPanel.WEButton.interactable = true;

        _aluVizualizer.UIController.FirstOperationButton.interactable = true;
        _aluVizualizer.UIController.SecondOperationButton.interactable = true;
        _aluVizualizer.UIController.ThirdOperationButton.interactable = true;
        _aluVizualizer.UIController.FourthOperationButton.interactable = true;
    }
    #endregion
}
