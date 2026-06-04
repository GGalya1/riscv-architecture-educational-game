using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

public struct ProcessorLevelState
{
    public int RegisterPCValue;
    public int RegisterOldPCValue;
    public int RegisterInstrValue;
    public int RegisterDataValue;
    public int RegisterScrAValue;
    public int RegisterSrcBValue;
    public int RegisterAluOutValue;

    public int FirstMemoryValue;
    public int SecondMemoryValue;
    public int ThirdMemoryValue;
    public int FourthMemoryValue;

    public int[] RegisterFieldValue;

    public bool RegisterPCwe;
    public bool RegisterOldPCwe;
    public bool RegisterInstrWe;
    public bool RegisterDataWe;
    public bool RegisterScrAwe;
    public bool RegisterSrcBwe;
    public bool RegisterAluOutWe;

    public int AluOperation;

    public int ExtenderOperation;

    public int MuXadrPath;
    public int MuXsrcAPath;
    public int MuXsrcBPath;
    public int MuXresultPath;
}

public enum ExerciseTyp {
    REGISTER_FIELD = 0,
    MEMORY = 1,
    BEQ = 2,
    JAL = 3,
}

public class FullProcessorRegisseur : BaseLevelRegisseur<ProcessorLevelState>
{
    [FormerlySerializedAs("_registerPCVisualizer")]
    [Header("Precossor Specific Components")]
    [SerializeField] private RegisterVisualizer registerPCVisualizer;
    [FormerlySerializedAs("_registerOldPCVisualizer")] [SerializeField] private RegisterVisualizer registerOldPCVisualizer;
    [FormerlySerializedAs("_registerIntructionVisualizer")] [SerializeField] private RegisterVisualizer registerIntructionVisualizer;
    [FormerlySerializedAs("_registerDataVisualizer")] [SerializeField] private RegisterVisualizer registerDataVisualizer;
    [FormerlySerializedAs("_registerSrcAVisualizer")] [SerializeField] private RegisterVisualizer registerSrcAVisualizer;
    [FormerlySerializedAs("_registerSrcBVisualizer")] [SerializeField] private RegisterVisualizer registerSrcBVisualizer;
    [FormerlySerializedAs("_registerALUOutVisualizer")] [SerializeField] private RegisterVisualizer registerAluOutVisualizer;

    [FormerlySerializedAs("_adrMUXVisualizer")] [SerializeField] private MultiplexerVisualizer adrMuxVisualizer;
    [FormerlySerializedAs("_srcAMUXVisualizer")] [SerializeField] private MultiplexerVisualizer srcAmuxVisualizer;
    [FormerlySerializedAs("_srcBMUXVisualizer")] [SerializeField] private MultiplexerVisualizer srcBmuxVisualizer;
    [FormerlySerializedAs("_resultMUXVisualizer")] [SerializeField] private MultiplexerVisualizer resultMuxVisualizer;

    [FormerlySerializedAs("_aluVizualizer")] [SerializeField] private AluVisualiser aluVizualizer;
    [FormerlySerializedAs("_extenderVizualizer")] [SerializeField] private ExtenderVisualizer extenderVizualizer;

    [FormerlySerializedAs("_memoryVisualizer")] [SerializeField] private InstructionDataMemoryVisualizer memoryVisualizer;
    [FormerlySerializedAs("_registerFileVisualizer")] [SerializeField] private RegisterFileVisualizer registerFileVisualizer;

    [FormerlySerializedAs("_numberBlinker")] [SerializeField] private Blinker numberBlinker;

    [Header("Initial values for level")]
    // [SerializeField] private ProcessorInitialState _initial;
    public static ProcessorInitialState Initial;

    [FormerlySerializedAs("_sidePanelInformer")]
    [Header("Extra Panel Fields")]
    [SerializeField] private SidePanelStateInformer sidePanelInformer;

    #region CACHED UI REFERENCES
    private InfoPanelUI _infoPCRegister;
    private InfoPanelUI _infoOldPCRegister;
    private InfoPanelUI _infoIntructionRegister;
    private InfoPanelUI _infoDataRegister;
    private InfoPanelUI _infoSrcARegister;
    private InfoPanelUI _infoSrcBRegister;
    private InfoPanelUI _infoAluOutRegister;

