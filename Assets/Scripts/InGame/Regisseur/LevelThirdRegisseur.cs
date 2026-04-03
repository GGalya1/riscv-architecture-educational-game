using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public struct LevelThreeState
{
    public int RegisterPCValue;
    public int RegisterInstrValue;

    public int firstMemoryValue;
    public int secondMemoryValue;
    public int thirdMemoryValue;
    public int fourthMemoryValue;

    public bool RegisterPCWE;
    public bool RegisterInstrWE;
    public bool IntrDataMemoryWE;

    public int CurrentChoosenMuxPath; // т.к. можем вызвать ResetVisualization и, основываясь  на выбранном пути, вызвать один из методов Vizualizer
    public int ALUOperation;
}

public class LevelThirdRegisseur : BaseLevelRegisseur
{
    [Header("Level 3 Specific Components")]
    [SerializeField] protected MuiltiplexerVizualizer _multiplexerVisualizer;
    [SerializeField] protected RegisterVizualizer _registerSrcAVisualizer;
    [SerializeField] protected RegisterVizualizer _registerSrcBVisualizer;
    [SerializeField] protected IntructionDataMemoryVizualizer _registerOutputVisualizer;
    [SerializeField] protected ALUVizualiser _aluVizualizer;

    [SerializeField] protected int srcAValue = 5;
    [SerializeField] protected int srcBValue = 7;

    [SerializeField] protected Blinker _numberBlinker;

    #region CACHED UI REFERENCES
    protected InfoPanelUI _infoSrcARegister;
    protected InfoPanelUI _infoSrcBRegister;
    protected InstrMemoryControlPanel _infoDataMemory; // ?
    #endregion

    // Intern components for computations
    protected Register srcA;
    protected Register srcB;
    protected DataInstMemory dataIntructionMemory;

    // protected override int RightAnswerValue => 66;

   
    protected int _currentBus = 0; // [0, 5]

    protected override void OnLevelStart()
    {
        // Инициализация логических компонентов
        srcA = new Register(srcAValue); srcA.WriteEnable = true;
        srcB = new Register(srcBValue); srcB.WriteEnable = true;
        dataIntructionMemory = new DataInstMemory(); dataIntructionMemory.MemoryWrite = true;
        dataIntructionMemory.LoadWord(0, 256);
        dataIntructionMemory.LoadWord(4, 128);
        dataIntructionMemory.LoadWord(8, -89);
        dataIntructionMemory.LoadWord(12, 66);

        // Кэширование UI-панелей визуализаторов
        _infoSrcARegister = _registerSrcAVisualizer.UIRegisterPanel;
        _infoSrcBRegister = _registerSrcBVisualizer.UIRegisterPanel;
        _infoDataMemory = _registerOutputVisualizer.UIRegisterPanel;


        if (_levelTargetDescription == null || _levelTargetDescription.Length == 0)
        {
            _levelTargetText.text = $"Ziel: \r\nSchreibe in Register 2 den Wert {RightAnswerValue}";
        }
        else
        {
            _levelTargetText.text = _levelTargetDescription;
        }
        

        UpdateVizualizers();
    }

    protected override void ApplyState(object state)
    {
        LevelThreeState s = (LevelThreeState)state;

        srcA = new Register(s.RegisterPCValue);
        srcB = new Register(s.RegisterInstrValue);
        dataIntructionMemory = new DataInstMemory();
        dataIntructionMemory._memory[0] = s.firstMemoryValue;
        dataIntructionMemory._memory[4] = s.secondMemoryValue;
        dataIntructionMemory._memory[8] = s.thirdMemoryValue;
        dataIntructionMemory._memory[12] = s.fourthMemoryValue;

        srcA.WriteEnable = s.RegisterPCWE;
        srcB.WriteEnable = s.RegisterInstrWE;
        dataIntructionMemory.MemoryWrite = s.IntrDataMemoryWE;

        int temp = s.CurrentChoosenMuxPath;
        if (temp == -1)
        {
            _multiplexerVisualizer.ResetVizualization();
        }
        else if (temp == 0)
        {
            _multiplexerVisualizer.SelectPath(0);
        }
        else if (temp == 1)
        {
            _multiplexerVisualizer.SelectPath(1);
        }
        else if (temp == 2)
        {
            _multiplexerVisualizer.SelectPath(2);
        }
        else
        {
            Debug.LogError($"Saved multiplexer value {temp} is not in [0, 2]");
        }
    }

    protected override void BlinkClockedComponents()
    {
        _registerSrcAVisualizer.TriggerBlink();
        _registerSrcBVisualizer.TriggerBlink();
        _registerOutputVisualizer.TriggerBlink();
        _numberBlinker.Trigger();
    }

    protected override bool CheckWinCondition()
    {
        return (srcB.Output == RightAnswerValue);
    }

