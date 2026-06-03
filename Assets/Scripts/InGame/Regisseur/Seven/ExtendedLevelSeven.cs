using System.Collections;
using System.Security.Cryptography;
using UnityEngine;

public struct ExtendedSevenLevelState
{
    public int RegisterSrcAValue;
    public int RegisterSrcBValue;
    public int RegisterA3Value;
    public int RegisterWD3Value;

    public int[] RegisterFieldValue;

    public bool RegisterSrcAWE;
    public bool RegisterSrcBWE;
    public bool RegisterA3WE;
    public bool RegisterWD3WE;

    public int ALUOperation;
}

public class ExtendedLevelSeven : BaseLevelRegisseur
{
    [Header("Level Seven Components")]
    [SerializeField] protected RegisterVizualizer _registerSrcAVisualizer;
    [SerializeField] protected RegisterVizualizer _registerSrcBVisualizer;
    [SerializeField] protected RegisterVizualizer _registerA3Visualizer;
    [SerializeField] protected RegisterVizualizer _registerWD3Visualizer;

    [SerializeField] protected ALUVizualiser _aluVizualizer;

    [SerializeField] protected RegisterFileVizualizer _registerFileVisualizer;

    #region CACHED UI REFERENCES
    protected InfoPanelUI _infoSrcARegister;
    protected InfoPanelUI _infoSrcBRegister;
    protected InfoPanelUI _infoA3Register;
    protected InfoPanelUI _infoWD3Register;
    #endregion

    // Intern components for computations
    protected Register srcA;
    protected Register srcB;
    protected Register a3;
    protected Register wd3;

    protected RegisterFile registerFile;


    protected int _currentBus = 0; // [0, 10]

    protected override void OnLevelStart()
    {
        srcA = new Register(2); srcA.WriteEnable = true;
        srcB = new Register(8); srcB.WriteEnable = true;
        a3 = new Register(0); a3.WriteEnable = true;
        wd3 = new Register(0); wd3.WriteEnable = true;

        registerFile = new RegisterFile(); registerFile.RegisterWriteEnable = true;
        registerFile.InitializeRegisters(new int[] { 0, 1, 39, 43, 5, 6, 8,
                                                     40, 3, 39, 13, 56, 64, 20,
                                                     50, 51, 0, 12, 53, 65, 29,
                                                     60, 61, 0, 25, 54, 0, 28,
                                                     70, 30, 31, 0});

        // Caching of UI panels for visualizers
        _infoSrcARegister = _registerSrcAVisualizer.UIRegisterPanel;
        _infoSrcBRegister = _registerSrcBVisualizer.UIRegisterPanel;
        _infoA3Register = _registerA3Visualizer.UIRegisterPanel;
        _infoWD3Register = _registerWD3Visualizer.UIRegisterPanel;


        if (_levelTargetDescription == null || _levelTargetDescription.Length == 0)
        {
            _levelTargetText.text = $"Hier Ziel schreiben";
        }
        else
        {
            _levelTargetText.text = _levelTargetDescription;
        }

        UpdateVizualizers();
    }

    protected override void ApplyState(object state)
    {
        ExtendedSevenLevelState s = (ExtendedSevenLevelState)state;

        srcA = new Register(s.RegisterSrcAValue);
        srcB = new Register(s.RegisterSrcBValue);
        a3 = new Register(s.RegisterA3Value);
        wd3 = new Register(s.RegisterWD3Value);

        registerFile.InitializeRegisters(s.RegisterFieldValue);

        srcA.WriteEnable = s.RegisterSrcAWE;
        srcB.WriteEnable = s.RegisterSrcBWE;
        a3.WriteEnable = s.RegisterA3WE;
        wd3.WriteEnable = s.RegisterWD3WE;

        _aluVizualizer.ChooseALUOperation(s.ALUOperation);
    }

    protected override void BlinkClockedComponents()
    {
        _registerSrcAVisualizer.TriggerBlink();
        _registerSrcBVisualizer.TriggerBlink();
        _registerA3Visualizer.TriggerBlink();
        _registerWD3Visualizer.TriggerBlink();

        _registerFileVisualizer.TriggerBlink();

    }

    protected override void BlockIngameInteractables()
    {
        _registerFileVisualizer.UIRegisterPanel.WEButton.interactable = false;

        _registerSrcAVisualizer.UIRegisterPanel.WEButton.interactable = false;
        _registerSrcBVisualizer.UIRegisterPanel.WEButton.interactable = false;
        _registerA3Visualizer.UIRegisterPanel.WEButton.interactable = false;
        _registerWD3Visualizer.UIRegisterPanel.WEButton.interactable = false;

        _aluVizualizer.UIController.FirstOperationButton.interactable = false;
        _aluVizualizer.UIController.SecondOperationButton.interactable = false;
        _aluVizualizer.UIController.ThirdOperationButton.interactable = false;
        _aluVizualizer.UIController.FourthOperationButton.interactable = false;
    }

    protected override bool CheckWinCondition()
    {
        return registerFile.Registers[0] == 42;
    }

    protected override object GetCurrentState()
    {
        return new ExtendedSevenLevelState {
            RegisterSrcAValue = srcA.Output,
            RegisterSrcBValue = srcB.Output,
            RegisterA3Value = a3.Output,
            RegisterWD3Value = wd3.Output,

            RegisterFieldValue = registerFile.Registers,

            RegisterSrcAWE = srcA.WriteEnable,
            RegisterSrcBWE = srcB.WriteEnable,
            RegisterA3WE = a3.WriteEnable,
            RegisterWD3WE = wd3.WriteEnable,

            ALUOperation = _aluVizualizer.CurrentALUOperation,
        };
    }

