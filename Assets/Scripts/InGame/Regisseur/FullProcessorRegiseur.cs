using DG.Tweening;
using System;
using System.Collections;
using System.Data.Common;
using System.Security.Cryptography;
using Unity.VisualScripting;
using UnityEngine;

public struct ProcessorLevelState
{
    public int RegisterPCValue;
    public int RegisterOldPCValue;
    public int RegisterInstrValue;
    public int RegisterDataValue;
    public int RegisterScrAValue;
    public int RegisterSrcBValue;
    public int RegisterALUOutValue;

    public int firstMemoryValue;
    public int secondMemoryValue;
    public int thirdMemoryValue;
    public int fourthMemoryValue;

    public int[] RegisterFieldValue;

    public bool RegisterPCWE;
    public bool RegisterOldPCWE;
    public bool RegisterInstrWE;
    public bool RegisterDataWE;
    public bool RegisterScrAWE;
    public bool RegisterSrcBWE;
    public bool RegisterALUOutWE;

    public int ALUOperation;

    public int ExtenderOperation;

    public int MUXadrPath;
    public int MUXsrcAPath;
    public int MUXsrcBPath;
    public int MUXresultPath;
}

public enum ExerciseTyp {
    REGISTER_FIELD = 0,
    MEMORY = 1,
    BEQ = 2,
    JAL = 3,
}

public class FullProcessorRegiseur : BaseLevelRegisseur
{
    [Header("Precossor Specific Components")]
    [SerializeField] private RegisterVizualizer _registerPCVisualizer;
    [SerializeField] private RegisterVizualizer _registerOldPCVisualizer;
    [SerializeField] private RegisterVizualizer _registerIntructionVisualizer;
    [SerializeField] private RegisterVizualizer _registerDataVisualizer;
    [SerializeField] private RegisterVizualizer _registerSrcAVisualizer;
    [SerializeField] private RegisterVizualizer _registerSrcBVisualizer;
    [SerializeField] private RegisterVizualizer _registerALUOutVisualizer;

    [SerializeField] private MuiltiplexerVizualizer _adrMUXVisualizer;
    [SerializeField] private MuiltiplexerVizualizer _srcAMUXVisualizer;
    [SerializeField] private MuiltiplexerVizualizer _srcBMUXVisualizer;
    [SerializeField] private MuiltiplexerVizualizer _resultMUXVisualizer;

    [SerializeField] private ALUVizualiser _aluVizualizer;
    [SerializeField] private ExternderVizualizer _extenderVizualizer;

    [SerializeField] private IntructionDataMemoryVizualizer _memoryVisualizer;
    [SerializeField] private RegisterFileVizualizer _registerFileVisualizer;

    [SerializeField] private Blinker _numberBlinker;

    [Header("Initial values for level")]
    // [SerializeField] private ProcessorInitialState _initial;
    public static ProcessorInitialState _initial;

    [Header("Extra Panel Fields")]
    [SerializeField] private SidePanelStateInformer _sidePanelInformer;

    #region CACHED UI REFERENCES
    private InfoPanelUI _infoPCRegister;
    private InfoPanelUI _infoOldPCRegister;
    private InfoPanelUI _infoIntructionRegister;
    private InfoPanelUI _infoDataRegister;
    private InfoPanelUI _infoSrcARegister;
    private InfoPanelUI _infoSrcBRegister;
    private InfoPanelUI _infoALUOutRegister;

    private InstrMemoryControlPanel _infoDataMemory;
    #endregion

    // Intern components for computations
    private Register pc;
    private Register oldPC;
    private Register instructionReg;
    private Register dataReg;
    private Register srcA;
    private Register srcB;
    private Register aluOutReg;

    private DataInstMemory dataIntructionMemory;
    protected RegisterFile registerFile;


    protected int _currentBus = 0; // [0, 10]

