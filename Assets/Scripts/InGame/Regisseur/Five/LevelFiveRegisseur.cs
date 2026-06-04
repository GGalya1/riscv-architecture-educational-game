using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;


public struct LevelFiveState
{
    public int RegisterPCValue;
    public int RegisterInstrValue;
    public int RegisterOutputValue;

    public int FirstMemoryValue;
    public int SecondMemoryValue;
    public int ThirdMemoryValue;
    public int FourthMemoryValue;

    public bool RegisterPcwe;
    public bool RegisterInstrWe;
    public bool RegisterOutputWe;

    public int AluOperation;

    public int ExtenderOperation;
}

public class LevelFiveRegisseur : BaseLevelRegisseur
{
    [FormerlySerializedAs("_registerSrcAVisualizer")]
    [Header("Level 5 Specific Components")]
    [SerializeField] private RegisterVisualizer registerSrcAVisualizer;
    [FormerlySerializedAs("_registerSrcBVisualizer")] [SerializeField] private RegisterVisualizer registerSrcBVisualizer;
    [FormerlySerializedAs("_registerOutputVisualizer")] [SerializeField] private RegisterVisualizer registerOutputVisualizer;
    [FormerlySerializedAs("_memoryVisualizer")] [SerializeField] private InstructionDataMemoryVisualizer memoryVisualizer;
    [FormerlySerializedAs("_aluVizualizer")] [SerializeField] private AluVisualiser aluVizualizer;
    [FormerlySerializedAs("_extenderVizualizer")] [SerializeField] private ExtenderVisualizer extenderVizualizer;

    [FormerlySerializedAs("_blinkerNumber")] [SerializeField] private Blinker blinkerNumber;

    #region CACHED UI REFERENCES
    private InfoPanelUI _infoSrcARegister;
    private InfoPanelUI _infoSrcBRegister;
    private InfoPanelUI _infoOutputRegister;
    private InstrMemoryControlPanel _infoDataMemory; // ?
    #endregion

    // Intern components for computations
    private Register _srcA;
    private Register _srcB;
    private Register _output;
    private DataInstMemory _dataIntructionMemory;

    protected override int RightAnswerValue => 66;


    protected int CurrentBus; // [0, 6]

    protected override void OnLevelStart()
    {
        // Initialization of logical components
        _srcA = new Register(0); _srcA.WriteEnable = true;
        _srcB = new Register(0); _srcB.WriteEnable = true;
        _output = new Register(0); _output.WriteEnable = true;

        _dataIntructionMemory = new DataInstMemory(); _dataIntructionMemory.MemoryWrite = true;

        _dataIntructionMemory.LoadWord(0, 1048576239);                   // J-Typ (1000)
        _dataIntructionMemory.LoadWord(4, 4314211);                      // B-Typ (16)
        _dataIntructionMemory.LoadWord(8, 7603235);                      // S-Typ (8)
        _dataIntructionMemory.LoadWord(12, 4301059);                     // I-Typ (4)

        // Caching of UI panels for visualizers
        _infoSrcARegister = registerSrcAVisualizer.UIRegisterPanel;
        _infoSrcBRegister = registerSrcBVisualizer.UIRegisterPanel;
        _infoOutputRegister = registerOutputVisualizer.UIRegisterPanel;

        _infoDataMemory = memoryVisualizer.UIRegisterPanel;

        SetLevelTargetText(levelTargetDescription);
            

        memoryVisualizer.UIRegisterPanel.Display($"{_dataIntructionMemory.Memory[0]}", $"{_dataIntructionMemory.Memory[4]}", $"{_dataIntructionMemory.Memory[8]}", $"{_dataIntructionMemory.Memory[12]}");
        UpdateVizualizers();
    }

