using System.Collections;
using UnityEngine;

public struct SubExtendedSevenLevelState
{
    public int RegisterSrcAValue;
    public int RegisterImmValue;
    public int RegisterA3Value;
    public int RegisterWD3Value;

    public int[] RegisterFieldValue;

    public bool RegisterSrcAWE;
    public bool RegisterImmWE;
    public bool RegisterA3WE;
    public bool RegisterWD3WE;

    public int ALUOperation;

    public int ExtenderOperation;

    public int MUXPath;
}

public class SubExtendedRegisseur : BaseLevelRegisseur
{
    [SerializeField] protected RegisterVizualizer _registerSrcAVisualizer;
    [SerializeField] protected RegisterVizualizer _registerImmediateVisualizer;
    [SerializeField] protected RegisterVizualizer _registerA3Visualizer;
    [SerializeField] protected RegisterVizualizer _registerWD3Visualizer;

    [SerializeField] protected ALUVizualiser _aluVizualizer;

    [SerializeField] protected RegisterFileVizualizer _registerFileVisualizer;
    [SerializeField] private ExternderVizualizer _extenderVizualizer;
    [SerializeField] private MuiltiplexerVizualizer _MUXVisualizer;

    #region CACHED UI REFERENCES
    protected InfoPanelUI _infoSrcARegister;
    protected InfoPanelUI _infoImmRegister;
    protected InfoPanelUI _infoA3Register;
    protected InfoPanelUI _infoWD3Register;
    #endregion

    // Intern components for computations
    protected Register srcA;
    protected Register immValue;
    protected Register a3;
    protected Register wd3;

    protected RegisterFile registerFile;

    protected int _currentBus = 0;

    protected override void OnLevelStart()
    {
        // addi x0, x4, 256
        srcA = new Register(4); srcA.WriteEnable = true;
        immValue = new Register(268566547); immValue.WriteEnable = true;
        a3 = new Register(0); a3.WriteEnable = true;
        wd3 = new Register(0); wd3.WriteEnable = true;

        registerFile = new RegisterFile(); registerFile.RegisterWriteEnable = true;
        registerFile.InitializeRegisters(new int[] { -98, 1, 39, 43, 0, 6, 8,
                                                     40, 3, 39, 13, 56, 64, 20,
                                                     50, 51, 0, 12, 53, 65, 29,
                                                     60, 61, 0, 25, 54, 0, 28,
                                                     70, 30, 31, 0});

        // Caching of UI panels for visualizers
        _infoSrcARegister = _registerSrcAVisualizer.UIRegisterPanel;
        _infoImmRegister = _registerImmediateVisualizer.UIRegisterPanel;
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
        SubExtendedSevenLevelState s = (SubExtendedSevenLevelState)state;

        srcA = new Register(s.RegisterSrcAValue);
        immValue = new Register(s.RegisterImmValue);
        a3 = new Register(s.RegisterA3Value);
        wd3 = new Register(s.RegisterWD3Value);

        registerFile.InitializeRegisters(s.RegisterFieldValue);

        srcA.WriteEnable = s.RegisterSrcAWE;
        immValue.WriteEnable = s.RegisterImmWE;
        a3.WriteEnable = s.RegisterA3WE;
        wd3.WriteEnable = s.RegisterWD3WE;

        _aluVizualizer.ChooseALUOperation(s.ALUOperation);
        _extenderVizualizer.ChooseALUOperation(s.ExtenderOperation);

        int currentPath = s.MUXPath;
        if (currentPath == -1)
        {
            _MUXVisualizer.ResetVizualization();
        }
        else if (currentPath == 0)
        {
            _MUXVisualizer.SelectPath(0);
        }
        else if (currentPath == 1)
        {
            _MUXVisualizer.SelectPath(1);
        }
        else if (currentPath == 2)
        {
            _MUXVisualizer.SelectPath(2);
        }
        else
        {
            Debug.LogError($"Saved multiplexer value {currentPath} is not in [0, 3]");
        }
    }

    protected override void BlinkClockedComponents()
    {
        _registerSrcAVisualizer.TriggerBlink();
        _registerImmediateVisualizer.TriggerBlink();
        _registerA3Visualizer.TriggerBlink();
        _registerWD3Visualizer.TriggerBlink();

        _registerFileVisualizer.TriggerBlink();
    }

