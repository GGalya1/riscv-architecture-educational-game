using System.Collections;
using UnityEngine;

public struct LevelSevenState
{
    public int RegisterAValue;
    public int RegisterBValue;
    public int RegisterOutputValue;

    public bool RegisterAWE;
    public bool RegisterBWE;
    public bool RegisterOutputWE;
    public bool RegisterFileWE;

    public int ALUOperation;
}

public class LevelSevenRegisseur : BaseLevelRegisseur
{
    [Header("Level 7 Specific Components")]
    [SerializeField] protected RegisterVizualizer _registerSrcAVisualizer;
    [SerializeField] protected RegisterVizualizer _registerSrcBVisualizer;
    [SerializeField] protected RegisterVizualizer _registerOutputVisualizer;
    [SerializeField] protected RegisterFileVizualizer _registerFileVisualizer;
    [SerializeField] protected ALUVizualiser _aluVizualizer;

    #region CACHED UI REFERENCES
    protected InfoPanelUI _infoSrcARegister;
    protected InfoPanelUI _infoSrcBRegister;
    protected InfoPanelUI _infoOutputRegister;
    #endregion

    // Intern components for computations
    protected Register srcA;
    protected Register srcB;
    protected Register output;
    protected RegisterFile registerFile;

    protected override int RightAnswerValue => 42;


    protected int _currentBus = 0; // [0, 2]

    protected override void OnLevelStart()
    {
        // Initialization of logical components
        srcA = new Register(6); srcA.WriteEnable = true;
        srcB = new Register(7); srcB.WriteEnable = true;
        output = new Register(0); output.WriteEnable = true;

        registerFile = new RegisterFile(); registerFile.RegisterWriteEnable = true;
        registerFile.InitializeRegisters(new int[] { 0, 1, 39, 43, 5, 6, 2,
                                                     40, 1, 39, 13, 56, 64, 20,
                                                     50, 51, 0, 12, 53, 65, 29,
                                                     60, 61, 0, 1, 54, 0, 28,
                                                     70, 30, 31, 0});

        // Caching of UI panels for visualizers
        _infoSrcARegister = _registerSrcAVisualizer.UIRegisterPanel;
        _infoSrcBRegister = _registerSrcBVisualizer.UIRegisterPanel;
        _infoOutputRegister = _registerOutputVisualizer.UIRegisterPanel;


        if (_levelTargetDescription == null || _levelTargetDescription.Length == 0)
        {
            _levelTargetText.text = $"Ziel: \r\nSchreibe in Register 3 die Summe von r7 und r6.";
        }
        else
        {
            _levelTargetText.text = _levelTargetDescription;
        }
        

        UpdateVizualizers();
        UpdateRegisterFileVizualization();
    }

    protected override void ApplyState(object state)
    {
        LevelSevenState s = (LevelSevenState)state;

        srcA = new Register(s.RegisterAValue);
        srcB = new Register(s.RegisterBValue);
        output = new Register(s.RegisterOutputValue);

        srcA.WriteEnable = s.RegisterAWE;
        srcB.WriteEnable = s.RegisterBWE;
        output.WriteEnable = s.RegisterOutputWE;
        registerFile.RegisterWriteEnable = s.RegisterFileWE;

        _aluVizualizer.ChooseALUOperation(s.ALUOperation);
    }

    protected override void BlinkClockedComponents()
    {
        _registerSrcAVisualizer.TriggerBlink();
        _registerSrcBVisualizer.TriggerBlink();
        _registerOutputVisualizer.TriggerBlink();

        _registerFileVisualizer.TriggerBlink();
    }

    protected override void BlockIngameInteractables()
    {
        _registerSrcAVisualizer.UIRegisterPanel.WEButton.interactable = false;
        _registerSrcBVisualizer.UIRegisterPanel.WEButton.interactable = false;
        _registerOutputVisualizer.UIRegisterPanel.WEButton.interactable = false;

        _registerFileVisualizer.UIRegisterPanel.WEButton.interactable = false;

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
        return new LevelSevenState
        {
            RegisterAValue = srcA.Output,
            RegisterBValue = srcB.Output,
            RegisterOutputValue = output.Output,

            RegisterAWE = srcA.WriteEnable,
            RegisterBWE = srcB.WriteEnable,
            RegisterOutputWE = output.WriteEnable,
            RegisterFileWE = registerFile.RegisterWriteEnable,

            ALUOperation = _aluVizualizer.CurrentALUOperation,
        };
    }

