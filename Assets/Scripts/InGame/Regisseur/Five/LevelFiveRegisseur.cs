using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;


public struct LevelFiveState
{
    public int RegisterPCValue;
    public int RegisterInstrValue;
    public int RegisterOutputValue;

    public int firstMemoryValue;
    public int secondMemoryValue;
    public int thirdMemoryValue;
    public int fourthMemoryValue;

    public bool RegisterPCWE;
    public bool RegisterInstrWE;
    public bool RegisterOutputWE;

    public int ALUOperation;

    public int ExtenderOperation;
}

public class LevelFiveRegisseur : BaseLevelRegisseur
{
    [Header("Level 5 Specific Components")]
    [SerializeField] private RegisterVizualizer _registerSrcAVisualizer;
    [SerializeField] private RegisterVizualizer _registerSrcBVisualizer;
    [SerializeField] private RegisterVizualizer _registerOutputVisualizer;
    [SerializeField] private IntructionDataMemoryVizualizer _memoryVisualizer;
    [SerializeField] private ALUVizualiser _aluVizualizer;
    [SerializeField] private ExternderVizualizer _extenderVizualizer;

    [SerializeField] private Blinker _blinkerNumber;

    #region CACHED UI REFERENCES
    private InfoPanelUI _infoSrcARegister;
    private InfoPanelUI _infoSrcBRegister;
    private InfoPanelUI _infoOutputRegister;
    private InstrMemoryControlPanel _infoDataMemory; // ?
    #endregion

    // Intern components for computations
    private Register srcA;
    private Register srcB;
    private Register output;
    private DataInstMemory dataIntructionMemory;

    protected override int RightAnswerValue => 66;


    protected int _currentBus = 0; // [0, 6]

    protected override void OnLevelStart()
    {
        // Initialization of logical components
        srcA = new Register(0); srcA.WriteEnable = true;
        srcB = new Register(0); srcB.WriteEnable = true;
        output = new Register(0); output.WriteEnable = true;

        dataIntructionMemory = new DataInstMemory(); dataIntructionMemory.MemoryWrite = true;

        dataIntructionMemory.LoadWord(0, 1048576239);                   // J-Typ (1000)
        dataIntructionMemory.LoadWord(4, 4314211);                      // B-Typ (16)
        dataIntructionMemory.LoadWord(8, 7603235);                      // S-Typ (8)
        dataIntructionMemory.LoadWord(12, 4301059);                     // I-Typ (4)

        // Caching of UI panels for visualizers
        _infoSrcARegister = _registerSrcAVisualizer.UIRegisterPanel;
        _infoSrcBRegister = _registerSrcBVisualizer.UIRegisterPanel;
        _infoOutputRegister = _registerOutputVisualizer.UIRegisterPanel;

        _infoDataMemory = _memoryVisualizer.UIRegisterPanel;

        if (_levelTargetDescription == null || _levelTargetDescription.Length == 0) {
            _levelTargetText.text = $"Ziel: \r\nExtende alle Werte aus dem Speicher korrekt und lege in Register 3.";
        }
        else {
            _levelTargetText.text = _levelTargetDescription;
        }
            

        _memoryVisualizer.UIRegisterPanel.Display($"{dataIntructionMemory._memory[0]}", $"{dataIntructionMemory._memory[4]}", $"{dataIntructionMemory._memory[8]}", $"{dataIntructionMemory._memory[12]}");
        UpdateVizualizers();
    }