    protected override void BlockIngameInteractables()
    {
        _registerSrcAVisualizer.UIRegisterPanel.WEButton.interactable = false;
        _registerImmediateVisualizer.UIRegisterPanel.WEButton.interactable = false;
        _registerA3Visualizer.UIRegisterPanel.WEButton.interactable = false;
        _registerWD3Visualizer.UIRegisterPanel.WEButton.interactable = false;

        _registerFileVisualizer.UIRegisterPanel.WEButton.interactable = false;

        _aluVizualizer.UIController.FirstOperationButton.interactable = false;
        _aluVizualizer.UIController.SecondOperationButton.interactable = false;
        _aluVizualizer.UIController.ThirdOperationButton.interactable = false;
        _aluVizualizer.UIController.FourthOperationButton.interactable = false;

        _extenderVizualizer.UIController.FirstOperationButton.interactable = false;
        _extenderVizualizer.UIController.SecondOperationButton.interactable = false;
        _extenderVizualizer.UIController.ThirdOperationButton.interactable = false;
        _extenderVizualizer.UIController.FourthOperationButton.interactable = false;

        _MUXVisualizer.UIController.FirstWayButton.interactable = false;
        _MUXVisualizer.UIController.SecondWayButton.interactable = false;
        _MUXVisualizer.UIController.ThirdWayButton.interactable = false;
    }

    protected override bool CheckWinCondition()
    {
        return registerFile.Registers[0] == 256;
    }

    protected override object GetCurrentState()
    {
        return new SubExtendedSevenLevelState
        {
            RegisterSrcAValue = srcA.Output,
            RegisterImmValue = immValue.Output,
            RegisterA3Value = a3.Output,
            RegisterWD3Value = wd3.Output,

            RegisterFieldValue = registerFile.Registers,

            RegisterSrcAWE = srcA.WriteEnable,
            RegisterImmWE = immValue.WriteEnable,
            RegisterA3WE = a3.WriteEnable,
            RegisterWD3WE = wd3.WriteEnable,

            ALUOperation = _aluVizualizer.CurrentALUOperation,

            ExtenderOperation = _extenderVizualizer.CurrentALUOperation,

            MUXPath = _MUXVisualizer.CurrentChoosenMuxPath,
        };
    }

    protected override void HandleClockUpdate()
    {
        // sinchronyse vizualisers and concrete objects
        srcA.WriteEnable = _registerSrcAVisualizer.isWriteEnabled;
        immValue.WriteEnable = _registerImmediateVisualizer.isWriteEnabled;
        a3.WriteEnable = _registerA3Visualizer.isWriteEnabled;
        wd3.WriteEnable = _registerWD3Visualizer.isWriteEnabled;
        
        registerFile.RegisterWriteEnable = _registerFileVisualizer.isWriteEnabled;

        // implementation
        registerFile.ReadAdress1 = srcA.Output;
        registerFile.ReadAdress2 = 0;

        registerFile.ReadRegisters();

        int a = 0;
        if (srcA.Output > 0 && srcA.Output < 16)
            a = registerFile.Registers[srcA.Output];
        int ext = Extender.Evaluate(_extenderVizualizer.CurrentALUOperation, (uint)immValue.Output);
        int muxVal = calculateMUX(0, ext, -1, _MUXVisualizer.CurrentChoosenMuxPath);

        wd3.Input = ALU.calculate(a, muxVal, _aluVizualizer.CurrentALUOperation);

        if (_tickCounter - 1 >= 0)
        {
            SubExtendedSevenLevelState legcyState = (SubExtendedSevenLevelState)_tickStateValues[_tickCounter - 1];

            registerFile.WriteAdress = legcyState.RegisterA3Value;
        }

        registerFile.WriteData = wd3.Output;



        srcA.PreClockUpdate();
        immValue.PreClockUpdate();
        a3.PreClockUpdate();
        wd3.PreClockUpdate();


        // Only if WriteEnable = true, call Clock
        srcA.Clock();
        immValue.Clock();
        a3.Clock();
        wd3.Clock();
        registerFile.Clock();
    }

    protected override void ReleaseIngameInteractables()
    {
        _registerSrcAVisualizer.UIRegisterPanel.WEButton.interactable = true;
        _registerImmediateVisualizer.UIRegisterPanel.WEButton.interactable = true;
        _registerA3Visualizer.UIRegisterPanel.WEButton.interactable = true;
        _registerWD3Visualizer.UIRegisterPanel.WEButton.interactable = true;

        _registerFileVisualizer.UIRegisterPanel.WEButton.interactable = true;

        _aluVizualizer.UIController.FirstOperationButton.interactable = true;
        _aluVizualizer.UIController.SecondOperationButton.interactable = true;
        _aluVizualizer.UIController.ThirdOperationButton.interactable = true;
        _aluVizualizer.UIController.FourthOperationButton.interactable = true;

        _extenderVizualizer.UIController.FirstOperationButton.interactable = true;
        _extenderVizualizer.UIController.SecondOperationButton.interactable = true;
        _extenderVizualizer.UIController.ThirdOperationButton.interactable = true;
        _extenderVizualizer.UIController.FourthOperationButton.interactable = true;

        _MUXVisualizer.UIController.FirstWayButton.interactable = true;
        _MUXVisualizer.UIController.SecondWayButton.interactable = true;
        _MUXVisualizer.UIController.ThirdWayButton.interactable = true;
    }