    protected override IEnumerator RunBusVisualizations() 
    {
        if (CurrentBus >= 0 && CurrentBus < maxTickNumber)
        {
            busController.StartBusSignal(busController.busSegments[0], _srcA.Output);
            busController.StartBusSignal(busController.busSegments[4], _srcA.Output);
            busController.StartBusSignal(busController.busSegments[5], 4);

            if (_dataIntructionMemory.Memory.ContainsKey(_srcA.Output))
            {
                yield return StartCoroutine(DelayedBusSignals(busController.busSegments[1], busController.busSegments[6], _dataIntructionMemory.Memory[_srcA.Output], Alu.Calculate(_srcA.Output, 4, aluVizualizer.CurrentAluOperation)));
            }
            else
            {
                yield return StartCoroutine(DelayedBusSignals(busController.busSegments[1], busController.busSegments[6], 0, Alu.Calculate(_srcA.Output, 4, aluVizualizer.CurrentAluOperation)));
            }

            yield return StartCoroutine(DelayedBusSignal(busController.busSegments[2], _srcB.Output));


            yield return StartCoroutine(DelayedBusSignal(busController.busSegments[3], Extender.Evaluate(extenderVizualizer.CurrentAluOperation, (uint)_srcB.Output)));

            CurrentBus++;
        }

        yield return new WaitUntil(() => busController.NoActiveSignals);
    }

    protected override IEnumerator ReverseBusVisualizations()
    {
        if (CurrentBus >= 1 && CurrentBus <= maxTickNumber)
        {
            busController.StartBusSignal(busController.busSegments[3], _output.Input, true);

            if (TickStateValues[TickCounter] is LevelFiveState s)
            {
                yield return StartCoroutine(DelayedBusSignal(busController.busSegments[2], s.RegisterInstrValue, true));

                yield return StartCoroutine(DelayedBusSignals(busController.busSegments[1], busController.busSegments[6], _srcB.Input, _srcA.Input, true, true));

                yield return new WaitUntil(() => busController.NoActiveSignals);

                busController.StartBusSignal(busController.busSegments[0], _srcA.Output, true);
                busController.StartBusSignal(busController.busSegments[4], _srcA.Output, true);
                busController.StartBusSignal(busController.busSegments[5], 4, true);
            }

            CurrentBus--;
        }

        yield return new WaitUntil(() => busController.NoActiveSignals);
    }

    protected override void HandleClockUpdate() {
        
        // sinchronyse vizualisers and concrete objects
        _srcA.WriteEnable = registerSrcAVisualizer.isWriteEnabled;
        _srcB.WriteEnable = registerSrcBVisualizer.isWriteEnabled;
        _output.WriteEnable = registerOutputVisualizer.isWriteEnabled;
        _dataIntructionMemory.MemoryWrite = registerOutputVisualizer.isWriteEnabled;

        // implementation
        if (_dataIntructionMemory.Memory.ContainsKey(_srcA.Output))
        {
            _srcB.Input = _dataIntructionMemory.Memory[_srcA.Output];
        }
        else
        {
            _srcB.Input = 0;
            // if(dataIntructionMemory.MemoryWrite)
            //  XXX
        }

        _srcA.Input = Alu.Calculate(_srcA.Output, 4, aluVizualizer.CurrentAluOperation);

        _output.Input = Extender.Evaluate(extenderVizualizer.CurrentAluOperation, (uint)_srcB.Output);

        if (CurrentBus == 2)
        {
            _dataIntructionMemory.LoadWord(0, _output.Output);
        }
        else if (CurrentBus == 3)
        {
            _dataIntructionMemory.LoadWord(4, _output.Output);
        }
        else if (CurrentBus == 4)
        {
            _dataIntructionMemory.LoadWord(8, _output.Output);
        }
        else if (CurrentBus == 5)
        {
            _dataIntructionMemory.LoadWord(12, _output.Output);
        }


        _srcA.PreClockUpdate();
        _srcB.PreClockUpdate();
        _output.PreClockUpdate();
        _dataIntructionMemory.PreClockUpdate();


        
        _srcA.Clock();
        _srcB.Clock();
        _output.Clock();
        _dataIntructionMemory.Clock();
    }

    protected override object GetCurrentState() {
        return new LevelFiveState
        {
            RegisterPCValue = _srcA.Output,
            RegisterInstrValue = _srcB.Output,
            RegisterOutputValue = _output.Output,

            FirstMemoryValue = _dataIntructionMemory.Memory[0],
            SecondMemoryValue = _dataIntructionMemory.Memory[4],
            ThirdMemoryValue = _dataIntructionMemory.Memory[8],
            FourthMemoryValue = _dataIntructionMemory.Memory[12],

            RegisterPcwe = _srcA.WriteEnable,
            RegisterInstrWe = _srcB.WriteEnable,
            RegisterOutputWe = _output.WriteEnable,


            AluOperation = aluVizualizer.CurrentAluOperation,

            ExtenderOperation = extenderVizualizer.CurrentAluOperation,
};
    }