    protected override IEnumerator RunBusVisualizations() 
    {
        if (_currentBus >= 0 && _currentBus < _maxTickNumber)
        {
            _busController.StartBusSignal(_busController.busSegments[0], srcA.Output);
            _busController.StartBusSignal(_busController.busSegments[4], srcA.Output);
            _busController.StartBusSignal(_busController.busSegments[5], 4);

            if (dataIntructionMemory._memory.ContainsKey(srcA.Output))
            {
                yield return StartCoroutine(DelayedBusSignals(_busController.busSegments[1], _busController.busSegments[6], dataIntructionMemory._memory[srcA.Output], ALU.calculate(srcA.Output, 4, _aluVizualizer.CurrentALUOperation)));
            }
            else
            {
                yield return StartCoroutine(DelayedBusSignals(_busController.busSegments[1], _busController.busSegments[6], 0, ALU.calculate(srcA.Output, 4, _aluVizualizer.CurrentALUOperation)));
            }

            yield return StartCoroutine(DelayedBusSignal(_busController.busSegments[2], srcB.Output));


            yield return StartCoroutine(DelayedBusSignal(_busController.busSegments[3], Extender.Evaluate(_extenderVizualizer.CurrentALUOperation, (uint)srcB.Output)));

            _currentBus++;
        }

        yield return new WaitUntil(() => _busController.NoActiveSignals);
    }

    protected override IEnumerator ReverseBusVisualizations()
    {
        if (_currentBus >= 1 && _currentBus <= _maxTickNumber)
        {
            _busController.StartBusSignal(_busController.busSegments[3], output.Input, true);

            if (_tickStateValues[_tickCounter] is LevelFiveState s)
            {
                yield return StartCoroutine(DelayedBusSignal(_busController.busSegments[2], s.RegisterInstrValue, true));

                yield return StartCoroutine(DelayedBusSignals(_busController.busSegments[1], _busController.busSegments[6], srcB.Input, srcA.Input, true, true));

                yield return new WaitUntil(() => _busController.NoActiveSignals);

                _busController.StartBusSignal(_busController.busSegments[0], srcA.Output, true);
                _busController.StartBusSignal(_busController.busSegments[4], srcA.Output, true);
                _busController.StartBusSignal(_busController.busSegments[5], 4, true);
            }

            _currentBus--;
        }

        yield return new WaitUntil(() => _busController.NoActiveSignals);
    }

    protected override void HandleClockUpdate() {
        
        // sinchronyse vizualisers and concrete objects
        srcA.WriteEnable = _registerSrcAVisualizer.isWriteEnabled;
        srcB.WriteEnable = _registerSrcBVisualizer.isWriteEnabled;
        output.WriteEnable = _registerOutputVisualizer.isWriteEnabled;
        dataIntructionMemory.MemoryWrite = _registerOutputVisualizer.isWriteEnabled;

        // implementation
        if (dataIntructionMemory._memory.ContainsKey(srcA.Output))
        {
            srcB.Input = dataIntructionMemory._memory[srcA.Output];
        }
        else
        {
            srcB.Input = 0;
            // if(dataIntructionMemory.MemoryWrite)
            //  XXX
        }

        srcA.Input = ALU.calculate(srcA.Output, 4, _aluVizualizer.CurrentALUOperation);

        output.Input = Extender.Evaluate(_extenderVizualizer.CurrentALUOperation, (uint)srcB.Output);

        if (_currentBus == 2)
        {
            dataIntructionMemory.LoadWord(0, output.Output);
        }
        else if (_currentBus == 3)
        {
            dataIntructionMemory.LoadWord(4, output.Output);
        }
        else if (_currentBus == 4)
        {
            dataIntructionMemory.LoadWord(8, output.Output);
        }
        else if (_currentBus == 5)
        {
            dataIntructionMemory.LoadWord(12, output.Output);
        }


        srcA.PreClockUpdate();
        srcB.PreClockUpdate();
        output.PreClockUpdate();
        dataIntructionMemory.PreClockUpdate();


        
        srcA.Clock();
        srcB.Clock();
        output.Clock();
        dataIntructionMemory.Clock();
    }