    protected override void HandleClockUpdate()
    {
        // sinchronyse vizualisers and concrete objects
        srcA.WriteEnable = _registerSrcAVisualizer.isWriteEnabled;
        srcB.WriteEnable = _registerSrcBVisualizer.isWriteEnabled;
        output.WriteEnable = _registerOutputVisualizer.isWriteEnabled;
        registerFile.RegisterWriteEnable = _registerFileVisualizer.isWriteEnabled;

        // implementation
        registerFile.ReadAdress1 = srcA.Output;
        registerFile.ReadAdress2 = srcB.Output;
        output.Input = ALU.calculate(registerFile.ReadData1, registerFile.ReadData2, _aluVizualizer.CurrentALUOperation);

        srcA.PreClockUpdate();
        srcB.PreClockUpdate();
        output.PreClockUpdate();
        registerFile.PreClockUpdate();


        // Only if WriteEnable = true, call Clock
        srcA.Clock();
        srcB.Clock();
        output.Clock();
        registerFile.Clock();
    }

    /*protected override bool IsStateEqual(object state)
    {
        if (!(state is LevelSevenState s)) return false;

        return (s.RegisterAValue == srcA.Output) &&
                (s.RegisterBValue == srcB.Output) &&
                (s.RegisterOutputValue == output.Output) &&

                (s.RegisterAWE == srcA.WriteEnable) &&
                (s.RegisterBWE == srcB.WriteEnable) &&
                (s.RegisterOutputWE == output.WriteEnable) &&
                (s.RegisterFileWE == registerFile.RegisterWriteEnable) &&

                (s.ALUOperation == _aluVizualizer.CurrentALUOperation);
    }*/

    protected override void ReleaseIngameInteractables()
    {
        _registerSrcAVisualizer.UIRegisterPanel.WEButton.interactable = true;
        _registerSrcBVisualizer.UIRegisterPanel.WEButton.interactable = true;
        _registerOutputVisualizer.UIRegisterPanel.WEButton.interactable = true;

        _registerFileVisualizer.UIRegisterPanel.WEButton.interactable = true;

        _aluVizualizer.UIController.FirstOperationButton.interactable = true;
        _aluVizualizer.UIController.SecondOperationButton.interactable = true;
        _aluVizualizer.UIController.ThirdOperationButton.interactable = true;
        _aluVizualizer.UIController.FourthOperationButton.interactable = true;
    }

    protected override IEnumerator ReverseBusVisualizations()
    {
        if (_currentBus >= 1 && _currentBus <= _maxTickNumber)
        {
            _busController.StartBusSignal(_busController.busSegments[4], output.Input, true);
            yield return new WaitUntil(() => _busController.NoActiveSignals);

            if (_tickStateValues[_tickCounter] is LevelSevenState s)
            {
                _busController.StartBusSignal(_busController.busSegments[2], registerFile.Registers[s.RegisterAValue], true);
                _busController.StartBusSignal(_busController.busSegments[3], registerFile.Registers[s.RegisterBValue], true);
                yield return new WaitUntil(() => _busController.NoActiveSignals);

                _busController.StartBusSignal(_busController.busSegments[0], s.RegisterAValue, true);
                _busController.StartBusSignal(_busController.busSegments[1], s.RegisterBValue, true);
            }
            

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

            _busController.StartBusSignal(_busController.busSegments[2], registerFile.ReadData1);
            _busController.StartBusSignal(_busController.busSegments[3], registerFile.ReadData2);
            yield return new WaitUntil(() => _busController.NoActiveSignals);

            _busController.StartBusSignal(_busController.busSegments[4], ALU.calculate(registerFile.ReadData1, registerFile.ReadData2, _aluVizualizer.CurrentALUOperation));

            _currentBus++;
        }

        yield return new WaitUntil(() => _busController.NoActiveSignals);
    }

    protected override void UpdateVizualizers()
    {
        _infoSrcARegister.Display("Register 1", $"{srcA.Output}");
        _infoSrcBRegister.Display("Register 2", $"{srcB.Output}");
        _infoOutputRegister.Display("Register 3", $"{output.Output}");

        _registerSrcAVisualizer.ForceUpdateWriteEnableVisualization(srcA.WriteEnable);
        _registerSrcBVisualizer.ForceUpdateWriteEnableVisualization(srcB.WriteEnable);
        _registerOutputVisualizer.ForceUpdateWriteEnableVisualization(output.WriteEnable);
        _registerFileVisualizer.ForceUpdateWriteEnableVisualization(registerFile.RegisterWriteEnable);
    }

    private void UpdateRegisterFileVizualization() {
        _registerFileVisualizer.UIRegisterPanel.Display(registerFile.Registers);
    }
}