    private InstrMemoryControlPanel _infoDataMemory;
    #endregion

    // Intern components for computations
    private Register _pc;
    private Register _oldPC;
    private Register _instructionReg;
    private Register _dataReg;
    private Register _srcA;
    private Register _srcB;
    private Register _aluOutReg;

    private DataInstMemory _dataIntructionMemory;
    private RegisterFile _registerFile;


    private int _currentBus; // [0, 10]

    protected void Awake()
    {
        levelManager.SetLevelDialogue(Initial.customDialogueGraph);
    }
    protected override void OnLevelStart()
    {
        _pc = new Register(Initial.pcRegisterInitialValue)
        {
            WriteEnable = true
        };
        _oldPC = new Register()
        {
            WriteEnable = true
        };
        _instructionReg = new Register()
        {
            WriteEnable = true
        };
        _dataReg = new Register()
        {
            WriteEnable = true
        };
        _srcA = new Register()
        {
            WriteEnable = true
        };
        _srcB = new Register()
        {
            WriteEnable = true
        };
        _aluOutReg = new Register()
        {
            WriteEnable = true
        };

        _registerFile = new RegisterFile
        {
            RegisterWriteEnable = true
        };
        _registerFile.InitializeRegisters(new [] { 0, 1, 39, 43, 5, 6, 8,
                                                     40, 3, 39, 13, 56, 63, 20,
                                                     50, 51, 0, 12, 53, 65, 29,
                                                     60, 61, 0, 25, 54, 0, 28,
                                                     70, 30, 31, 0});

        _dataIntructionMemory = new DataInstMemory
        {
            MemoryWrite = true
        };

        _dataIntructionMemory.LoadWord(0, Initial.firstMemoWord);
        _dataIntructionMemory.LoadWord(4, Initial.secondMemoWord);
        _dataIntructionMemory.LoadWord(8, Initial.thirdMemoWord);
        _dataIntructionMemory.LoadWord(12, Initial.fourthMemoWord);

        // Caching of UI panels for visualizers
        _infoPCRegister = registerPCVisualizer.UIRegisterPanel;
        _infoOldPCRegister = registerOldPCVisualizer.UIRegisterPanel;
        _infoIntructionRegister = registerIntructionVisualizer.UIRegisterPanel;
        _infoDataRegister = registerDataVisualizer.UIRegisterPanel;
        _infoSrcARegister = registerSrcAVisualizer.UIRegisterPanel;
        _infoSrcBRegister = registerSrcBVisualizer.UIRegisterPanel;
        _infoAluOutRegister = registerAluOutVisualizer.UIRegisterPanel;

        UpdateVisualizers();
    }

    protected override void ApplyState(ProcessorLevelState s)
    {
        _pc = new Register(s.RegisterPCValue);
        _oldPC = new Register(s.RegisterOldPCValue);
        _instructionReg = new Register(s.RegisterInstrValue);
        _dataReg = new Register(s.RegisterDataValue);
        _srcA = new Register(s.RegisterScrAValue);
        _srcB = new Register(s.RegisterSrcBValue);
        _aluOutReg = new Register(s.RegisterAluOutValue);
        
        ApplyMuxState(s.MuXadrPath, adrMuxVisualizer);
        ApplyMuxState(s.MuXsrcAPath, srcAmuxVisualizer);
        ApplyMuxState(s.MuXsrcBPath, srcBmuxVisualizer);
        ApplyMuxState(s.MuXresultPath, resultMuxVisualizer);

        _dataIntructionMemory = new DataInstMemory
        {
            Memory =
            {
                [0] = s.FirstMemoryValue,
                [4] = s.SecondMemoryValue,
                [8] = s.ThirdMemoryValue,
                [12] = s.FourthMemoryValue
            }
        };

        // noch Register File einfuegen
        _registerFile.InitializeRegisters(s.RegisterFieldValue);

        _pc.WriteEnable = s.RegisterPCwe;
        _oldPC.WriteEnable = s.RegisterOldPCwe;
        _instructionReg.WriteEnable = s.RegisterInstrWe;
        _dataReg.WriteEnable = s.RegisterDataWe;
        _srcA.WriteEnable = s.RegisterScrAwe;
        _srcB.WriteEnable = s.RegisterSrcBwe;
        _aluOutReg.WriteEnable = s.RegisterAluOutWe;

        aluVizualizer.ChooseAluOperation(s.AluOperation);
        extenderVizualizer.ChooseAluOperation(s.ExtenderOperation);
    }

