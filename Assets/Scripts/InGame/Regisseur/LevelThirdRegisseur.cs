using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public struct LevelThreeState
{
    public int RegisterPCValue;
    public int RegisterInstrValue;

    public int FirstMemoryValue;
    public int SecondMemoryValue;
    public int ThirdMemoryValue;
    public int FourthMemoryValue;

    public bool RegisterPcwe;
    public bool RegisterInstrWe;
    public bool IntrDataMemoryWe;

    public int CurrentChoosenMuxPath; // since we can call ResetVisualization and, based on the selected path, call one of the Visualizer methods
    public int AluOperation;
}

public class LevelThirdRegisseur : BaseLevelRegisseur<LevelThreeState>
{
    [FormerlySerializedAs("_multiplexerVisualizer")]
    [Header("Level 3 Specific Components")]
    [SerializeField] protected MultiplexerVisualizer multiplexerVisualizer;
    [FormerlySerializedAs("_registerSrcAVisualizer")] [SerializeField] protected RegisterVisualizer registerSrcAVisualizer;
    [FormerlySerializedAs("_registerSrcBVisualizer")] [SerializeField] protected RegisterVisualizer registerSrcBVisualizer;
    [FormerlySerializedAs("_registerOutputVisualizer")] [SerializeField] protected InstructionDataMemoryVisualizer registerOutputVisualizer;
    [FormerlySerializedAs("_aluVizualizer")] [SerializeField] protected AluVisualiser aluVizualizer;

    [SerializeField] protected int srcAValue = 5;
    [SerializeField] protected int srcBValue = 7;

    [FormerlySerializedAs("_numberBlinker")] [SerializeField] protected Blinker numberBlinker;

    #region CACHED UI REFERENCES
    protected InfoPanelUI InfoSrcARegister;
    protected InfoPanelUI InfoSrcBRegister;
    protected InstrMemoryControlPanel InfoDataMemory; // ?
    #endregion

    // Intern components for computations
    protected Register SrcA;
    protected Register SrcB;
    protected DataInstMemory DataIntructionMemory;

    // protected override int RightAnswerValue => 66;

   
    protected int CurrentBus; // [0, 5]

    protected override void OnLevelStart()
    {
        // Initialization of logical components
        SrcA = new Register(srcAValue)
        {
            WriteEnable = true
        };
        SrcB = new Register(srcBValue)
        {
            WriteEnable = true
        };
        DataIntructionMemory = new DataInstMemory
        {
            MemoryWrite = true
        };
        DataIntructionMemory.LoadWord(0, 256);
        DataIntructionMemory.LoadWord(4, 128);
        DataIntructionMemory.LoadWord(8, -89);
        DataIntructionMemory.LoadWord(12, 66);

        // Caching of UI panels for visualizers
        InfoSrcARegister = registerSrcAVisualizer.UIRegisterPanel;
        InfoSrcBRegister = registerSrcBVisualizer.UIRegisterPanel;
        InfoDataMemory = registerOutputVisualizer.UIRegisterPanel;
       

        UpdateVisualizers();
    }

    protected override void ApplyState(LevelThreeState s)
    {
        SrcA = new Register(s.RegisterPCValue);
        SrcB = new Register(s.RegisterInstrValue);
        DataIntructionMemory = new DataInstMemory
        {
            Memory =
            {
                [0] = s.FirstMemoryValue,
                [4] = s.SecondMemoryValue,
                [8] = s.ThirdMemoryValue,
                [12] = s.FourthMemoryValue
            }
        };

        SrcA.WriteEnable = s.RegisterPcwe;
        SrcB.WriteEnable = s.RegisterInstrWe;
        DataIntructionMemory.MemoryWrite = s.IntrDataMemoryWe;

        ApplyMuxState(s.CurrentChoosenMuxPath, multiplexerVisualizer);
    }

    protected override void BlinkClockedComponents()
    {
        registerSrcAVisualizer.TriggerBlink();
        registerSrcBVisualizer.TriggerBlink();
        registerOutputVisualizer.TriggerBlink();
        numberBlinker.Trigger();
    }

    protected override bool CheckWinCondition()
    {
        return (SrcB.Output == RightAnswerValue);
    }