    protected override object GetCurrentState() {
        return new LevelFiveState
        {
            RegisterPCValue = srcA.Output,
            RegisterInstrValue = srcB.Output,
            RegisterOutputValue = output.Output,

            firstMemoryValue = dataIntructionMemory._memory[0],
            secondMemoryValue = dataIntructionMemory._memory[4],
            thirdMemoryValue = dataIntructionMemory._memory[8],
            fourthMemoryValue = dataIntructionMemory._memory[12],

            RegisterPCWE = srcA.WriteEnable,
            RegisterInstrWE = srcB.WriteEnable,
            RegisterOutputWE = output.WriteEnable,


            ALUOperation = _aluVizualizer.CurrentALUOperation,

            ExtenderOperation = _extenderVizualizer.CurrentALUOperation,
};
    }

    protected override void ApplyState(object state)
    {
        LevelFiveState s = (LevelFiveState)state;

        srcA = new Register(s.RegisterPCValue);
        srcB = new Register(s.RegisterInstrValue);
        output = new Register(s.RegisterOutputValue);

        dataIntructionMemory = new DataInstMemory();
        dataIntructionMemory._memory[0] = s.firstMemoryValue;
        dataIntructionMemory._memory[4] = s.secondMemoryValue;
        dataIntructionMemory._memory[8] = s.thirdMemoryValue;
        dataIntructionMemory._memory[12] = s.fourthMemoryValue;

        srcA.WriteEnable = s.RegisterPCWE;
        srcB.WriteEnable = s.RegisterInstrWE;
        output.WriteEnable = s.RegisterOutputWE;


        _aluVizualizer.ChooseALUOperation(s.ALUOperation);
        _extenderVizualizer.ChooseALUOperation(s.ExtenderOperation);
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
        _registerSrcAVisualizer.TriggerBlink();
        _registerSrcBVisualizer.TriggerBlink();
        _registerOutputVisualizer.TriggerBlink();
        _memoryVisualizer.TriggerBlink();
        _blinkerNumber.Trigger();
    }

    protected override void BlockIngameInteractables()
    {
        _registerSrcAVisualizer.UIRegisterPanel.WEButton.interactable = false;
        _registerSrcBVisualizer.UIRegisterPanel.WEButton.interactable = false;
        _registerOutputVisualizer.UIRegisterPanel.WEButton.interactable = false;
        _memoryVisualizer.UIRegisterPanel.WEButton.interactable = false;

        _aluVizualizer.UIController.FirstOperationButton.interactable = false;
        _aluVizualizer.UIController.SecondOperationButton.interactable = false;
        _aluVizualizer.UIController.ThirdOperationButton.interactable = false;
        _aluVizualizer.UIController.FourthOperationButton.interactable = false;

        _extenderVizualizer.UIController.FirstOperationButton.interactable = false;
        _extenderVizualizer.UIController.SecondOperationButton.interactable = false;
        _extenderVizualizer.UIController.ThirdOperationButton.interactable = false;
        _extenderVizualizer.UIController.FourthOperationButton.interactable = false;
    }

    protected override void ReleaseIngameInteractables()
    {
        _registerSrcAVisualizer.UIRegisterPanel.WEButton.interactable = true;
        _registerSrcBVisualizer.UIRegisterPanel.WEButton.interactable = true;
        _registerOutputVisualizer.UIRegisterPanel.WEButton.interactable = true;
        _memoryVisualizer.UIRegisterPanel.WEButton.interactable = true;

        _aluVizualizer.UIController.FirstOperationButton.interactable = true;
        _aluVizualizer.UIController.SecondOperationButton.interactable = true;
        _aluVizualizer.UIController.ThirdOperationButton.interactable = true;
        _aluVizualizer.UIController.FourthOperationButton.interactable = true;

        _extenderVizualizer.UIController.FirstOperationButton.interactable = true;
        _extenderVizualizer.UIController.SecondOperationButton.interactable = true;
        _extenderVizualizer.UIController.ThirdOperationButton.interactable = true;
        _extenderVizualizer.UIController.FourthOperationButton.interactable = true;
    }