    protected override void BlinkClockedComponents()
    {
        registerPCVisualizer.TriggerBlink();
        registerOldPCVisualizer.TriggerBlink();
        registerIntructionVisualizer.TriggerBlink();
        registerDataVisualizer.TriggerBlink();
        registerSrcAVisualizer.TriggerBlink();
        registerSrcBVisualizer.TriggerBlink();
        registerAluOutVisualizer.TriggerBlink();

        memoryVisualizer.TriggerBlink();
        registerFileVisualizer.TriggerBlink();

        numberBlinker.Trigger();
    }

    protected override void BlockInGameInteractable()
    {
        memoryVisualizer.UIRegisterPanel.WeButton.interactable = false;
        registerFileVisualizer.UIRegisterPanel.WeButton.interactable = false;

        SwitchInteractablesAccessability(false);

        SwitchMuxInteractables(false, adrMuxVisualizer);
        SwitchMuxInteractables(false, srcAmuxVisualizer);
        SwitchMuxInteractables(false, srcBmuxVisualizer);
        SwitchMuxInteractables(false, resultMuxVisualizer);

        aluVizualizer.uiController.FirstOperationButton.interactable = false;
        aluVizualizer.uiController.SecondOperationButton.interactable = false;
        aluVizualizer.uiController.ThirdOperationButton.interactable = false;
        aluVizualizer.uiController.FourthOperationButton.interactable = false;

        extenderVizualizer.uiController.FirstOperationButton.interactable = false;
        extenderVizualizer.uiController.SecondOperationButton.interactable = false;
        extenderVizualizer.uiController.ThirdOperationButton.interactable = false;
        extenderVizualizer.uiController.FourthOperationButton.interactable = false;
    }
    private void SwitchInteractablesAccessability(bool trigger) {
        registerPCVisualizer.UIRegisterPanel.WeButton.interactable = trigger;
        registerOldPCVisualizer.UIRegisterPanel.WeButton.interactable = trigger;
        registerIntructionVisualizer.UIRegisterPanel.WeButton.interactable = trigger;
        registerDataVisualizer.UIRegisterPanel.WeButton.interactable = trigger;
        registerSrcAVisualizer.UIRegisterPanel.WeButton.interactable = trigger;
        registerSrcBVisualizer.UIRegisterPanel.WeButton.interactable = trigger;
        registerAluOutVisualizer.UIRegisterPanel.WeButton.interactable = trigger;
    }
    private static void SwitchMuxInteractables(bool trigger, MultiplexerVisualizer target)
    {
        target.UIController.FirstWayButton.interactable = trigger;
        target.UIController.SecondWayButton.interactable = trigger;
        target.UIController.ThirdWayButton.interactable = trigger;
    }

    protected override bool CheckWinCondition()
    {
        if (Initial.aufgabeTyp == ExerciseTyp.REGISTER_FIELD)
        {
            return _registerFile.Registers[Initial.registerFieldAddressAnswer] == Initial.registerFieldValueAnswer;
        }
        else if (Initial.aufgabeTyp == ExerciseTyp.MEMORY)
        {
            return _dataIntructionMemory.Memory[Initial.memoryAddressAnswer] == Initial.memoryValueAnswer;
        }
        else if (Initial.aufgabeTyp == ExerciseTyp.BEQ)
        {
            return _pc.Output == Initial.pcValueAnswer;
        }
        else if (Initial.aufgabeTyp == ExerciseTyp.JAL) {
            return _pc.Output == Initial.pcValueAnswer && _registerFile.Registers[Initial.registerFieldAddressAnswer] == Initial.registerFieldValueAnswer;
        }
        else {
            return false;
        }
    }