    protected override void ApplyState(object state)
    {
        var s = (LevelFiveState)state;

        _srcA = new Register(s.RegisterPCValue);
        _srcB = new Register(s.RegisterInstrValue);
        _output = new Register(s.RegisterOutputValue);

        _dataIntructionMemory = new DataInstMemory();
        _dataIntructionMemory.Memory[0] = s.FirstMemoryValue;
        _dataIntructionMemory.Memory[4] = s.SecondMemoryValue;
        _dataIntructionMemory.Memory[8] = s.ThirdMemoryValue;
        _dataIntructionMemory.Memory[12] = s.FourthMemoryValue;

        _srcA.WriteEnable = s.RegisterPcwe;
        _srcB.WriteEnable = s.RegisterInstrWe;
        _output.WriteEnable = s.RegisterOutputWe;


        aluVizualizer.ChooseAluOperation(s.AluOperation);
        extenderVizualizer.ChooseAluOperation(s.ExtenderOperation);
    }

    /*protected override bool IsStateEqual(object state)
    {
        if (!(state is LevelFiveState s)) return false;

        return (s.RegisterPCValue == srcA.Output) &&
                (s.RegisterInstrValue == srcB.Output) &&
                (s.RegisterOutputValue == output.Output) &&

                (s.firstMemoryValue == dataIntructionMemory._memory[0]) &&
                (s.secondMemoryValue == dataIntructionMemory._memory[4]) &&
                (s.thirdMemoryValue == dataIntructionMemory._memory[8]) &&
                (s.fourthMemoryValue == dataIntructionMemory._memory[12]) &&

                (s.RegisterPCWE == srcA.WriteEnable) &&
                (s.RegisterInstrWE == srcB.WriteEnable) &&
                (s.RegisterOutputWE == output.WriteEnable) &&
                (s.ALUOperation == _aluVizualizer.CurrentALUOperation) &&
                (s.ExtenderOperation == _extenderVizualizer.CurrentALUOperation);
    }*/

    protected override void BlinkClockedComponents()
    {
        registerSrcAVisualizer.TriggerBlink();
        registerSrcBVisualizer.TriggerBlink();
        registerOutputVisualizer.TriggerBlink();
        memoryVisualizer.TriggerBlink();
        blinkerNumber.Trigger();
    }

    protected override void BlockIngameInteractables()
    {
        registerSrcAVisualizer.UIRegisterPanel.WeButton.interactable = false;
        registerSrcBVisualizer.UIRegisterPanel.WeButton.interactable = false;
        registerOutputVisualizer.UIRegisterPanel.WeButton.interactable = false;
        memoryVisualizer.UIRegisterPanel.WeButton.interactable = false;

        aluVizualizer.uiController.FirstOperationButton.interactable = false;
        aluVizualizer.uiController.SecondOperationButton.interactable = false;
        aluVizualizer.uiController.ThirdOperationButton.interactable = false;
        aluVizualizer.uiController.FourthOperationButton.interactable = false;

        extenderVizualizer.uiController.FirstOperationButton.interactable = false;
        extenderVizualizer.uiController.SecondOperationButton.interactable = false;
        extenderVizualizer.uiController.ThirdOperationButton.interactable = false;
        extenderVizualizer.uiController.FourthOperationButton.interactable = false;
    }

    protected override void ReleaseIngameInteractables()
    {
        registerSrcAVisualizer.UIRegisterPanel.WeButton.interactable = true;
        registerSrcBVisualizer.UIRegisterPanel.WeButton.interactable = true;
        registerOutputVisualizer.UIRegisterPanel.WeButton.interactable = true;
        memoryVisualizer.UIRegisterPanel.WeButton.interactable = true;

        aluVizualizer.uiController.FirstOperationButton.interactable = true;
        aluVizualizer.uiController.SecondOperationButton.interactable = true;
        aluVizualizer.uiController.ThirdOperationButton.interactable = true;
        aluVizualizer.uiController.FourthOperationButton.interactable = true;

        extenderVizualizer.uiController.FirstOperationButton.interactable = true;
        extenderVizualizer.uiController.SecondOperationButton.interactable = true;
        extenderVizualizer.uiController.ThirdOperationButton.interactable = true;
        extenderVizualizer.uiController.FourthOperationButton.interactable = true;
    }