    protected void Awake()
    {
        _levelManager.SetLevelDialogue(_initial.customDialogueGraph);
    }
    protected override void OnLevelStart()
    {
        pc = new Register(_initial.pcRegisterInitialValue);               pc.WriteEnable = true;
        oldPC = new Register(0);            oldPC.WriteEnable = true;
        instructionReg = new Register(0);   instructionReg.WriteEnable = true;
        dataReg = new Register(0);          dataReg.WriteEnable = true;
        srcA = new Register(0);             srcA.WriteEnable = true;
        srcB = new Register(0);             srcB.WriteEnable = true;
        aluOutReg = new Register(0);        aluOutReg.WriteEnable = true;

        registerFile = new RegisterFile(); registerFile.RegisterWriteEnable = true;
        registerFile.InitializeRegisters(new int[] { 0, 1, 39, 43, 5, 6, 8,
                                                     40, 3, 39, 13, 56, 63, 20,
                                                     50, 51, 0, 12, 53, 65, 29,
                                                     60, 61, 0, 25, 54, 0, 28,
                                                     70, 30, 31, 0});

        dataIntructionMemory = new DataInstMemory(); dataIntructionMemory.MemoryWrite = true;

        dataIntructionMemory.LoadWord(0, _initial.firstMemoWord);
        dataIntructionMemory.LoadWord(4, _initial.secondMemoWord);
        dataIntructionMemory.LoadWord(8, _initial.thirdMemoWord);
        dataIntructionMemory.LoadWord(12, _initial.fourthMemoWord);

        // Caching of UI panels for visualizers
        _infoPCRegister = _registerPCVisualizer.UIRegisterPanel;
        _infoOldPCRegister = _registerOldPCVisualizer.UIRegisterPanel;
        _infoIntructionRegister = _registerIntructionVisualizer.UIRegisterPanel;
        _infoDataRegister = _registerDataVisualizer.UIRegisterPanel;
        _infoSrcARegister = _registerSrcAVisualizer.UIRegisterPanel;
        _infoSrcBRegister = _registerSrcBVisualizer.UIRegisterPanel;
        _infoALUOutRegister = _registerALUOutVisualizer.UIRegisterPanel;


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
        ProcessorLevelState s = (ProcessorLevelState)state;

        pc = new Register(s.RegisterPCValue);
        oldPC = new Register(s.RegisterOldPCValue);
        instructionReg = new Register(s.RegisterInstrValue);
        dataReg = new Register(s.RegisterDataValue);
        srcA = new Register(s.RegisterScrAValue);
        srcB = new Register(s.RegisterSrcBValue);
        aluOutReg = new Register(s.RegisterALUOutValue);

        muxVizualizerHelper(s.MUXadrPath, _adrMUXVisualizer);
        muxVizualizerHelper(s.MUXsrcAPath, _srcAMUXVisualizer);
        muxVizualizerHelper(s.MUXsrcBPath, _srcBMUXVisualizer);
        muxVizualizerHelper(s.MUXresultPath, _resultMUXVisualizer);

        dataIntructionMemory = new DataInstMemory();
        dataIntructionMemory._memory[0] = s.firstMemoryValue;
        dataIntructionMemory._memory[4] = s.secondMemoryValue;
        dataIntructionMemory._memory[8] = s.thirdMemoryValue;
        dataIntructionMemory._memory[12] = s.fourthMemoryValue;

        // noch Register File einfuegen
        registerFile.InitializeRegisters(s.RegisterFieldValue);

        pc.WriteEnable = s.RegisterPCWE;
        oldPC.WriteEnable = s.RegisterOldPCWE;
        instructionReg.WriteEnable = s.RegisterInstrWE;
        dataReg.WriteEnable = s.RegisterDataWE;
        srcA.WriteEnable = s.RegisterScrAWE;
        srcB.WriteEnable = s.RegisterSrcBWE;
        aluOutReg.WriteEnable = s.RegisterALUOutWE;

        _aluVizualizer.ChooseALUOperation(s.ALUOperation);
        _extenderVizualizer.ChooseALUOperation(s.ExtenderOperation);
    }
    private void muxVizualizerHelper(int currentPath, MuiltiplexerVizualizer mux) {
        if (currentPath == -1)
        {
            mux.ResetVizualization();
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
        _registerPCVisualizer.TriggerBlink();
        _registerOldPCVisualizer.TriggerBlink();
        _registerIntructionVisualizer.TriggerBlink();
        _registerDataVisualizer.TriggerBlink();
        _registerSrcAVisualizer.TriggerBlink();
        _registerSrcBVisualizer.TriggerBlink();
        _registerALUOutVisualizer.TriggerBlink();

        _memoryVisualizer.TriggerBlink();
        _registerFileVisualizer.TriggerBlink();

        _numberBlinker.Trigger();
    }

    protected override void BlockIngameInteractables()
    {
        _memoryVisualizer.UIRegisterPanel.WEButton.interactable = false;
        _registerFileVisualizer.UIRegisterPanel.WEButton.interactable = false;

        SwitchInteractablesAccessability(false);

        SwitchMUXInteractables(false, _adrMUXVisualizer);
        SwitchMUXInteractables(false, _srcAMUXVisualizer);
        SwitchMUXInteractables(false, _srcBMUXVisualizer);
        SwitchMUXInteractables(false, _resultMUXVisualizer);

        _aluVizualizer.UIController.FirstOperationButton.interactable = false;
        _aluVizualizer.UIController.SecondOperationButton.interactable = false;
        _aluVizualizer.UIController.ThirdOperationButton.interactable = false;
        _aluVizualizer.UIController.FourthOperationButton.interactable = false;

        _extenderVizualizer.UIController.FirstOperationButton.interactable = false;
        _extenderVizualizer.UIController.SecondOperationButton.interactable = false;
        _extenderVizualizer.UIController.ThirdOperationButton.interactable = false;
        _extenderVizualizer.UIController.FourthOperationButton.interactable = false;
    }
    private void SwitchInteractablesAccessability(bool trigger) {
        _registerPCVisualizer.UIRegisterPanel.WEButton.interactable = trigger;
        _registerOldPCVisualizer.UIRegisterPanel.WEButton.interactable = trigger;
        _registerIntructionVisualizer.UIRegisterPanel.WEButton.interactable = trigger;
        _registerDataVisualizer.UIRegisterPanel.WEButton.interactable = trigger;
        _registerSrcAVisualizer.UIRegisterPanel.WEButton.interactable = trigger;
        _registerSrcBVisualizer.UIRegisterPanel.WEButton.interactable = trigger;
        _registerALUOutVisualizer.UIRegisterPanel.WEButton.interactable = trigger;
    }
    private void SwitchMUXInteractables(bool trigger, MuiltiplexerVizualizer target)
    {
        target.UIController.FirstWayButton.interactable = trigger;
        target.UIController.SecondWayButton.interactable = trigger;
        target.UIController.ThirdWayButton.interactable = trigger;
    }

    protected override bool CheckWinCondition()
    {
        if (_initial._aufgabeTyp == ExerciseTyp.REGISTER_FIELD)
        {
            return registerFile.Registers[_initial.RegisterFieldAdressAnswer] == _initial.RegisterFieldValueAnswer;
        }
        else if (_initial._aufgabeTyp == ExerciseTyp.MEMORY)
        {
            return dataIntructionMemory._memory[_initial.MemoryAdressAnswer] == _initial.MemoryValueAnswer;
        }
        else if (_initial._aufgabeTyp == ExerciseTyp.BEQ)
        {
            return pc.Output == _initial.pcValueAnswer;
        }
        else if (_initial._aufgabeTyp == ExerciseTyp.JAL) {
            return pc.Output == _initial.pcValueAnswer && registerFile.Registers[_initial.RegisterFieldAdressAnswer] == _initial.RegisterFieldValueAnswer;
        }
        else {
            return false;
        }
    }

    protected override object GetCurrentState()
    {
        return new ProcessorLevelState
        {
            RegisterPCValue = pc.Output,
            RegisterOldPCValue = oldPC.Output,
            RegisterInstrValue = instructionReg.Output,
            RegisterDataValue = dataReg.Output,
            RegisterScrAValue = srcA.Output,
            RegisterSrcBValue = srcB.Output,
            RegisterALUOutValue = aluOutReg.Output,

            RegisterFieldValue = registerFile.Registers,

            firstMemoryValue = dataIntructionMemory._memory[0],
            secondMemoryValue = dataIntructionMemory._memory[4],
            thirdMemoryValue = dataIntructionMemory._memory[8],
            fourthMemoryValue = dataIntructionMemory._memory[12],

            RegisterPCWE = pc.WriteEnable,
            RegisterOldPCWE = oldPC.WriteEnable,
            RegisterInstrWE = instructionReg.WriteEnable,
            RegisterDataWE = dataReg.WriteEnable,
            RegisterScrAWE = srcA.WriteEnable,
            RegisterSrcBWE = srcB.WriteEnable,
            RegisterALUOutWE = aluOutReg.WriteEnable,

            ALUOperation = _aluVizualizer.CurrentALUOperation,

            ExtenderOperation = _extenderVizualizer.CurrentALUOperation,

            MUXadrPath = _adrMUXVisualizer.CurrentChoosenMuxPath,
            MUXsrcAPath = _srcAMUXVisualizer.CurrentChoosenMuxPath,
            MUXsrcBPath = _srcBMUXVisualizer.CurrentChoosenMuxPath,
            MUXresultPath = _resultMUXVisualizer.CurrentChoosenMuxPath,
        };
    }

    protected override void HandleClockUpdate()
    {
        // sinchronyse vizualisers and concrete objects
        pc.WriteEnable = _registerPCVisualizer.isWriteEnabled;
        oldPC.WriteEnable = _registerOldPCVisualizer.isWriteEnabled;
        instructionReg.WriteEnable = _registerIntructionVisualizer.isWriteEnabled;
        dataReg.WriteEnable = _registerDataVisualizer.isWriteEnabled;
        srcA.WriteEnable = _registerSrcAVisualizer.isWriteEnabled;
        srcB.WriteEnable = _registerSrcBVisualizer.isWriteEnabled;
        aluOutReg.WriteEnable = _registerALUOutVisualizer.isWriteEnabled;
        dataIntructionMemory.MemoryWrite = _memoryVisualizer.isWriteEnabled;
        registerFile.RegisterWriteEnable = _registerFileVisualizer.isWriteEnabled;



        // implementation

        #region first step (memory)
        int tmpAdress = calculateAdressMUX();
        if (dataIntructionMemory._memory.ContainsKey(tmpAdress))
        {
            instructionReg.Input = dataIntructionMemory._memory[tmpAdress];
            dataReg.Input = dataIntructionMemory._memory[tmpAdress];
        }
        else
        {
            instructionReg.Input = 0;
            dataReg.Input = 0;
        }

        oldPC.Input = pc.Output;

        dataIntructionMemory.Adress = tmpAdress;
        #endregion

        #region second step (register file)

        // A1: [19:15] (Register Source 1)
        registerFile.ReadAdress1 = (instructionReg.Output >> 15) & 0x1F;

        // A2: [24:20] (Register Source 2)
        registerFile.ReadAdress2 = (instructionReg.Output >> 20) & 0x1F;

        registerFile.ReadRegisters();

        //Debug.LogWarning($"This is tick {_tickCounter}. A1: {registerFile.ReadAdress1}, A2: {registerFile.ReadAdress2}, A3: {(temp >> 7) & 0x1F}. Command: {commandBuilder(temp)}");

        // A3: [11:7] (Register Destination / rd)

        srcA.Input = registerFile.ReadData1;
        srcB.Input = registerFile.ReadData2;
        #endregion

        #region third step (ALU)
        aluOutReg.Input = calculateALU();
        dataIntructionMemory.WriteData = srcB.Output;
        #endregion

        #region fourth step (WB)
        
        if (_tickCounter - 2 >= 0) {
            ProcessorLevelState legcyState = (ProcessorLevelState)_tickStateValues[_tickCounter - 2];

            registerFile.WriteAdress = ((legcyState.RegisterInstrValue >> 7) & 0x1F);
            registerFile.WriteData = legcyState.RegisterInstrValue;
        }

        int tmpResult = calculateResultMUX();
        registerFile.WriteData = tmpResult;
        pc.Input = tmpResult;
        #endregion

        pc.PreClockUpdate();
        oldPC.PreClockUpdate();
        instructionReg.PreClockUpdate();
        dataReg.PreClockUpdate();
        srcA.PreClockUpdate();
        srcB.PreClockUpdate();
        aluOutReg.PreClockUpdate();
        dataIntructionMemory.PreClockUpdate();

        pc.Clock();
        oldPC.Clock();
        instructionReg.Clock();
        dataReg.Clock();
        srcA.Clock();
        srcB.Clock();
        aluOutReg.Clock();
        dataIntructionMemory.Clock();
        registerFile.Clock();
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

                (s.MUXadrPath == _adrMUXVisualizer.CurrentChoosenMuxPath) &&
                (s.MUXsrcAPath == _srcAMUXVisualizer.CurrentChoosenMuxPath) &&
                (s.MUXsrcBPath == _srcBMUXVisualizer.CurrentChoosenMuxPath) &&
                (s.MUXresultPath == _resultMUXVisualizer.CurrentChoosenMuxPath);
    }*/

    protected override void ReleaseIngameInteractables()
    {
        _memoryVisualizer.UIRegisterPanel.WEButton.interactable = true;
        _registerFileVisualizer.UIRegisterPanel.WEButton.interactable = true;

        SwitchInteractablesAccessability(true);

        SwitchMUXInteractables(true, _adrMUXVisualizer);
        SwitchMUXInteractables(true, _srcAMUXVisualizer);
        SwitchMUXInteractables(true, _srcBMUXVisualizer);
        SwitchMUXInteractables(true, _resultMUXVisualizer);

        _aluVizualizer.UIController.FirstOperationButton.interactable = true;
        _aluVizualizer.UIController.SecondOperationButton.interactable = true;
        _aluVizualizer.UIController.ThirdOperationButton.interactable = true;
        _aluVizualizer.UIController.FourthOperationButton.interactable = true;

        _extenderVizualizer.UIController.FirstOperationButton.interactable = true;
        _extenderVizualizer.UIController.SecondOperationButton.interactable = true;
        _extenderVizualizer.UIController.ThirdOperationButton.interactable = true;
        _extenderVizualizer.UIController.FourthOperationButton.interactable = true;
    }

    protected override IEnumerator RunBusVisualizations()
    {
        if (_currentBus >= 0 && _currentBus < _maxTickNumber)
        {
            switch (_currentBus % 4)
            {
                case 0: yield return RunFetchVizualization(); break;
                case 1: yield return RunDecodeVizualization(); break;
                case 2: yield return RunExecutionVizualization(); break;
                case 3: yield return RunWriteBackVizualization(); break;
            }

            _currentBus++;
        }

        yield return new WaitUntil(() => _busController.NoActiveSignals);
    }

    #region vizualization helpers
    private IEnumerator RunFetchVizualization() {
        _busController.StartBusSignal(_busController.busSegments[10], pc.Output);
        _busController.StartBusSignal(_busController.busSegments[19], pc.Output);
        _busController.StartBusSignal(_busController.busSegments[20], pc.Output);

        int MUXSrcA = calculateSrcAMUX();
        int MUXSrcB = calculateSrBMUX();
        int Output = calculateResultMUX();
        int AdressValue = calculateAdressMUX();

        yield return StartCoroutine(DelayedBusSignal(_busController.busSegments[11], AdressValue));

        // ob Value existiert
        if (dataIntructionMemory._memory.ContainsKey(AdressValue)) {
            yield return StartCoroutine(DelayedBusSignal(_busController.busSegments[12], dataIntructionMemory._memory[AdressValue]));
        }
        else
        {
            yield return StartCoroutine(DelayedBusSignal(_busController.busSegments[12], 0));
        }


        yield return StartCoroutine(DelayedBusSignal(_busController.busSegments[9], 4));
        
        yield return StartCoroutine(DelayedBusSignals(_busController.busSegments[16], _busController.busSegments[15], MUXSrcA, MUXSrcB));

        yield return StartCoroutine(DelayedBusSignal(_busController.busSegments[6], calculateALU()));

        
        yield return StartCoroutine(DelayedBusSignal(_busController.busSegments[23], Output));

        yield return new WaitUntil(() => _busController.NoActiveSignals);
    }
    private IEnumerator RunDecodeVizualization()
    {
        _busController.StartBusSignal(_busController.busSegments[0], instructionReg.Output);
        _busController.StartBusSignal(_busController.busSegments[1], instructionReg.Output);
        // _busController.StartBusSignal(_busController.busSegments[2], instructionReg.Output);
        _busController.StartBusSignal(_busController.busSegments[3], instructionReg.Output);

        int srcAValue = 0;
        int srcBValue = 0;
        if (dataIntructionMemory._memory.ContainsKey(instructionReg.Output))
        {
            srcAValue = dataIntructionMemory._memory[instructionReg.Output];
        }
        if (dataIntructionMemory._memory.ContainsKey(instructionReg.Output)) {
            srcBValue = dataIntructionMemory._memory[instructionReg.Output];
        }
        yield return StartCoroutine(DelayedBusSignals(_busController.busSegments[13], _busController.busSegments[14], srcAValue, srcBValue));

        yield return new WaitUntil(() => _busController.NoActiveSignals);
    }
    private IEnumerator RunExecutionVizualization() { // das noch korrigieren
        _busController.StartBusSignal(_busController.busSegments[16], srcA.Output);
        _busController.StartBusSignal(_busController.busSegments[8], Extender.Evaluate(_extenderVizualizer.CurrentALUOperation, (uint)instructionReg.Output));

        yield return new WaitUntil(() => _busController.NoActiveSignals);

        _busController.StartBusSignal(_busController.busSegments[22], calculateSrcAMUX());
        _busController.StartBusSignal(_busController.busSegments[7], calculateSrBMUX());

        yield return new WaitUntil(() => _busController.NoActiveSignals);
        _busController.StartBusSignal(_busController.busSegments[17], calculateALU());

        yield return new WaitUntil(() => _busController.NoActiveSignals);
    }
    private IEnumerator RunWriteBackVizualization() { // das noch korrigieren
        _busController.StartBusSignal(_busController.busSegments[18], aluOutReg.Output);
        _busController.StartBusSignal(_busController.busSegments[5], aluOutReg.Output);

        yield return new WaitUntil(() => _busController.NoActiveSignals);

        int res = calculateResultMUX();
        _busController.StartBusSignal(_busController.busSegments[23], res);
        _busController.StartBusSignal(_busController.busSegments[24], res);
        _busController.StartBusSignal(_busController.busSegments[25], res);

        yield return new WaitUntil(() => _busController.NoActiveSignals);
    }
    protected IEnumerator DelayedBusSignal(LineRenderer busToStart, int value, bool reverse = false)
    {
        yield return new WaitUntil(() => _busController.NoActiveSignals);

        _busController.StartBusSignal(busToStart, value, reverse);
    }

    protected IEnumerator DelayedBusSignals(LineRenderer firstBusToStart, LineRenderer secondBusToStart, int val1, int val2, bool firstReverse = false, bool secondReverse = false)
    {
        yield return new WaitUntil(() => _busController.NoActiveSignals);

        _busController.StartBusSignal(firstBusToStart, val1, firstReverse);
        _busController.StartBusSignal(secondBusToStart, val2, secondReverse);
    }
    private int calculateSrcAMUX() { 
        return calculateMUX(_srcAMUXVisualizer.CurrentChoosenMuxPath, pc.Output, oldPC.Output, srcA.Output);
    }
    private int calculateSrBMUX()
    {
        return calculateMUX(_srcBMUXVisualizer.CurrentChoosenMuxPath, srcB.Output, Extender.Evaluate(_extenderVizualizer.CurrentALUOperation, (uint)instructionReg.Output), 4);
    }
    private int calculateResultMUX() { 
        return calculateMUX(_resultMUXVisualizer.CurrentChoosenMuxPath, aluOutReg.Output, dataReg.Output, calculateALU());
    }
    private int calculateAdressMUX() { 
        return calculateMUX(_adrMUXVisualizer.CurrentChoosenMuxPath, pc.Output, calculateResultMUX(), 0);
    }
    private int calculateMUX(int muxCurrentPath, int first, int second, int third) {
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
    private int calculateALU() {
        int MUXSrcA = calculateMUX(_srcAMUXVisualizer.CurrentChoosenMuxPath, pc.Output, oldPC.Output, srcA.Output);

        int ExtenderTmp = 0;
        if (_tickCounter > 0) {
            ProcessorLevelState legcyState = (ProcessorLevelState)_tickStateValues[_tickCounter - 1];
            ExtenderTmp = legcyState.RegisterInstrValue;
        }
        int MUXSrcB = calculateMUX(_srcBMUXVisualizer.CurrentChoosenMuxPath, 
            srcB.Output, 
            Extender.Evaluate(_extenderVizualizer.CurrentALUOperation, (uint)ExtenderTmp), 
            4);
        return ALU.calculate(MUXSrcA, MUXSrcB, _aluVizualizer.CurrentALUOperation);
    }

    private IEnumerator ReverseFetchVizualization() { // noch zu korrigieren
        yield return StartCoroutine(DelayedBusSignal(_busController.busSegments[23], pc.Input, true));

        yield return StartCoroutine(DelayedBusSignal(_busController.busSegments[6], calculateALU(), true));

        yield return StartCoroutine(DelayedBusSignals(_busController.busSegments[16], _busController.busSegments[15], srcA.Output, srcB.Output, true, true));

        yield return StartCoroutine(DelayedBusSignal(_busController.busSegments[12], instructionReg.Input, true));

        yield return StartCoroutine(DelayedBusSignal(_busController.busSegments[9], 4, true));

        yield return StartCoroutine(DelayedBusSignal(_busController.busSegments[11], dataIntructionMemory.Adress, true));

        _busController.StartBusSignal(_busController.busSegments[10], oldPC.Input, true);
        _busController.StartBusSignal(_busController.busSegments[19], oldPC.Input, true);
        _busController.StartBusSignal(_busController.busSegments[20], oldPC.Input, true);

        yield return new WaitUntil(() => _busController.NoActiveSignals);
    }
    private IEnumerator ReverseDecodeVizualization()
    {
        yield return StartCoroutine(DelayedBusSignals(_busController.busSegments[13], _busController.busSegments[14], srcA.Input, srcB.Input, true, true));

        _busController.StartBusSignal(_busController.busSegments[0], instructionReg.Output, true);
        _busController.StartBusSignal(_busController.busSegments[1], instructionReg.Output, true);
        // _busController.StartBusSignal(_busController.busSegments[2], instructionReg.Output);
        _busController.StartBusSignal(_busController.busSegments[3], instructionReg.Output, true);
        
        yield return new WaitUntil(() => _busController.NoActiveSignals);
    } 
    private IEnumerator ReverseExecutionVizualization() // das noch korrigieren
    {
        _busController.StartBusSignal(_busController.busSegments[17], calculateALU(), true);

        yield return new WaitUntil(() => _busController.NoActiveSignals);

        _busController.StartBusSignal(_busController.busSegments[16], srcA.Output, true);
        _busController.StartBusSignal(_busController.busSegments[8], Extender.Evaluate(_extenderVizualizer.CurrentALUOperation, (uint)instructionReg.Output), true);

        yield return new WaitUntil(() => _busController.NoActiveSignals);

        _busController.StartBusSignal(_busController.busSegments[22], calculateSrcAMUX(), true);
        _busController.StartBusSignal(_busController.busSegments[7], calculateSrBMUX(), true);

        yield return new WaitUntil(() => _busController.NoActiveSignals);
    }
    private IEnumerator ReverseWriteBackVizualization() // das noch korrigieren
    {
        int res = calculateResultMUX();
        _busController.StartBusSignal(_busController.busSegments[23], res, true);
        _busController.StartBusSignal(_busController.busSegments[24], res, true);
        _busController.StartBusSignal(_busController.busSegments[25], res, true);

        yield return new WaitUntil(() => _busController.NoActiveSignals);

        _busController.StartBusSignal(_busController.busSegments[18], aluOutReg.Output, true);
        _busController.StartBusSignal(_busController.busSegments[5], aluOutReg.Output, true);

        yield return new WaitUntil(() => _busController.NoActiveSignals);
    }
    #endregion

    protected override IEnumerator ReverseBusVisualizations()
    {
        if (_currentBus >= 1 && _currentBus <= _maxTickNumber)
        {
            switch ((_currentBus - 1) % 4)
            {
                case 0: yield return ReverseFetchVizualization(); break;
                case 1: yield return ReverseDecodeVizualization(); break;
                case 2: yield return ReverseExecutionVizualization(); break;
                case 3: yield return ReverseWriteBackVizualization(); break;
            }

            _currentBus--;
        }

        yield return new WaitUntil(() => _busController.NoActiveSignals);
    }

    protected override void UpdateVizualizers()
    {
        UpdateSidePanel();

        _infoPCRegister.Display("PC Register", $"{pc.Output}");
        _infoOldPCRegister.Display("Old PC Register", $"{oldPC.Output}");
        _infoIntructionRegister.Display("Instruction Register", commandBuilder((uint)instructionReg.Output));
        _infoDataRegister.Display("Data Register", $"{dataReg.Output}");
        _infoSrcARegister.Display("SrcA Register", $"{srcA.Output}");
        _infoSrcBRegister.Display("SrcB Register", $"{srcB.Output}");
        _infoALUOutRegister.Display("ALU Out Register", $"{aluOutReg.Output}");

        _memoryVisualizer.UIRegisterPanel.Display(
            commandBuilder((uint)dataIntructionMemory._memory[0]),
            commandBuilder((uint)dataIntructionMemory._memory[4]),
            commandBuilder((uint)dataIntructionMemory._memory[8]),
            commandBuilder((uint)dataIntructionMemory._memory[12])
        );
        _registerFileVisualizer.UIRegisterPanel.Display(registerFile.Registers);


        // ==============================  WE SECTION  =====================================
        _registerPCVisualizer.ForceUpdateWriteEnableVisualization(pc.WriteEnable);
        _registerOldPCVisualizer.ForceUpdateWriteEnableVisualization(oldPC.WriteEnable);
        _registerIntructionVisualizer.ForceUpdateWriteEnableVisualization(instructionReg.WriteEnable);
        _registerDataVisualizer.ForceUpdateWriteEnableVisualization(dataReg.WriteEnable);
        _registerSrcAVisualizer.ForceUpdateWriteEnableVisualization(srcA.WriteEnable);
        _registerSrcBVisualizer.ForceUpdateWriteEnableVisualization(srcB.WriteEnable);
        _registerALUOutVisualizer.ForceUpdateWriteEnableVisualization(aluOutReg.WriteEnable);

        _registerFileVisualizer.ForceUpdateWriteEnableVisualization(registerFile.RegisterWriteEnable);
        _memoryVisualizer.ForceUpdateWriteEnableVisualization(dataIntructionMemory.MemoryWrite);
    }


    private void UpdateSidePanel() {
        bool containsKey = pc.Output % 4 == 0 
                            && pc.Output <= 12
                            && pc.Output >= 0 
                            && dataIntructionMemory._memory.ContainsKey(pc.Output)
                            && dataIntructionMemory._memory[pc.Output] > 1000000;
        if (containsKey)
        {
            _sidePanelInformer.SetStateInfo((int)StateName.FETCH);
        }
        else if (instructionReg.Output > 1000000)
        {
            _sidePanelInformer.SetStateInfo((int)StateName.DECODE);
        }
        else if (_tickCounter - 4 >= 0)
        {
            ProcessorLevelState s = (ProcessorLevelState)_tickStateValues[_tickCounter - 3];

            if (s.RegisterInstrValue >= 1000000)
            {
                int opcode = s.RegisterInstrValue & 0x7F;

                if (opcode == 0x03)
                {
                    _sidePanelInformer.SetStateInfo((int)StateName.MEM_WB);
                }
            }
        }
        else if (_tickCounter - 3 >= 0)
        {
            ProcessorLevelState s = (ProcessorLevelState)_tickStateValues[_tickCounter - 2];

            if (s.RegisterInstrValue >= 1000000)
            {
                int opcode = s.RegisterInstrValue & 0x7F;

                switch (opcode)
                {
                    case 0x33:
                    case 0x13:
                        _sidePanelInformer.SetStateInfo((int)StateName.ALU_WB);
                        break;
                    case 0x03:
                        _sidePanelInformer.SetStateInfo((int)StateName.MEM_READ);
                        break;
                    case 0x23:
                        _sidePanelInformer.SetStateInfo((int)StateName.MEM_WRITE);
                        break;
                    case 0x6F:
                    case 0x67:
                        _sidePanelInformer.SetStateInfo((int)StateName.ALU_WB);
                        break;
                }
            }
        }
        else if (_tickCounter - 2 >= 0) {
            ProcessorLevelState s = (ProcessorLevelState)_tickStateValues[_tickCounter - 1];

            if (s.RegisterInstrValue >= 1000000) {
                int opcode = s.RegisterInstrValue & 0x7F;

                 switch(opcode)
                {
                    case 0x33:
                        _sidePanelInformer.SetStateInfo((int)StateName.EXECUTE_R);
                        break;
                    case 0x13:
                        _sidePanelInformer.SetStateInfo((int)StateName.EXECUTE_I);
                        break;
                    case 0x03:
                    case 0x23:
                        _sidePanelInformer.SetStateInfo((int)StateName.MEM_ADRESS);
                        break;
                    case 0x63:
                        _sidePanelInformer.SetStateInfo((int)StateName.BEQ);
                        break;
                    case 0x6F:
                    case 0x67:
                        _sidePanelInformer.SetStateInfo((int)StateName.JAL);
                        break;
                }
            }
        }
        else
        {
            _sidePanelInformer.SetStateInfo((int)StateName.UNKNOWN);
            Debug.LogWarning($"pc: {pc.Output} | mem: {dataIntructionMemory._memory[pc.Output] > 1000000} | isDa: {dataIntructionMemory._memory.ContainsKey(pc.Output)}");
            Debug.LogError($"instruction reg: {instructionReg.Output}");
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
        if (_initial != null && _initial.nextSceneInitial != null)
        {
            return _initial.nextSceneInitial;
        }
        return null;
    }
    #endregion
}