    protected override ProcessorLevelState GetCurrentState()
    {
        return new ProcessorLevelState
        {
            RegisterPCValue = _pc.Output,
            RegisterOldPCValue = _oldPC.Output,
            RegisterInstrValue = _instructionReg.Output,
            RegisterDataValue = _dataReg.Output,
            RegisterScrAValue = _srcA.Output,
            RegisterSrcBValue = _srcB.Output,
            RegisterAluOutValue = _aluOutReg.Output,

            RegisterFieldValue = (int[])_registerFile.Registers.Clone(),

            FirstMemoryValue = _dataIntructionMemory.Memory[0],
            SecondMemoryValue = _dataIntructionMemory.Memory[4],
            ThirdMemoryValue = _dataIntructionMemory.Memory[8],
            FourthMemoryValue = _dataIntructionMemory.Memory[12],

            RegisterPCwe = _pc.WriteEnable,
            RegisterOldPCwe = _oldPC.WriteEnable,
            RegisterInstrWe = _instructionReg.WriteEnable,
            RegisterDataWe = _dataReg.WriteEnable,
            RegisterScrAwe = _srcA.WriteEnable,
            RegisterSrcBwe = _srcB.WriteEnable,
            RegisterAluOutWe = _aluOutReg.WriteEnable,

            AluOperation = aluVizualizer.CurrentAluOperation,

            ExtenderOperation = extenderVizualizer.CurrentAluOperation,

            MuXadrPath = adrMuxVisualizer.CurrentChosenMuxPath,
            MuXsrcAPath = srcAmuxVisualizer.CurrentChosenMuxPath,
            MuXsrcBPath = srcBmuxVisualizer.CurrentChosenMuxPath,
            MuXresultPath = resultMuxVisualizer.CurrentChosenMuxPath,
        };
    }

    protected override void HandleClockUpdate()
    {
        // synchronize visualizers and concrete objects
        _pc.WriteEnable = registerPCVisualizer.isWriteEnabled;
        _oldPC.WriteEnable = registerOldPCVisualizer.isWriteEnabled;
        _instructionReg.WriteEnable = registerIntructionVisualizer.isWriteEnabled;
        _dataReg.WriteEnable = registerDataVisualizer.isWriteEnabled;
        _srcA.WriteEnable = registerSrcAVisualizer.isWriteEnabled;
        _srcB.WriteEnable = registerSrcBVisualizer.isWriteEnabled;
        _aluOutReg.WriteEnable = registerAluOutVisualizer.isWriteEnabled;
        _dataIntructionMemory.MemoryWrite = memoryVisualizer.isWriteEnabled;
        _registerFile.RegisterWriteEnable = registerFileVisualizer.isWriteEnabled;



        // implementation

        #region first step (memory)
        var tmpAddress = CalculateAdressMux();
        if (_dataIntructionMemory.Memory.ContainsKey(tmpAddress))
        {
            _instructionReg.Input = _dataIntructionMemory.Memory[tmpAddress];
            _dataReg.Input = _dataIntructionMemory.Memory[tmpAddress];
        }
        else
        {
            _instructionReg.Input = 0;
            _dataReg.Input = 0;
        }

        _oldPC.Input = _pc.Output;

        _dataIntructionMemory.Address = tmpAddress;
        #endregion

        #region second step (register file)

        // A1: [19:15] (Register Source 1)
        _registerFile.ReadAdress1 = (_instructionReg.Output >> 15) & 0x1F;

        // A2: [24:20] (Register Source 2)
        _registerFile.ReadAdress2 = (_instructionReg.Output >> 20) & 0x1F;

        _registerFile.ReadRegisters();

        //Debug.LogWarning($"This is tick {_tickCounter}. A1: {registerFile.ReadAdress1}, A2: {registerFile.ReadAdress2}, A3: {(temp >> 7) & 0x1F}. Command: {commandBuilder(temp)}");

        // A3: [11:7] (Register Destination / rd)

        _srcA.Input = _registerFile.ReadData1;
        _srcB.Input = _registerFile.ReadData2;
        #endregion

        #region third step (ALU)
        _aluOutReg.Input = CalculateAlu();
        _dataIntructionMemory.WriteData = _srcB.Output;
        #endregion

        #region fourth step (WB)
        
        if (TickCounter - 2 >= 0) {
            _registerFile.WriteAdress = ((TickStateValues[TickCounter - 2].RegisterInstrValue >> 7) & 0x1F);
            _registerFile.WriteData = TickStateValues[TickCounter - 2].RegisterInstrValue;
        }

        var tmpResult = CalculateResultMux();
        _registerFile.WriteData = tmpResult;
        _pc.Input = tmpResult;
        #endregion

        _pc.PreClockUpdate();
        _oldPC.PreClockUpdate();
        _instructionReg.PreClockUpdate();
        _dataReg.PreClockUpdate();
        _srcA.PreClockUpdate();
        _srcB.PreClockUpdate();
        _aluOutReg.PreClockUpdate();
        _dataIntructionMemory.PreClockUpdate();

        _pc.Clock();
        _oldPC.Clock();
        _instructionReg.Clock();
        _dataReg.Clock();
        _srcA.Clock();
        _srcB.Clock();
        _aluOutReg.Clock();
        _dataIntructionMemory.Clock();
        _registerFile.Clock();
    }
    protected override void ReleaseInGameInteractable()
    {
        memoryVisualizer.UIRegisterPanel.WeButton.interactable = true;
        registerFileVisualizer.UIRegisterPanel.WeButton.interactable = true;

        SwitchInteractablesAccessability(true);

        SwitchMuxInteractables(true, adrMuxVisualizer);
        SwitchMuxInteractables(true, srcAmuxVisualizer);
        SwitchMuxInteractables(true, srcBmuxVisualizer);
        SwitchMuxInteractables(true, resultMuxVisualizer);

        aluVizualizer.uiController.FirstOperationButton.interactable = true;
        aluVizualizer.uiController.SecondOperationButton.interactable = true;
        aluVizualizer.uiController.ThirdOperationButton.interactable = true;
        aluVizualizer.uiController.FourthOperationButton.interactable = true;

        extenderVizualizer.uiController.FirstOperationButton.interactable = true;
        extenderVizualizer.uiController.SecondOperationButton.interactable = true;
        extenderVizualizer.uiController.ThirdOperationButton.interactable = true;
        extenderVizualizer.uiController.FourthOperationButton.interactable = true;
    }

