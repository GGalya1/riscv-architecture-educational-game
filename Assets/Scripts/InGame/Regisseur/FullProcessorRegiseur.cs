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

    public bool RegisterPcwe;
    public bool RegisterOldPcwe;
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

public class FullProcessorRegiseur : BaseLevelRegisseur
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
    protected RegisterFile RegisterFile;


    protected int CurrentBus; // [0, 10]

    protected void Awake()
    {
        levelManager.SetLevelDialogue(Initial.customDialogueGraph);
    }
    protected override void OnLevelStart()
    {
        _pc = new Register(Initial.pcRegisterInitialValue);               _pc.WriteEnable = true;
        _oldPC = new Register(0);            _oldPC.WriteEnable = true;
        _instructionReg = new Register(0);   _instructionReg.WriteEnable = true;
        _dataReg = new Register(0);          _dataReg.WriteEnable = true;
        _srcA = new Register(0);             _srcA.WriteEnable = true;
        _srcB = new Register(0);             _srcB.WriteEnable = true;
        _aluOutReg = new Register(0);        _aluOutReg.WriteEnable = true;

        RegisterFile = new RegisterFile(); RegisterFile.RegisterWriteEnable = true;
        RegisterFile.InitializeRegisters(new int[] { 0, 1, 39, 43, 5, 6, 8,
                                                     40, 3, 39, 13, 56, 63, 20,
                                                     50, 51, 0, 12, 53, 65, 29,
                                                     60, 61, 0, 25, 54, 0, 28,
                                                     70, 30, 31, 0});

        _dataIntructionMemory = new DataInstMemory(); _dataIntructionMemory.MemoryWrite = true;

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


        if (levelTargetDescription == null || levelTargetDescription.Length == 0)
        {
            levelTargetText.text = $"Hier Ziel schreiben";
        }
        else
        {
            levelTargetText.text = levelTargetDescription;
        }


        UpdateVizualizers();
    }

    protected override void ApplyState(object state)
    {
        var s = (ProcessorLevelState)state;

        _pc = new Register(s.RegisterPCValue);
        _oldPC = new Register(s.RegisterOldPCValue);
        _instructionReg = new Register(s.RegisterInstrValue);
        _dataReg = new Register(s.RegisterDataValue);
        _srcA = new Register(s.RegisterScrAValue);
        _srcB = new Register(s.RegisterSrcBValue);
        _aluOutReg = new Register(s.RegisterAluOutValue);

        MuxVizualizerHelper(s.MuXadrPath, adrMuxVisualizer);
        MuxVizualizerHelper(s.MuXsrcAPath, srcAmuxVisualizer);
        MuxVizualizerHelper(s.MuXsrcBPath, srcBmuxVisualizer);
        MuxVizualizerHelper(s.MuXresultPath, resultMuxVisualizer);

        _dataIntructionMemory = new DataInstMemory();
        _dataIntructionMemory.Memory[0] = s.FirstMemoryValue;
        _dataIntructionMemory.Memory[4] = s.SecondMemoryValue;
        _dataIntructionMemory.Memory[8] = s.ThirdMemoryValue;
        _dataIntructionMemory.Memory[12] = s.FourthMemoryValue;

        // noch Register File einfuegen
        RegisterFile.InitializeRegisters(s.RegisterFieldValue);

        _pc.WriteEnable = s.RegisterPcwe;
        _oldPC.WriteEnable = s.RegisterOldPcwe;
        _instructionReg.WriteEnable = s.RegisterInstrWe;
        _dataReg.WriteEnable = s.RegisterDataWe;
        _srcA.WriteEnable = s.RegisterScrAwe;
        _srcB.WriteEnable = s.RegisterSrcBwe;
        _aluOutReg.WriteEnable = s.RegisterAluOutWe;

        aluVizualizer.ChooseAluOperation(s.AluOperation);
        extenderVizualizer.ChooseAluOperation(s.ExtenderOperation);
    }
    private void MuxVizualizerHelper(int currentPath, MultiplexerVisualizer mux) {
        if (currentPath == -1)
        {
            mux.ResetVisualisation();
        }
        else if (currentPath == 0)
        {
            mux.SelectPath(0);
        }
        else if (currentPath == 1)
        {
            mux.SelectPath(1);
        }
        else if (currentPath == 2)
        {
            mux.SelectPath(2);
        }
        else
        {
            Debug.LogError($"Saved multiplexer value {currentPath} is not in [0, 3]");
        }
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

    protected override void BlockIngameInteractables()
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
    private void SwitchMuxInteractables(bool trigger, MultiplexerVisualizer target)
    {
        target.UIController.FirstWayButton.interactable = trigger;
        target.UIController.SecondWayButton.interactable = trigger;
        target.UIController.ThirdWayButton.interactable = trigger;
    }

    protected override bool CheckWinCondition()
    {
        if (Initial.aufgabeTyp == ExerciseTyp.REGISTER_FIELD)
        {
            return RegisterFile.Registers[Initial.registerFieldAddressAnswer] == Initial.registerFieldValueAnswer;
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
            return _pc.Output == Initial.pcValueAnswer && RegisterFile.Registers[Initial.registerFieldAddressAnswer] == Initial.registerFieldValueAnswer;
        }
        else {
            return false;
        }
    }

    protected override object GetCurrentState()
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

            RegisterFieldValue = RegisterFile.Registers,

            FirstMemoryValue = _dataIntructionMemory.Memory[0],
            SecondMemoryValue = _dataIntructionMemory.Memory[4],
            ThirdMemoryValue = _dataIntructionMemory.Memory[8],
            FourthMemoryValue = _dataIntructionMemory.Memory[12],

            RegisterPcwe = _pc.WriteEnable,
            RegisterOldPcwe = _oldPC.WriteEnable,
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
        // sinchronyse vizualisers and concrete objects
        _pc.WriteEnable = registerPCVisualizer.isWriteEnabled;
        _oldPC.WriteEnable = registerOldPCVisualizer.isWriteEnabled;
        _instructionReg.WriteEnable = registerIntructionVisualizer.isWriteEnabled;
        _dataReg.WriteEnable = registerDataVisualizer.isWriteEnabled;
        _srcA.WriteEnable = registerSrcAVisualizer.isWriteEnabled;
        _srcB.WriteEnable = registerSrcBVisualizer.isWriteEnabled;
        _aluOutReg.WriteEnable = registerAluOutVisualizer.isWriteEnabled;
        _dataIntructionMemory.MemoryWrite = memoryVisualizer.isWriteEnabled;
        RegisterFile.RegisterWriteEnable = registerFileVisualizer.isWriteEnabled;



        // implementation

        #region first step (memory)
        var tmpAdress = CalculateAdressMux();
        if (_dataIntructionMemory.Memory.ContainsKey(tmpAdress))
        {
            _instructionReg.Input = _dataIntructionMemory.Memory[tmpAdress];
            _dataReg.Input = _dataIntructionMemory.Memory[tmpAdress];
        }
        else
        {
            _instructionReg.Input = 0;
            _dataReg.Input = 0;
        }

        _oldPC.Input = _pc.Output;

        _dataIntructionMemory.Address = tmpAdress;
        #endregion

        #region second step (register file)

        // A1: [19:15] (Register Source 1)
        RegisterFile.ReadAdress1 = (_instructionReg.Output >> 15) & 0x1F;

        // A2: [24:20] (Register Source 2)
        RegisterFile.ReadAdress2 = (_instructionReg.Output >> 20) & 0x1F;

        RegisterFile.ReadRegisters();

        //Debug.LogWarning($"This is tick {_tickCounter}. A1: {registerFile.ReadAdress1}, A2: {registerFile.ReadAdress2}, A3: {(temp >> 7) & 0x1F}. Command: {commandBuilder(temp)}");

        // A3: [11:7] (Register Destination / rd)

        _srcA.Input = RegisterFile.ReadData1;
        _srcB.Input = RegisterFile.ReadData2;
        #endregion

        #region third step (ALU)
        _aluOutReg.Input = CalculateAlu();
        _dataIntructionMemory.WriteData = _srcB.Output;
        #endregion

        #region fourth step (WB)
        
        if (TickCounter - 2 >= 0) {
            var legcyState = (ProcessorLevelState)TickStateValues[TickCounter - 2];

            RegisterFile.WriteAdress = ((legcyState.RegisterInstrValue >> 7) & 0x1F);
            RegisterFile.WriteData = legcyState.RegisterInstrValue;
        }

        var tmpResult = CalculateResultMux();
        RegisterFile.WriteData = tmpResult;
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
        RegisterFile.Clock();
    }

    /*protected override bool IsStateEqual(object state)
    {
        if (!(state is ProcessorLevelState s)) return false;

        return (s.RegisterPCValue == pc.Output) &&
                (s.RegisterOldPCValue == oldPC.Output) &&
                (s.RegisterInstrValue == instructionReg.Output) &&
                (s.RegisterDataValue == dataReg.Output) &&
                (s.RegisterScrAValue == srcA.Output) &&
                (s.RegisterSrcBValue == srcB.Output) &&
                (s.RegisterALUOutValue == aluOutReg.Output) &&

                (s.firstMemoryValue == dataIntructionMemory._memory[0]) &&
                (s.secondMemoryValue == dataIntructionMemory._memory[4]) &&
                (s.thirdMemoryValue == dataIntructionMemory._memory[8]) &&
                (s.fourthMemoryValue == dataIntructionMemory._memory[12]) &&

                (s.RegisterPCWE == pc.WriteEnable) &&
                (s.RegisterOldPCWE == oldPC.WriteEnable) &&
                (s.RegisterInstrWE == instructionReg.WriteEnable) &&
                (s.RegisterDataWE == dataReg.WriteEnable) &&
                (s.RegisterScrAWE == srcA.WriteEnable) &&
                (s.RegisterSrcBWE == srcB.WriteEnable) &&
                (s.RegisterALUOutWE == aluOutReg.WriteEnable) &&

                (s.ALUOperation == _aluVizualizer.CurrentALUOperation) &&

                (s.ExtenderOperation == _extenderVizualizer.CurrentALUOperation) &&

                (s.MUXadrPath == _adrMUXVisualizer.CurrentChosenMuxPath) &&
                (s.MUXsrcAPath == _srcAMUXVisualizer.CurrentChosenMuxPath) &&
                (s.MUXsrcBPath == _srcBMUXVisualizer.CurrentChosenMuxPath) &&
                (s.MUXresultPath == _resultMUXVisualizer.CurrentChosenMuxPath);
    }*/

    protected override void ReleaseIngameInteractables()
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
        if (CurrentBus >= 0 && CurrentBus < maxTickNumber)
        {
            switch (CurrentBus % 4)
            {
                case 0: yield return RunFetchVizualization(); break;
                case 1: yield return RunDecodeVizualization(); break;
                case 2: yield return RunExecutionVizualization(); break;
                case 3: yield return RunWriteBackVizualization(); break;
            }

            CurrentBus++;
        }

        yield return new WaitUntil(() => busController.NoActiveSignals);
    }

    #region vizualization helpers
    private IEnumerator RunFetchVizualization() {
        busController.StartBusSignal(busController.busSegments[10], _pc.Output);
        busController.StartBusSignal(busController.busSegments[19], _pc.Output);
        busController.StartBusSignal(busController.busSegments[20], _pc.Output);

        var muxSrcA = CalculateSrcAmux();
        var muxSrcB = CalculateSrBmux();
        var output = CalculateResultMux();
        var adressValue = CalculateAdressMux();

        yield return StartCoroutine(DelayedBusSignal(busController.busSegments[11], adressValue));

        // ob Value existiert
        if (_dataIntructionMemory.Memory.ContainsKey(adressValue)) {
            yield return StartCoroutine(DelayedBusSignal(busController.busSegments[12], _dataIntructionMemory.Memory[adressValue]));
        }
        else
        {
            yield return StartCoroutine(DelayedBusSignal(busController.busSegments[12], 0));
        }


        yield return StartCoroutine(DelayedBusSignal(busController.busSegments[9], 4));
        
        yield return StartCoroutine(DelayedBusSignals(busController.busSegments[16], busController.busSegments[15], muxSrcA, muxSrcB));

        yield return StartCoroutine(DelayedBusSignal(busController.busSegments[6], CalculateAlu()));

        
        yield return StartCoroutine(DelayedBusSignal(busController.busSegments[23], output));

        yield return new WaitUntil(() => busController.NoActiveSignals);
    }
    private IEnumerator RunDecodeVizualization()
    {
        busController.StartBusSignal(busController.busSegments[0], _instructionReg.Output);
        busController.StartBusSignal(busController.busSegments[1], _instructionReg.Output);
        // _busController.StartBusSignal(_busController.busSegments[2], instructionReg.Output);
        busController.StartBusSignal(busController.busSegments[3], _instructionReg.Output);

        var srcAValue = 0;
        var srcBValue = 0;
        if (_dataIntructionMemory.Memory.ContainsKey(_instructionReg.Output))
        {
            srcAValue = _dataIntructionMemory.Memory[_instructionReg.Output];
        }
        if (_dataIntructionMemory.Memory.ContainsKey(_instructionReg.Output)) {
            srcBValue = _dataIntructionMemory.Memory[_instructionReg.Output];
        }
        yield return StartCoroutine(DelayedBusSignals(busController.busSegments[13], busController.busSegments[14], srcAValue, srcBValue));

        yield return new WaitUntil(() => busController.NoActiveSignals);
    }
    private IEnumerator RunExecutionVizualization() { // das noch korrigieren
        busController.StartBusSignal(busController.busSegments[16], _srcA.Output);
        busController.StartBusSignal(busController.busSegments[8], Extender.Evaluate(extenderVizualizer.CurrentAluOperation, (uint)_instructionReg.Output));

        yield return new WaitUntil(() => busController.NoActiveSignals);

        busController.StartBusSignal(busController.busSegments[22], CalculateSrcAmux());
        busController.StartBusSignal(busController.busSegments[7], CalculateSrBmux());

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
    protected IEnumerator DelayedBusSignal(LineRenderer busToStart, int value, bool reverse = false)
    {
        yield return new WaitUntil(() => busController.NoActiveSignals);

        busController.StartBusSignal(busToStart, value, reverse);
    }

    protected IEnumerator DelayedBusSignals(LineRenderer firstBusToStart, LineRenderer secondBusToStart, int val1, int val2, bool firstReverse = false, bool secondReverse = false)
    {
        yield return new WaitUntil(() => busController.NoActiveSignals);

        busController.StartBusSignal(firstBusToStart, val1, firstReverse);
        busController.StartBusSignal(secondBusToStart, val2, secondReverse);
    }
    private int CalculateSrcAmux() { 
        return CalculateMux(srcAmuxVisualizer.CurrentChosenMuxPath, _pc.Output, _oldPC.Output, _srcA.Output);
    }
    private int CalculateSrBmux()
    {
        return CalculateMux(srcBmuxVisualizer.CurrentChosenMuxPath, _srcB.Output, Extender.Evaluate(extenderVizualizer.CurrentAluOperation, (uint)_instructionReg.Output), 4);
    }
    private int CalculateResultMux() { 
        return CalculateMux(resultMuxVisualizer.CurrentChosenMuxPath, _aluOutReg.Output, _dataReg.Output, CalculateAlu());
    }
    private int CalculateAdressMux() { 
        return CalculateMux(adrMuxVisualizer.CurrentChosenMuxPath, _pc.Output, CalculateResultMux(), 0);
    }
    private int CalculateMux(int muxCurrentPath, int first, int second, int third) {
        var result = 0;
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
    private int CalculateAlu() {
        var muxSrcA = CalculateMux(srcAmuxVisualizer.CurrentChosenMuxPath, _pc.Output, _oldPC.Output, _srcA.Output);

        var extenderTmp = 0;
        if (TickCounter > 0) {
            var legcyState = (ProcessorLevelState)TickStateValues[TickCounter - 1];
            extenderTmp = legcyState.RegisterInstrValue;
        }
        var muxSrcB = CalculateMux(srcBmuxVisualizer.CurrentChosenMuxPath, 
            _srcB.Output, 
            Extender.Evaluate(extenderVizualizer.CurrentAluOperation, (uint)extenderTmp), 
            4);
        return Alu.Calculate(muxSrcA, muxSrcB, aluVizualizer.CurrentAluOperation);
    }

    private IEnumerator ReverseFetchVizualization() { // noch zu korrigieren
        yield return StartCoroutine(DelayedBusSignal(busController.busSegments[23], _pc.Input, true));

        yield return StartCoroutine(DelayedBusSignal(busController.busSegments[6], CalculateAlu(), true));

        yield return StartCoroutine(DelayedBusSignals(busController.busSegments[16], busController.busSegments[15], _srcA.Output, _srcB.Output, true, true));

        yield return StartCoroutine(DelayedBusSignal(busController.busSegments[12], _instructionReg.Input, true));

        yield return StartCoroutine(DelayedBusSignal(busController.busSegments[9], 4, true));

        yield return StartCoroutine(DelayedBusSignal(busController.busSegments[11], _dataIntructionMemory.Address, true));

        busController.StartBusSignal(busController.busSegments[10], _oldPC.Input, true);
        busController.StartBusSignal(busController.busSegments[19], _oldPC.Input, true);
        busController.StartBusSignal(busController.busSegments[20], _oldPC.Input, true);

        yield return new WaitUntil(() => busController.NoActiveSignals);
    }
    private IEnumerator ReverseDecodeVizualization()
    {
        yield return StartCoroutine(DelayedBusSignals(busController.busSegments[13], busController.busSegments[14], _srcA.Input, _srcB.Input, true, true));

        busController.StartBusSignal(busController.busSegments[0], _instructionReg.Output, true);
        busController.StartBusSignal(busController.busSegments[1], _instructionReg.Output, true);
        // _busController.StartBusSignal(_busController.busSegments[2], instructionReg.Output);
        busController.StartBusSignal(busController.busSegments[3], _instructionReg.Output, true);
        
        yield return new WaitUntil(() => busController.NoActiveSignals);
    } 
    private IEnumerator ReverseExecutionVizualization() // das noch korrigieren
    {
        busController.StartBusSignal(busController.busSegments[17], CalculateAlu(), true);

        yield return new WaitUntil(() => busController.NoActiveSignals);

        busController.StartBusSignal(busController.busSegments[16], _srcA.Output, true);
        busController.StartBusSignal(busController.busSegments[8], Extender.Evaluate(extenderVizualizer.CurrentAluOperation, (uint)_instructionReg.Output), true);

        yield return new WaitUntil(() => busController.NoActiveSignals);

        busController.StartBusSignal(busController.busSegments[22], CalculateSrcAmux(), true);
        busController.StartBusSignal(busController.busSegments[7], CalculateSrBmux(), true);

        yield return new WaitUntil(() => busController.NoActiveSignals);
    }
    private IEnumerator ReverseWriteBackVizualization() // das noch korrigieren
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
        if (CurrentBus >= 1 && CurrentBus <= maxTickNumber)
        {
            switch ((CurrentBus - 1) % 4)
            {
                case 0: yield return ReverseFetchVizualization(); break;
                case 1: yield return ReverseDecodeVizualization(); break;
                case 2: yield return ReverseExecutionVizualization(); break;
                case 3: yield return ReverseWriteBackVizualization(); break;
            }

            CurrentBus--;
        }

        yield return new WaitUntil(() => busController.NoActiveSignals);
    }

    protected override void UpdateVizualizers()
    {
        UpdateSidePanel();

        _infoPCRegister.Display("PC Register", $"{_pc.Output}");
        _infoOldPCRegister.Display("Old PC Register", $"{_oldPC.Output}");
        _infoIntructionRegister.Display("Instruction Register", CommandBuilder((uint)_instructionReg.Output));
        _infoDataRegister.Display("Data Register", $"{_dataReg.Output}");
        _infoSrcARegister.Display("SrcA Register", $"{_srcA.Output}");
        _infoSrcBRegister.Display("SrcB Register", $"{_srcB.Output}");
        _infoAluOutRegister.Display("ALU Out Register", $"{_aluOutReg.Output}");

        memoryVisualizer.UIRegisterPanel.Display(
            CommandBuilder((uint)_dataIntructionMemory.Memory[0]),
            CommandBuilder((uint)_dataIntructionMemory.Memory[4]),
            CommandBuilder((uint)_dataIntructionMemory.Memory[8]),
            CommandBuilder((uint)_dataIntructionMemory.Memory[12])
        );
        registerFileVisualizer.UIRegisterPanel.Display(RegisterFile.Registers);


        // ==============================  WE SECTION  =====================================
        registerPCVisualizer.ForceUpdateWriteEnableVisualization(_pc.WriteEnable);
        registerOldPCVisualizer.ForceUpdateWriteEnableVisualization(_oldPC.WriteEnable);
        registerIntructionVisualizer.ForceUpdateWriteEnableVisualization(_instructionReg.WriteEnable);
        registerDataVisualizer.ForceUpdateWriteEnableVisualization(_dataReg.WriteEnable);
        registerSrcAVisualizer.ForceUpdateWriteEnableVisualization(_srcA.WriteEnable);
        registerSrcBVisualizer.ForceUpdateWriteEnableVisualization(_srcB.WriteEnable);
        registerAluOutVisualizer.ForceUpdateWriteEnableVisualization(_aluOutReg.WriteEnable);

        registerFileVisualizer.ForceUpdateWriteEnableVisualization(RegisterFile.RegisterWriteEnable);
        memoryVisualizer.ForceUpdateWriteEnableVisualization(_dataIntructionMemory.MemoryWrite);
    }


    private void UpdateSidePanel() {
        var containsKey = _pc.Output % 4 == 0 
                          && _pc.Output <= 12
                          && _pc.Output >= 0 
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
            var s = (ProcessorLevelState)TickStateValues[TickCounter - 3];

            if (s.RegisterInstrValue >= 1000000)
            {
                var opcode = s.RegisterInstrValue & 0x7F;

                if (opcode == 0x03)
                {
                    sidePanelInformer.SetStateInfo((int)StateName.MEM_WB);
                }
            }
        }
        else if (TickCounter - 3 >= 0)
        {
            var s = (ProcessorLevelState)TickStateValues[TickCounter - 2];

            if (s.RegisterInstrValue >= 1000000)
            {
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
        }
        else if (TickCounter - 2 >= 0) {
            var s = (ProcessorLevelState)TickStateValues[TickCounter - 1];

            if (s.RegisterInstrValue >= 1000000) {
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
        }
        else
        {
            sidePanelInformer.SetStateInfo((int)StateName.UNKNOWN);
            Debug.LogWarning($"pc: {_pc.Output} | mem: {_dataIntructionMemory.Memory[_pc.Output] > 1000000} | isDa: {_dataIntructionMemory.Memory.ContainsKey(_pc.Output)}");
            Debug.LogError($"instruction reg: {_instructionReg.Output}");
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

    private object GetNextLevelData()
    {
        if (Initial != null && Initial.nextSceneInitial != null)
        {
            return Initial.nextSceneInitial;
        }
        return null;
    }
    #endregion
}