    protected override void HandleClockUpdate()
    {
        srcA.WriteEnable = _registerSrcAVisualizer.isWriteEnabled;
        srcB.WriteEnable = _registerSrcBVisualizer.isWriteEnabled;
        a3.WriteEnable = _registerA3Visualizer.isWriteEnabled;
        wd3.WriteEnable = _registerWD3Visualizer.isWriteEnabled;

        registerFile.RegisterWriteEnable = _registerFileVisualizer.isWriteEnabled;

        // implementation
        // A1: [19:15] (Register Source 1)
        registerFile.ReadAdress1 = srcA.Output;

        // A2: [24:20] (Register Source 2)
        registerFile.ReadAdress2 = srcB.Output;

        registerFile.ReadRegisters();

        // A3: [11:7] (Register Destination / rd)

        int a = registerFile.ReadData1;
        int b = registerFile.ReadData2;

        wd3.Input = ALU.calculate(a, b, _aluVizualizer.CurrentALUOperation);
        if (_tickCounter - 1 >= 0)
        {
            ExtendedSevenLevelState legcyState = (ExtendedSevenLevelState)_tickStateValues[_tickCounter - 1];

            registerFile.WriteAdress = legcyState.RegisterA3Value;
        }
        registerFile.WriteData = wd3.Output;


        srcA.PreClockUpdate();
        srcB.PreClockUpdate();
        a3.PreClockUpdate();
        wd3.PreClockUpdate();

        srcA.Clock();
        srcB.Clock();
        a3.Clock();
        wd3.Clock();
        registerFile.Clock();
    }

    protected override void ReleaseIngameInteractables()
    {
        _registerFileVisualizer.UIRegisterPanel.WEButton.interactable = true;

        _registerSrcAVisualizer.UIRegisterPanel.WEButton.interactable = true;
        _registerSrcBVisualizer.UIRegisterPanel.WEButton.interactable = true;
        _registerA3Visualizer.UIRegisterPanel.WEButton.interactable = true;
        _registerWD3Visualizer.UIRegisterPanel.WEButton.interactable = true;

        _aluVizualizer.UIController.FirstOperationButton.interactable = true;
        _aluVizualizer.UIController.SecondOperationButton.interactable = true;
        _aluVizualizer.UIController.ThirdOperationButton.interactable = true;
        _aluVizualizer.UIController.FourthOperationButton.interactable = true;
    }

    protected override IEnumerator ReverseBusVisualizations()
    {
        if (_currentBus >= 1 && _currentBus <= _maxTickNumber)
        {
            _busController.StartBusSignal(_busController.busSegments[6], wd3.Input, true);

            yield return new WaitUntil(() => _busController.NoActiveSignals);

            _busController.StartBusSignal(_busController.busSegments[5], wd3.Input, true);

            yield return new WaitUntil(() => _busController.NoActiveSignals);

            int a = 0;
            int b = 0;
            if (srcA.Output > 0 && srcA.Output < 16)
                a = registerFile.Registers[srcA.Output];

            if (srcB.Output > 0 & srcB.Output < 16)
                b = registerFile.Registers[srcB.Output];

            _busController.StartBusSignal(_busController.busSegments[3], a, true);
            _busController.StartBusSignal(_busController.busSegments[4], b, true);

            yield return new WaitUntil(() => _busController.NoActiveSignals);

            _busController.StartBusSignal(_busController.busSegments[0], srcA.Output, true);
            _busController.StartBusSignal(_busController.busSegments[1], srcB.Output, true);
            _busController.StartBusSignal(_busController.busSegments[2], a3.Output, true);

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
            _busController.StartBusSignal(_busController.busSegments[2], a3.Output);

            yield return new WaitUntil(() => _busController.NoActiveSignals);

            int a = 0;
            int b = 0;
            if (srcA.Output > 0 && srcA.Output < 16)
                a = registerFile.Registers[srcA.Output];
            
            if(srcB.Output > 0 & srcB.Output < 16)
                b = registerFile.Registers[srcB.Output];
            

            _busController.StartBusSignal(_busController.busSegments[3], a);
            _busController.StartBusSignal(_busController.busSegments[4], b);

            yield return new WaitUntil(() => _busController.NoActiveSignals);

            _busController.StartBusSignal(_busController.busSegments[5], ALU.calculate(a, b, _aluVizualizer.CurrentALUOperation));

            yield return new WaitUntil(() => _busController.NoActiveSignals);

            _busController.StartBusSignal(_busController.busSegments[6], wd3.Output);

            _currentBus++;
        }

        yield return new WaitUntil(() => _busController.NoActiveSignals);
    }

    protected override void UpdateVizualizers()
    {
        _infoSrcARegister.Display("Register A1", $"{srcA.Output}");
        _infoSrcBRegister.Display("Register A2", $"{srcB.Output}");
        _infoA3Register.Display("Register A3", $"{a3.Output}");
        _infoWD3Register.Display("Register WD3", $"{wd3.Output}");


        _registerFileVisualizer.UIRegisterPanel.Display(registerFile.Registers);


        // ==============================  WE SECTION  =====================================
        _registerSrcAVisualizer.ForceUpdateWriteEnableVisualization(srcA.WriteEnable);
        _registerSrcBVisualizer.ForceUpdateWriteEnableVisualization(srcB.WriteEnable);
        _registerA3Visualizer.ForceUpdateWriteEnableVisualization(a3.WriteEnable);
        _registerWD3Visualizer.ForceUpdateWriteEnableVisualization(wd3.WriteEnable);

        _registerFileVisualizer.ForceUpdateWriteEnableVisualization(registerFile.RegisterWriteEnable);
    }
}