    protected override IEnumerator RunBusVisualizations()
    {
        if (_currentBus >= 0 && _currentBus < maxTickNumber)
        {
            switch (_currentBus % 4)
            {
                case 0: yield return RunFetchVisualisation(); break;
                case 1: yield return RunDecodeVisualisation(); break;
                case 2: yield return RunExecutionVizualization(); break;
                case 3: yield return RunWriteBackVizualization(); break;
            }

            _currentBus++;
        }

        yield return new WaitUntil(() => busController.NoActiveSignals);
    }

    #region vizualization helpers
    private IEnumerator RunFetchVisualisation() {
        busController.StartBusSignal(busController.busSegments[10], _pc.Output);
        busController.StartBusSignal(busController.busSegments[19], _pc.Output);
        busController.StartBusSignal(busController.busSegments[20], _pc.Output);

        var muxSrcA = CalculateSrcAMux();
        var muxSrcB = CalculateSrcBMux();
        var output = CalculateResultMux();
        var addressValue = CalculateAdressMux();

        yield return StartCoroutine(DelayedSignal(busController.busSegments[11], addressValue));

        // ob Value existiert
        if (_dataIntructionMemory.Memory.TryGetValue(addressValue, out var value)) {
            yield return StartCoroutine(DelayedSignal(busController.busSegments[12], value));
        }
        else
        {
            yield return StartCoroutine(DelayedSignal(busController.busSegments[12], 0));
        }


        yield return StartCoroutine(DelayedSignal(busController.busSegments[9], 4));
        
        yield return StartCoroutine(DelayedSignals(busController.busSegments[16], muxSrcA, busController.busSegments[15], muxSrcB));

        yield return StartCoroutine(DelayedSignal(busController.busSegments[6], CalculateAlu()));

        
        yield return StartCoroutine(DelayedSignal(busController.busSegments[23], output));

        yield return new WaitUntil(() => busController.NoActiveSignals);
    }
    private IEnumerator RunDecodeVisualisation()
    {
        busController.StartBusSignal(busController.busSegments[0], _instructionReg.Output);
        busController.StartBusSignal(busController.busSegments[1], _instructionReg.Output);
        // _busController.StartBusSignal(_busController.busSegments[2], instructionReg.Output);
        busController.StartBusSignal(busController.busSegments[3], _instructionReg.Output);

        var srcAValue = 0;
        var srcBValue = 0;
        if (_dataIntructionMemory.Memory.TryGetValue(_instructionReg.Output, out var value))
        {
            srcAValue = value;
        }
        if (_dataIntructionMemory.Memory.TryGetValue(_instructionReg.Output, out var value1)) {
            srcBValue = value1;
        }
        yield return StartCoroutine(DelayedSignals(busController.busSegments[13], srcAValue, busController.busSegments[14], srcBValue));

        yield return new WaitUntil(() => busController.NoActiveSignals);
    }
    private IEnumerator RunExecutionVizualization() { // das noch korrigieren
        busController.StartBusSignal(busController.busSegments[16], _srcA.Output);
        busController.StartBusSignal(busController.busSegments[8], Extender.Evaluate(extenderVizualizer.CurrentAluOperation, (uint)_instructionReg.Output));

        yield return new WaitUntil(() => busController.NoActiveSignals);

        busController.StartBusSignal(busController.busSegments[22], CalculateSrcAMux());
        busController.StartBusSignal(busController.busSegments[7], CalculateSrcBMux());

        yield return new WaitUntil(() => busController.NoActiveSignals);
        busController.StartBusSignal(busController.busSegments[17], CalculateAlu());

        yield return new WaitUntil(() => busController.NoActiveSignals);
    }
    private IEnumerator RunWriteBackVizualization() { // das noch korrigieren
        busController.StartBusSignal(busController.busSegments[18], _aluOutReg.Output);
        busController.StartBusSignal(busController.busSegments[5], _aluOutReg.Output);

        yield return new WaitUntil(() => busController.NoActiveSignals);

        var res = CalculateResultMux();
        busController.StartBusSignal(busController.busSegments[23], res);
        busController.StartBusSignal(busController.busSegments[24], res);
        busController.StartBusSignal(busController.busSegments[25], res);

        yield return new WaitUntil(() => busController.NoActiveSignals);
    }
    