    protected override bool CheckWinCondition()
    {
        if (_tickStateValues == null 
            || _tickStateValues[4] == null
            || _tickStateValues[2] == null
            || _tickStateValues[3] == null) 
        { return false; }

        if (!(_tickStateValues[4] is LevelFiveState s3)) return false;
        if (!(_tickStateValues[2] is LevelFiveState s1)) return false;
        if (!(_tickStateValues[3] is LevelFiveState s2)) return false;

        uint val1 = (uint)s1.RegisterOutputValue;
        uint val2 = (uint)s2.RegisterOutputValue;
        uint val3 = (uint)s3.RegisterOutputValue;
        uint val4 = (uint)output.Output;

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
        _infoSrcARegister.Display("Register 1", $"{srcA.Output}");
        _infoSrcBRegister.Display("Register 2", commandBuilder((uint)srcB.Output));
        _infoOutputRegister.Display("Register 3", $"{output.Output}");
        _registerSrcAVisualizer.ForceUpdateWriteEnableVisualization(srcA.WriteEnable);
        _registerSrcBVisualizer.ForceUpdateWriteEnableVisualization(srcB.WriteEnable);
        _registerOutputVisualizer.ForceUpdateWriteEnableVisualization(output.WriteEnable);

        
        _memoryVisualizer.UIRegisterPanel.Display(
            commandBuilder((uint)dataIntructionMemory._memory[0]),
            commandBuilder((uint)dataIntructionMemory._memory[4]),
            commandBuilder((uint)dataIntructionMemory._memory[8]),
            commandBuilder((uint)dataIntructionMemory._memory[12])
        );
        _memoryVisualizer.ForceUpdateWriteEnableVisualization (dataIntructionMemory.MemoryWrite);
    }

    #region helpers
    protected IEnumerator DelayedBusSignal(LineRenderer busToStart, bool reverse = false)
    {
        yield return new WaitUntil(() => _busController.NoActiveSignals);

        // We're sending the third signal
        _busController.StartBusSignal(busToStart, reverse);
    }
    protected IEnumerator DelayedBusSignal(LineRenderer busToStart, int value, bool reverse = false)
    {
        yield return new WaitUntil(() => _busController.NoActiveSignals);

        // We're sending the third signal
        _busController.StartBusSignal(busToStart, value, reverse);
    }

    protected IEnumerator DelayedBusSignals(LineRenderer firstBusToStart, LineRenderer secondBusToStart)
    {
        yield return new WaitUntil(() => _busController.NoActiveSignals);

        _busController.StartBusSignal(firstBusToStart);
        _busController.StartBusSignal(secondBusToStart);
    }
    protected IEnumerator DelayedBusSignals(LineRenderer firstBusToStart, LineRenderer secondBusToStart, bool firstReverse, bool secondReverse)
    {
        yield return new WaitUntil(() => _busController.NoActiveSignals);

        _busController.StartBusSignal(firstBusToStart, firstReverse);
        _busController.StartBusSignal(secondBusToStart, secondReverse);
    }
    protected IEnumerator DelayedBusSignals(LineRenderer firstBusToStart, LineRenderer secondBusToStart, int val1, int val2)
    {
        yield return new WaitUntil(() => _busController.NoActiveSignals);

        _busController.StartBusSignal(firstBusToStart, val1);
        _busController.StartBusSignal(secondBusToStart, val2);
    }
    protected IEnumerator DelayedBusSignals(LineRenderer firstBusToStart, LineRenderer secondBusToStart, int val1, int val2, bool firstReverse, bool secondReverse)
    {
        yield return new WaitUntil(() => _busController.NoActiveSignals);

        _busController.StartBusSignal(firstBusToStart, val1, firstReverse);
        _busController.StartBusSignal(secondBusToStart, val2, secondReverse);
    }
    #endregion
}