    protected override LevelThreeState GetCurrentState()
    {
        return new LevelThreeState
        {
            RegisterPCValue = SrcA.Output,
            RegisterInstrValue = SrcB.Output,

            FirstMemoryValue = DataIntructionMemory.Memory[0],
            SecondMemoryValue = DataIntructionMemory.Memory[4],
            ThirdMemoryValue = DataIntructionMemory.Memory[8],
            FourthMemoryValue = DataIntructionMemory.Memory[12],

            RegisterPcwe = SrcA.WriteEnable,
            RegisterInstrWe = SrcB.WriteEnable,
            IntrDataMemoryWe = DataIntructionMemory.MemoryWrite,

            CurrentChoosenMuxPath = multiplexerVisualizer.CurrentChosenMuxPath,
            AluOperation = aluVizualizer.CurrentAluOperation,
        };
    }

    protected override void HandleClockUpdate()
    {
        var path = multiplexerVisualizer.CurrentChosenMuxPath;
        int[] inputs = { SrcA.Output, SrcB.Output };

        if (path == -1)
        {
            Debug.LogError("Multiplexer path not selected (-1). Data will be lost.");
        }
        else if (path is >= 0 and <= 1)
        {
            Multiplexer.SelectNto1(inputs, path);
        }
        else
        {
            Debug.LogError($"Multiplexer path {path} is an invalid value!");
        }

        // sinchronyse vizualisers and concrete objects
        SrcA.WriteEnable = registerSrcAVisualizer.isWriteEnabled;
        SrcB.WriteEnable = registerSrcBVisualizer.isWriteEnabled;
        DataIntructionMemory.MemoryWrite = registerOutputVisualizer.isWriteEnabled;

        // implementation
        SrcB.Input = DataIntructionMemory.Memory.GetValueOrDefault(SrcA.Output, 0);
        

        var p = multiplexerVisualizer.CurrentChosenMuxPath;
        switch (p)
        {
            case -1:
                Debug.LogError("MUX path is -1. No value will be propagated");
                SrcA.Input = 0;
                break;
            case 0:
                SrcA.Input = Alu.Calculate(SrcA.Output, 4, aluVizualizer.CurrentAluOperation);
                break;
            case 1:
                SrcA.Input = Alu.Calculate(SrcB.Output, 4, aluVizualizer.CurrentAluOperation);
                break;
            default:
                Debug.LogError($"MUX path is incorrect! Expected [-1, 1] but got {p}");
                SrcA.Input = 0;
                break;
        }
        
        

        SrcA.PreClockUpdate();
        SrcB.PreClockUpdate();
        DataIntructionMemory.PreClockUpdate();


        // Only if WriteEnable = true, call Clock
        SrcA.Clock();
        SrcB.Clock();
        DataIntructionMemory.Clock();
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

                (s.CurrentChosenMuxPath == _multiplexerVisualizer.CurrentChosenMuxPath) &&
                (s.RegisterPCWE == srcA.WriteEnable) &&
                (s.RegisterInstrWE == srcB.WriteEnable) &&
                (s.IntrDataMemoryWE == dataIntructionMemory.MemoryWrite) &&
                (s.ALUOperation == _aluVizualizer.CurrentALUOperation);
    }*/

    protected override IEnumerator ReverseBusVisualizations()
    {
        if (CurrentBus >= 1 && CurrentBus <= maxTickNumber)
        {
            busController.StartBusSignal(busController.busSegments[5], SrcA.Input, true);


                var upperBusSignal = 0;
                if (multiplexerVisualizer.CurrentChosenMuxPath == 0) {
                    upperBusSignal = TickStateValues[TickCounter].RegisterPCValue;
                }
                else if (multiplexerVisualizer.CurrentChosenMuxPath == 1) {
                    upperBusSignal = TickStateValues[TickCounter].RegisterInstrValue;
                }
                yield return StartCoroutine(DelayedSignals(busController.busSegments[3], upperBusSignal, busController.busSegments[4], 4, true, true));

                yield return StartCoroutine(DelayedSignals(busController.busSegments[6], TickStateValues[TickCounter].RegisterPCValue, busController.busSegments[2], TickStateValues[TickCounter].RegisterInstrValue, true, true));
            

            yield return StartCoroutine(DelayedSignal(busController.busSegments[1], SrcB.Input, true));


                yield return StartCoroutine(DelayedSignal(busController.busSegments[0], TickStateValues[TickCounter].RegisterPCValue, true));
            
            

            CurrentBus--;
        }

        yield return new WaitUntil(() => busController.NoActiveSignals);
    }