    private int CalculateSrcAMux() { 
        return EvaluateMux(srcAmuxVisualizer.CurrentChosenMuxPath, _pc.Output, _oldPC.Output, _srcA.Output);
    }
    private int CalculateSrcBMux()
    {
        return EvaluateMux(srcBmuxVisualizer.CurrentChosenMuxPath, _srcB.Output, Extender.Evaluate(extenderVizualizer.CurrentAluOperation, (uint)_instructionReg.Output), 4);
    }
    private int CalculateResultMux() { 
        return EvaluateMux(resultMuxVisualizer.CurrentChosenMuxPath, _aluOutReg.Output, _dataReg.Output, CalculateAlu());
    }
    private int CalculateAdressMux() { 
        return EvaluateMux(adrMuxVisualizer.CurrentChosenMuxPath, _pc.Output, CalculateResultMux(), 0);
    }
    
    private int CalculateAlu() {
        var muxSrcA = EvaluateMux(srcAmuxVisualizer.CurrentChosenMuxPath, _pc.Output, _oldPC.Output, _srcA.Output);

        var extenderTmp = 0;
        if (TickCounter > 0) {
            extenderTmp =  TickStateValues[TickCounter - 1].RegisterInstrValue;
        }
        var muxSrcB = EvaluateMux(srcBmuxVisualizer.CurrentChosenMuxPath, 
            _srcB.Output, 
            Extender.Evaluate(extenderVizualizer.CurrentAluOperation, (uint)extenderTmp), 
            4);
        return Alu.Calculate(muxSrcA, muxSrcB, aluVizualizer.CurrentAluOperation);
    }