    protected override bool CheckWinCondition()
    {
        if (TickStateValues == null 
            || TickStateValues[4] == null
            || TickStateValues[2] == null
            || TickStateValues[3] == null) 
        { return false; }

        if (!(TickStateValues[4] is LevelFiveState s3)) return false;
        if (!(TickStateValues[2] is LevelFiveState s1)) return false;
        if (!(TickStateValues[3] is LevelFiveState s2)) return false;

        var val1 = (uint)s1.RegisterOutputValue;
        var val2 = (uint)s2.RegisterOutputValue;
        var val3 = (uint)s3.RegisterOutputValue;
        var val4 = (uint)_output.Output;

        if (val1 != 1000 ||
            val2 != 8 ||
            val3 != 8 ||
            val4 != 4)
        {
            return false;
        }

        return true;
    }

    protected override void UpdateVizualizers()
    {
        _infoSrcARegister.Display("Register 1", $"{_srcA.Output}");
        _infoSrcBRegister.Display("Register 2", CommandBuilder((uint)_srcB.Output));
        _infoOutputRegister.Display("Register 3", $"{_output.Output}");
        registerSrcAVisualizer.ForceUpdateWriteEnableVisualization(_srcA.WriteEnable);
        registerSrcBVisualizer.ForceUpdateWriteEnableVisualization(_srcB.WriteEnable);
        registerOutputVisualizer.ForceUpdateWriteEnableVisualization(_output.WriteEnable);

        
        memoryVisualizer.UIRegisterPanel.Display(
            CommandBuilder((uint)_dataIntructionMemory.Memory[0]),
            CommandBuilder((uint)_dataIntructionMemory.Memory[4]),
            CommandBuilder((uint)_dataIntructionMemory.Memory[8]),
            CommandBuilder((uint)_dataIntructionMemory.Memory[12])
        );
        memoryVisualizer.ForceUpdateWriteEnableVisualization (_dataIntructionMemory.MemoryWrite);
    }

    #region helpers
    protected IEnumerator DelayedBusSignal(LineRenderer busToStart, bool reverse = false)
    {
        yield return new WaitUntil(() => busController.NoActiveSignals);

        // We're sending the third signal
        busController.StartBusSignal(busToStart, reverse);
    }
    protected IEnumerator DelayedBusSignal(LineRenderer busToStart, int value, bool reverse = false)
    {
        yield return new WaitUntil(() => busController.NoActiveSignals);

        // We're sending the third signal
        busController.StartBusSignal(busToStart, value, reverse);
    }

    protected IEnumerator DelayedBusSignals(LineRenderer firstBusToStart, LineRenderer secondBusToStart)
    {
        yield return new WaitUntil(() => busController.NoActiveSignals);

        busController.StartBusSignal(firstBusToStart);
        busController.StartBusSignal(secondBusToStart);
    }
    protected IEnumerator DelayedBusSignals(LineRenderer firstBusToStart, LineRenderer secondBusToStart, bool firstReverse, bool secondReverse)
    {
        yield return new WaitUntil(() => busController.NoActiveSignals);

        busController.StartBusSignal(firstBusToStart, firstReverse);
        busController.StartBusSignal(secondBusToStart, secondReverse);
    }
    protected IEnumerator DelayedBusSignals(LineRenderer firstBusToStart, LineRenderer secondBusToStart, int val1, int val2)
    {
        yield return new WaitUntil(() => busController.NoActiveSignals);

        busController.StartBusSignal(firstBusToStart, val1);
        busController.StartBusSignal(secondBusToStart, val2);
    }
    protected IEnumerator DelayedBusSignals(LineRenderer firstBusToStart, LineRenderer secondBusToStart, int val1, int val2, bool firstReverse, bool secondReverse)
    {
        yield return new WaitUntil(() => busController.NoActiveSignals);

        busController.StartBusSignal(firstBusToStart, val1, firstReverse);
        busController.StartBusSignal(secondBusToStart, val2, secondReverse);
    }
    #endregion
}