    protected override IEnumerator RunBusVisualizations()
    {
        if (CurrentBus >= 0 && CurrentBus < maxTickNumber)
        {
            busController.StartBusSignal(busController.busSegments[0], SrcA.Output);
            busController.StartBusSignal(busController.busSegments[6], SrcA.Output);

            // should be by a short divisor
            if (DataIntructionMemory.Memory.TryGetValue(SrcA.Output, out var value))
            {
                yield return StartCoroutine(DelayedSignal(busController.busSegments[1], value));
            }
            else {
                yield return StartCoroutine(DelayedSignal(busController.busSegments[1], 0));
            }


            // should follow the first one with a short division
            yield return StartCoroutine(DelayedSignal(busController.busSegments[2], SrcB.Output));

            var propagationVal = 0;
            if (multiplexerVisualizer.CurrentChosenMuxPath == -1)
            {
                yield return StartCoroutine(DelayedSignal(busController.busSegments[4], 0));
            }
            else {
                
                if (multiplexerVisualizer.CurrentChosenMuxPath == 0)
                {
                    propagationVal = SrcA.Output;
                }
                else if (multiplexerVisualizer.CurrentChosenMuxPath == 1)
                {
                    propagationVal = SrcB.Output;
                }
                else
                {
                    Debug.LogError($"Unexpected MUX path {multiplexerVisualizer.CurrentChosenMuxPath}");
                }

                yield return StartCoroutine(DelayedSignals(busController.busSegments[3], propagationVal, busController.busSegments[4], 4));
            }


            // from ALU to first register
            yield return StartCoroutine(DelayedSignal(busController.busSegments[5], Alu.Calculate(propagationVal, 4, aluVizualizer.CurrentAluOperation)));

            CurrentBus++;
        }

        yield return new WaitUntil(() => busController.NoActiveSignals);
    }

    protected override void UpdateVisualizers()
    {
        InfoSrcARegister.Display("Register 1", $"{SrcA.Output}");
        InfoSrcBRegister.Display("Register 2", $"{SrcB.Output}");
        registerOutputVisualizer.UIRegisterPanel.Display($"{DataIntructionMemory.Memory[0]}", $"{DataIntructionMemory.Memory[4]}", $"{DataIntructionMemory.Memory[8]}", $"{DataIntructionMemory.Memory[12]}");

        registerSrcAVisualizer.ForceUpdateWriteEnableVisualization(SrcA.WriteEnable);
        registerSrcBVisualizer.ForceUpdateWriteEnableVisualization(SrcB.WriteEnable);
        registerOutputVisualizer.ForceUpdateWriteEnableVisualization(DataIntructionMemory.MemoryWrite);
    }

    #region
    protected override void BlockInGameInteractable()
    {
        registerSrcAVisualizer.UIRegisterPanel.WeButton.interactable = false;
        registerSrcBVisualizer.UIRegisterPanel.WeButton.interactable = false;
        registerOutputVisualizer.UIRegisterPanel.WeButton.interactable = false;


        multiplexerVisualizer.UIController.FirstWayButton.interactable = false;
        multiplexerVisualizer.UIController.SecondWayButton.interactable = false;


        aluVizualizer.uiController.FirstOperationButton.interactable = false;
        aluVizualizer.uiController.SecondOperationButton.interactable = false;
        aluVizualizer.uiController.ThirdOperationButton.interactable = false;
        aluVizualizer.uiController.FourthOperationButton.interactable = false;
    }

    protected override void ReleaseInGameInteractable()
    {
        registerSrcAVisualizer.UIRegisterPanel.WeButton.interactable = true;
        registerSrcBVisualizer.UIRegisterPanel.WeButton.interactable = true;
        registerOutputVisualizer.UIRegisterPanel.WeButton.interactable = true;

        multiplexerVisualizer.UIController.FirstWayButton.interactable = true;
        multiplexerVisualizer.UIController.SecondWayButton.interactable = true;


        aluVizualizer.uiController.FirstOperationButton.interactable = true;
        aluVizualizer.uiController.SecondOperationButton.interactable = true;
        aluVizualizer.uiController.ThirdOperationButton.interactable = true;
        aluVizualizer.uiController.FourthOperationButton.interactable = true;
    }
    #endregion
}