    private IEnumerator ReverseFetchVisualisation() { // noch zu korrigieren
        yield return StartCoroutine(DelayedSignal(busController.busSegments[23], _pc.Input, true));

        yield return StartCoroutine(DelayedSignal(busController.busSegments[6], CalculateAlu(), true));

        yield return StartCoroutine(DelayedSignals(busController.busSegments[16], _srcA.Output, busController.busSegments[15], _srcB.Output, true, true));

        yield return StartCoroutine(DelayedSignal(busController.busSegments[12], _instructionReg.Input, true));

        yield return StartCoroutine(DelayedSignal(busController.busSegments[9], 4, true));

        yield return StartCoroutine(DelayedSignal(busController.busSegments[11], _dataIntructionMemory.Address, true));

        busController.StartBusSignal(busController.busSegments[10], _oldPC.Input, true);
        busController.StartBusSignal(busController.busSegments[19], _oldPC.Input, true);
        busController.StartBusSignal(busController.busSegments[20], _oldPC.Input, true);

        yield return new WaitUntil(() => busController.NoActiveSignals);
    }
    private IEnumerator ReverseDecodeVisualisation()
    {
        yield return StartCoroutine(DelayedSignals(busController.busSegments[13], _srcA.Input, busController.busSegments[14], _srcB.Input, true, true));

        busController.StartBusSignal(busController.busSegments[0], _instructionReg.Output, true);
        busController.StartBusSignal(busController.busSegments[1], _instructionReg.Output, true);
        // _busController.StartBusSignal(_busController.busSegments[2], instructionReg.Output);
        busController.StartBusSignal(busController.busSegments[3], _instructionReg.Output, true);
        
        yield return new WaitUntil(() => busController.NoActiveSignals);
    } 
    private IEnumerator ReverseExecutionVisualisation() // das noch korrigieren
    {
        busController.StartBusSignal(busController.busSegments[17], CalculateAlu(), true);

        yield return new WaitUntil(() => busController.NoActiveSignals);

        busController.StartBusSignal(busController.busSegments[16], _srcA.Output, true);
        busController.StartBusSignal(busController.busSegments[8], Extender.Evaluate(extenderVizualizer.CurrentAluOperation, (uint)_instructionReg.Output), true);

        yield return new WaitUntil(() => busController.NoActiveSignals);

        busController.StartBusSignal(busController.busSegments[22], CalculateSrcAMux(), true);
        busController.StartBusSignal(busController.busSegments[7], CalculateSrcBMux(), true);

        yield return new WaitUntil(() => busController.NoActiveSignals);
    }
    private IEnumerator ReverseWriteBackVisualisation() // das noch korrigieren
    {
        var res = CalculateResultMux();
        busController.StartBusSignal(busController.busSegments[23], res, true);
        busController.StartBusSignal(busController.busSegments[24], res, true);
        busController.StartBusSignal(busController.busSegments[25], res, true);

        yield return new WaitUntil(() => busController.NoActiveSignals);

        busController.StartBusSignal(busController.busSegments[18], _aluOutReg.Output, true);
        busController.StartBusSignal(busController.busSegments[5], _aluOutReg.Output, true);

        yield return new WaitUntil(() => busController.NoActiveSignals);
    }
    #endregion

    protected override IEnumerator ReverseBusVisualizations()
    {
        if (_currentBus >= 1 && _currentBus <= maxTickNumber)
        {
            switch ((_currentBus - 1) % 4)
            {
                case 0: yield return ReverseFetchVisualisation(); break;
                case 1: yield return ReverseDecodeVisualisation(); break;
                case 2: yield return ReverseExecutionVisualisation(); break;
                case 3: yield return ReverseWriteBackVisualisation(); break;
            }

            _currentBus--;
        }

        yield return new WaitUntil(() => busController.NoActiveSignals);
    }

    protected override void UpdateVisualizers()
    {
        UpdateSidePanel();

        _infoPCRegister.Display("PC Register", $"{_pc.Output}");
        _infoOldPCRegister.Display("Old PC Register", $"{_oldPC.Output}");
        _infoIntructionRegister.Display("Instruction Register", RiscVDecoder.CommandBuilder((uint)_instructionReg.Output));
        _infoDataRegister.Display("Data Register", $"{_dataReg.Output}");
        _infoSrcARegister.Display("SrcA Register", $"{_srcA.Output}");
        _infoSrcBRegister.Display("SrcB Register", $"{_srcB.Output}");
        _infoAluOutRegister.Display("ALU Out Register", $"{_aluOutReg.Output}");

        memoryVisualizer.UIRegisterPanel.Display(
            RiscVDecoder.CommandBuilder((uint)_dataIntructionMemory.Memory[0]),
            RiscVDecoder.CommandBuilder((uint)_dataIntructionMemory.Memory[4]),
            RiscVDecoder.CommandBuilder((uint)_dataIntructionMemory.Memory[8]),
            RiscVDecoder.CommandBuilder((uint)_dataIntructionMemory.Memory[12])
        );
        registerFileVisualizer.UIRegisterPanel.Display(_registerFile.Registers);


        // ==============================  WE SECTION  =====================================
        registerPCVisualizer.ForceUpdateWriteEnableVisualization(_pc.WriteEnable);
        registerOldPCVisualizer.ForceUpdateWriteEnableVisualization(_oldPC.WriteEnable);
        registerIntructionVisualizer.ForceUpdateWriteEnableVisualization(_instructionReg.WriteEnable);
        registerDataVisualizer.ForceUpdateWriteEnableVisualization(_dataReg.WriteEnable);
        registerSrcAVisualizer.ForceUpdateWriteEnableVisualization(_srcA.WriteEnable);
        registerSrcBVisualizer.ForceUpdateWriteEnableVisualization(_srcB.WriteEnable);
        registerAluOutVisualizer.ForceUpdateWriteEnableVisualization(_aluOutReg.WriteEnable);

        registerFileVisualizer.ForceUpdateWriteEnableVisualization(_registerFile.RegisterWriteEnable);
        memoryVisualizer.ForceUpdateWriteEnableVisualization(_dataIntructionMemory.MemoryWrite);
    }