    protected override IEnumerator ReverseBusVisualizations()
    {
        if (_currentBus >= 1 && _currentBus <= _maxTickNumber)
        {
            _busController.StartBusSignal(_busController.busSegments[8], wd3.Input, true);
            yield return new WaitUntil(() => _busController.NoActiveSignals);

            _busController.StartBusSignal(_busController.busSegments[7], wd3.Input, true);
            yield return new WaitUntil(() => _busController.NoActiveSignals);

            int ext = Extender.Evaluate(_extenderVizualizer.CurrentALUOperation, (uint)immValue.Output);
            int mux = calculateMUX(0, ext, -1, _MUXVisualizer.CurrentChoosenMuxPath);

            _busController.StartBusSignal(_busController.busSegments[3], registerFile.Registers[srcA.Output], true);
            _busController.StartBusSignal(_busController.busSegments[6], mux, true);
            yield return new WaitUntil(() => _busController.NoActiveSignals);

            _busController.StartBusSignal(_busController.busSegments[4], 0, true);
            _busController.StartBusSignal(_busController.busSegments[5], ext, true);
            yield return new WaitUntil(() => _busController.NoActiveSignals);

            _busController.StartBusSignal(_busController.busSegments[0], srcA.Output);
            _busController.StartBusSignal(_busController.busSegments[1], a3.Output);
            _busController.StartBusSignal(_busController.busSegments[2], immValue.Output);

            _currentBus--;
        }

        yield return new WaitUntil(() => _busController.NoActiveSignals);
    }

    protected override IEnumerator RunBusVisualizations()
    {
        if (_currentBus >= 0 && _currentBus < _maxTickNumber)
        {
            _busController.StartBusSignal(_busController.busSegments[0], srcA.Output);
            _busController.StartBusSignal(_busController.busSegments[1], a3.Output);
            _busController.StartBusSignal(_busController.busSegments[2], immValue.Output);
            yield return new WaitUntil(() => _busController.NoActiveSignals);

            int a = 0;
            if (srcA.Output > 0 && srcA.Output < 16)
                a = registerFile.Registers[srcA.Output];

            int ext = Extender.Evaluate(_extenderVizualizer.CurrentALUOperation, (uint)immValue.Output);
            int mux = calculateMUX(0, ext, -1, _MUXVisualizer.CurrentChoosenMuxPath);

            _busController.StartBusSignal(_busController.busSegments[4], 0);
            _busController.StartBusSignal(_busController.busSegments[5], ext);
            yield return new WaitUntil(() => _busController.NoActiveSignals);

            _busController.StartBusSignal(_busController.busSegments[3], a);
            _busController.StartBusSignal(_busController.busSegments[6], mux);
            yield return new WaitUntil(() => _busController.NoActiveSignals);

            _busController.StartBusSignal(_busController.busSegments[7], ALU.calculate(a, mux, _aluVizualizer.CurrentALUOperation));

            yield return new WaitUntil(() => _busController.NoActiveSignals);

            _busController.StartBusSignal(_busController.busSegments[8], wd3.Output);

            _currentBus++;
        }

        yield return new WaitUntil(() => _busController.NoActiveSignals);
    }

    protected override void UpdateVizualizers()
    {
        _infoSrcARegister.Display("Register A1", $"{srcA.Output}");
        _infoImmRegister.Display("Register A2", commandBuilder((uint)immValue.Output));
        _infoA3Register.Display("Register A3", $"{a3.Output}");
        _infoWD3Register.Display("Register WD3", $"{wd3.Output}");

        _registerFileVisualizer.UIRegisterPanel.Display(registerFile.Registers);


        // ==============================  WE SECTION  =====================================
        _registerSrcAVisualizer.ForceUpdateWriteEnableVisualization(srcA.WriteEnable);
        _registerImmediateVisualizer.ForceUpdateWriteEnableVisualization(immValue.WriteEnable);
        _registerA3Visualizer.ForceUpdateWriteEnableVisualization(a3.WriteEnable);
        _registerWD3Visualizer.ForceUpdateWriteEnableVisualization(wd3.WriteEnable);

        _registerFileVisualizer.ForceUpdateWriteEnableVisualization(registerFile.RegisterWriteEnable);
    }

    #region helpers
    private int calculateMUX(int muxCurrentPath, int first, int second, int third)
    {
        int result = 0;
        if (muxCurrentPath == 0)
        {
            result = first;
        }
        else if (muxCurrentPath == 1)
        {
            result = second;
        }
        else if (muxCurrentPath == 2)
        {
            result = third;
        }
        /*else
        {
            Debug.LogError($"Unexpected MUX path {muxCurrentPath}. Expected value: [0, 3]");
        }*/
        return result;
    }
    #endregion
}