    protected override object GetCurrentState()
    {
        return new LevelThreeState
        {
            RegisterPCValue = srcA.Output,
            RegisterInstrValue = srcB.Output,

            firstMemoryValue = dataIntructionMemory._memory[0],
            secondMemoryValue = dataIntructionMemory._memory[4],
            thirdMemoryValue = dataIntructionMemory._memory[8],
            fourthMemoryValue = dataIntructionMemory._memory[12],

            RegisterPCWE = srcA.WriteEnable,
            RegisterInstrWE = srcB.WriteEnable,
            IntrDataMemoryWE = dataIntructionMemory.MemoryWrite,

            CurrentChoosenMuxPath = _multiplexerVisualizer.CurrentChoosenMuxPath,
            ALUOperation = _aluVizualizer.CurrentALUOperation,
        };
    }

    protected override void HandleClockUpdate()
    {
        int path = _multiplexerVisualizer.CurrentChoosenMuxPath;
        int[] inputs = { srcA.Output, srcB.Output };
        int res = 0;

        if (path == -1)
        {
            Debug.LogError("Multiplexer path not selected (-1). Data will be lost.");
        }
        else if (path >= 0 && path <= 1)
        {
            res = Multiplexer.SelectNto1(inputs, path);
        }
        else
        {
            Debug.LogError($"Multiplexer path {path} is an invalid value!");
        }

        // sinchronyse vizualisers and concrete objects
        srcA.WriteEnable = _registerSrcAVisualizer.isWriteEnabled;
        srcB.WriteEnable = _registerSrcBVisualizer.isWriteEnabled;
        dataIntructionMemory.MemoryWrite = _registerOutputVisualizer.isWriteEnabled;

        // implementation
        if (dataIntructionMemory._memory.ContainsKey(srcA.Output))
        {
            srcB.Input = dataIntructionMemory._memory[srcA.Output];
        }
        else {
            srcB.Input = 0;
        }
        

        int p = _multiplexerVisualizer.CurrentChoosenMuxPath;
        if (p == -1) {
            Debug.LogError("MUX path is -1. No value will be propagated");
            srcA.Input = 0;
        }
        else if (p == 0) {
            srcA.Input = ALU.calculate(srcA.Output, 4, _aluVizualizer.CurrentALUOperation);
        }
        else if (p == 1) {
            srcA.Input = ALU.calculate(srcB.Output, 4, _aluVizualizer.CurrentALUOperation);
        }
        else {
            Debug.LogError($"MUX path is incorrect! Expected [-1, 1] but got {p}");
            srcA.Input = 0;
        }
        
        

        srcA.PreClockUpdate();
        srcB.PreClockUpdate();
        dataIntructionMemory.PreClockUpdate();


        // Only if WriteEnable = true, call Clock
        srcA.Clock();
        srcB.Clock();
        dataIntructionMemory.Clock();
    }

    /*protected override bool IsStateEqual(object state)
    {
        if (!(state is LevelThreeState s)) return false;

        return (s.RegisterPCValue == srcA.Output) &&
                (s.RegisterInstrValue == srcB.Output) &&

                (s.firstMemoryValue == dataIntructionMemory._memory[0]) &&
                (s.secondMemoryValue == dataIntructionMemory._memory[4]) &&
                (s.thirdMemoryValue == dataIntructionMemory._memory[8]) &&
                (s.fourthMemoryValue == dataIntructionMemory._memory[12]) &&

                (s.CurrentChoosenMuxPath == _multiplexerVisualizer.CurrentChoosenMuxPath) &&
                (s.RegisterPCWE == srcA.WriteEnable) &&
                (s.RegisterInstrWE == srcB.WriteEnable) &&
                (s.IntrDataMemoryWE == dataIntructionMemory.MemoryWrite) &&
                (s.ALUOperation == _aluVizualizer.CurrentALUOperation);
    }*/