    private void UpdateSidePanel() {
        var containsKey = _pc.Output % 4 == 0 
                          && _pc.Output is <= 12 and >= 0
                          && _dataIntructionMemory.Memory.ContainsKey(_pc.Output)
                          && _dataIntructionMemory.Memory[_pc.Output] > 1000000;
        if (containsKey)
        {
            sidePanelInformer.SetStateInfo((int)StateName.FETCH);
        }
        else if (_instructionReg.Output > 1000000)
        {
            sidePanelInformer.SetStateInfo((int)StateName.DECODE);
        }
        else if (TickCounter - 4 >= 0)
        {
            if (TickStateValues[TickCounter - 3].RegisterInstrValue < 1000000) return;
            var opcode = TickStateValues[TickCounter - 3].RegisterInstrValue & 0x7F;

            if (opcode == 0x03)
            {
                sidePanelInformer.SetStateInfo((int)StateName.MEM_WB);
            }
        }
        else if (TickCounter - 3 >= 0)
        {
            var s = TickStateValues[TickCounter - 2];

            if (s.RegisterInstrValue < 1000000) return;
            var opcode = s.RegisterInstrValue & 0x7F;

            switch (opcode)
            {
                case 0x33:
                case 0x13:
                    sidePanelInformer.SetStateInfo((int)StateName.ALU_WB);
                    break;
                case 0x03:
                    sidePanelInformer.SetStateInfo((int)StateName.MEM_READ);
                    break;
                case 0x23:
                    sidePanelInformer.SetStateInfo((int)StateName.MEM_WRITE);
                    break;
                case 0x6F:
                case 0x67:
                    sidePanelInformer.SetStateInfo((int)StateName.ALU_WB);
                    break;
            }
        }
        else if (TickCounter - 2 >= 0) {
            var s = TickStateValues[TickCounter - 1];

            if (s.RegisterInstrValue < 1000000) return;
            var opcode = s.RegisterInstrValue & 0x7F;

            switch(opcode)
            {
                case 0x33:
                    sidePanelInformer.SetStateInfo((int)StateName.EXECUTE_R);
                    break;
                case 0x13:
                    sidePanelInformer.SetStateInfo((int)StateName.EXECUTE_I);
                    break;
                case 0x03:
                case 0x23:
                    sidePanelInformer.SetStateInfo((int)StateName.MEM_ADDRESS);
                    break;
                case 0x63:
                    sidePanelInformer.SetStateInfo((int)StateName.BEQ);
                    break;
                case 0x6F:
                case 0x67:
                    sidePanelInformer.SetStateInfo((int)StateName.JAL);
                    break;
            }
        }
        else
        {
            sidePanelInformer.SetStateInfo((int)StateName.UNKNOWN);
        }
    }

    #region transition to next level
    private void OnEnable()
    {
        LevelManager.OnRequestNextLevelData += GetNextLevelData;
    }

    private void OnDisable()
    {
        LevelManager.OnRequestNextLevelData -= GetNextLevelData;
    }

    private static object GetNextLevelData()
    {
        if (Initial != null && Initial.nextSceneInitial != null)
        {
            return Initial.nextSceneInitial;
        }
        return null;
    }
    #endregion
}