    protected override IEnumerator ReverseBusVisualizations()
    {
        if (_currentBus >= 1 && _currentBus <= _maxTickNumber)
        {
            _busController.StartBusSignal(_busController.busSegments[5], srcA.Input, true);

            if (_tickStateValues[_tickCounter] is LevelThreeState s)
            {
                int upperBusSignal = 0;
                if (_multiplexerVisualizer.CurrentChoosenMuxPath == 0) {
                    upperBusSignal = s.RegisterPCValue;
                }
                else if (_multiplexerVisualizer.CurrentChoosenMuxPath == 1) {
                    upperBusSignal = s.RegisterInstrValue;
                }
                yield return StartCoroutine(DelayedBusSignals(_busController.busSegments[3], _busController.busSegments[4], upperBusSignal, 4, true, true));

                yield return StartCoroutine(DelayedBusSignals(_busController.busSegments[6], _busController.busSegments[2], s.RegisterPCValue, s.RegisterInstrValue, true, true));
            }

            yield return StartCoroutine(DelayedBusSignal(_busController.busSegments[1], srcB.Input, true));

            if (_tickStateValues[_tickCounter] is LevelThreeState st)
            {
                yield return StartCoroutine(DelayedBusSignal(_busController.busSegments[0], st.RegisterPCValue, true));
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
            _busController.StartBusSignal(_busController.busSegments[6], srcA.Output);

            // должна с коротким делеем
            if (dataIntructionMemory._memory.ContainsKey(srcA.Output))
            {
                yield return StartCoroutine(DelayedBusSignal(_busController.busSegments[1], dataIntructionMemory._memory[srcA.Output]));
            }
            else {
                yield return StartCoroutine(DelayedBusSignal(_busController.busSegments[1], 0));
            }


            // должна после первого с коротким делеем
            yield return StartCoroutine(DelayedBusSignal(_busController.busSegments[2], srcB.Output));

            int propagationVal = 0;
            if (_multiplexerVisualizer.CurrentChoosenMuxPath == -1)
            {
                yield return StartCoroutine(DelayedBusSignal(_busController.busSegments[4], 0));
            }
            else {
                
                if (_multiplexerVisualizer.CurrentChoosenMuxPath == 0)
                {
                    propagationVal = srcA.Output;
                }
                else if (_multiplexerVisualizer.CurrentChoosenMuxPath == 1)
                {
                    propagationVal = srcB.Output;
                }
                else
                {
                    Debug.LogError($"Unexpected MUX path {_multiplexerVisualizer.CurrentChoosenMuxPath}");
                }

                yield return StartCoroutine(DelayedBusSignals(_busController.busSegments[3], _busController.busSegments[4], propagationVal, 4));
            }


            // from ALU to first register
            yield return StartCoroutine(DelayedBusSignal(_busController.busSegments[5], ALU.calculate(propagationVal, 4, _aluVizualizer.CurrentALUOperation)));

            _currentBus++;
        }

        yield return new WaitUntil(() => _busController.NoActiveSignals);
    }



    protected IEnumerator DelayedBusSignal(LineRenderer busToStart, bool reverse=false)
    {
        yield return new WaitUntil(() => _busController.NoActiveSignals);

        // Запускаем третий сигнал
        _busController.StartBusSignal(busToStart, reverse);
    }
    protected IEnumerator DelayedBusSignal(LineRenderer busToStart, int value, bool reverse = false)
    {
        yield return new WaitUntil(() => _busController.NoActiveSignals);

        // Запускаем третий сигнал
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

    protected override void UpdateVizualizers()
    {
        _infoSrcARegister.Display("Register 1", $"{srcA.Output}");
        _infoSrcBRegister.Display("Register 2", $"{srcB.Output}");
        //_infoDataMemory.Display($"{dataIntructionMemory._memory[0]}", $"{dataIntructionMemory._memory[4]}", $"{dataIntructionMemory._memory[8]}", $"{dataIntructionMemory._memory[12]}");
        _registerOutputVisualizer.UIRegisterPanel.Display($"{dataIntructionMemory._memory[0]}", $"{dataIntructionMemory._memory[4]}", $"{dataIntructionMemory._memory[8]}", $"{dataIntructionMemory._memory[12]}");

        _registerSrcAVisualizer.ForceUpdateWriteEnableVisualization(srcA.WriteEnable);
        _registerSrcBVisualizer.ForceUpdateWriteEnableVisualization(srcB.WriteEnable);
        _registerOutputVisualizer.ForceUpdateWriteEnableVisualization(dataIntructionMemory.MemoryWrite);
    }

    #region
    protected override void BlockIngameInteractables()
    {
        _registerSrcAVisualizer.UIRegisterPanel.WEButton.interactable = false;
        _registerSrcBVisualizer.UIRegisterPanel.WEButton.interactable = false;
        _registerOutputVisualizer.UIRegisterPanel.WEButton.interactable = false;


        _multiplexerVisualizer.UIController.FirstWayButton.interactable = false;
        _multiplexerVisualizer.UIController.SecondWayButton.interactable = false;


        _aluVizualizer.UIController.FirstOperationButton.interactable = false;
        _aluVizualizer.UIController.SecondOperationButton.interactable = false;
        _aluVizualizer.UIController.ThirdOperationButton.interactable = false;
        _aluVizualizer.UIController.FourthOperationButton.interactable = false;
    }

    protected override void ReleaseIngameInteractables()
    {
        _registerSrcAVisualizer.UIRegisterPanel.WEButton.interactable = true;
        _registerSrcBVisualizer.UIRegisterPanel.WEButton.interactable = true;
        _registerOutputVisualizer.UIRegisterPanel.WEButton.interactable = true;

        _multiplexerVisualizer.UIController.FirstWayButton.interactable = true;
        _multiplexerVisualizer.UIController.SecondWayButton.interactable = true;


        _aluVizualizer.UIController.FirstOperationButton.interactable = true;
        _aluVizualizer.UIController.SecondOperationButton.interactable = true;
        _aluVizualizer.UIController.ThirdOperationButton.interactable = true;
        _aluVizualizer.UIController.FourthOperationButton.interactable = true;
    }
    #endregion
}
