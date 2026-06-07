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

    public int CurrentChosenMuxPath; // since we can call ResetVisualization and, based on the selected path, call one of the Visualizer methods
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
    [FormerlySerializedAs("aluVizualizer")] [FormerlySerializedAs("_aluVizualizer")] [SerializeField] protected AluVisualiser aluVisualizer;

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
    protected DataInstMemory DataInstructionMemory;

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
        DataInstructionMemory = new DataInstMemory
        {
            MemoryWrite = true
        };
        DataInstructionMemory.LoadWord(0, 256);
        DataInstructionMemory.LoadWord(4, 128);
        DataInstructionMemory.LoadWord(8, -89);
        DataInstructionMemory.LoadWord(12, 66);

        // Caching of UI panels for visualizers
        InfoSrcARegister = registerSrcAVisualizer.UIRegisterPanel;
        InfoSrcBRegister = registerSrcBVisualizer.UIRegisterPanel;
        InfoDataMemory = registerOutputVisualizer.UIRegisterPanel;
       

        UpdateVisualizers();
    }

    protected override void ApplyState(LevelThreeState s)
    {
        SrcA.Reset(s.RegisterPCValue);
        SrcB.Reset(s.RegisterInstrValue);
        DataInstructionMemory = new DataInstMemory
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
        DataInstructionMemory.MemoryWrite = s.IntrDataMemoryWe;

        ApplyMuxState(s.CurrentChosenMuxPath, multiplexerVisualizer);
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

            FirstMemoryValue = DataInstructionMemory.Memory[0],
            SecondMemoryValue = DataInstructionMemory.Memory[4],
            ThirdMemoryValue = DataInstructionMemory.Memory[8],
            FourthMemoryValue = DataInstructionMemory.Memory[12],

            RegisterPcwe = SrcA.WriteEnable,
            RegisterInstrWe = SrcB.WriteEnable,
            IntrDataMemoryWe = DataInstructionMemory.MemoryWrite,

            CurrentChosenMuxPath = multiplexerVisualizer.CurrentChosenMuxPath,
            AluOperation = aluVisualizer.CurrentAluOperation,
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

        // synchronize visualizers and concrete objects
        SrcA.WriteEnable = registerSrcAVisualizer.isWriteEnabled;
        SrcB.WriteEnable = registerSrcBVisualizer.isWriteEnabled;
        DataInstructionMemory.MemoryWrite = registerOutputVisualizer.isWriteEnabled;

        // implementation
        SrcB.Input = DataInstructionMemory.Memory.GetValueOrDefault(SrcA.Output, 0);
        

        var p = multiplexerVisualizer.CurrentChosenMuxPath;
        switch (p)
        {
            case -1:
                Debug.LogError("MUX path is -1. No value will be propagated");
                SrcA.Input = 0;
                break;
            case 0:
                SrcA.Input = Alu.Calculate(SrcA.Output, 4, aluVisualizer.CurrentAluOperation);
                break;
            case 1:
                SrcA.Input = Alu.Calculate(SrcB.Output, 4, aluVisualizer.CurrentAluOperation);
                break;
            default:
                Debug.LogError($"MUX path is incorrect! Expected [-1, 1] but got {p}");
                SrcA.Input = 0;
                break;
        }
        
        

        SrcA.PreClockUpdate();
        SrcB.PreClockUpdate();
        DataInstructionMemory.PreClockUpdate();


        // Only if WriteEnable = true, call Clock
        SrcA.Clock();
        SrcB.Clock();
        DataInstructionMemory.Clock();
    }

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
            if (DataInstructionMemory.Memory.TryGetValue(SrcA.Output, out var value))
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
            yield return StartCoroutine(DelayedSignal(busController.busSegments[5], Alu.Calculate(propagationVal, 4, aluVisualizer.CurrentAluOperation)));

            CurrentBus++;
        }

        yield return new WaitUntil(() => busController.NoActiveSignals);
    }

    protected override void UpdateVisualizers()
    {
        InfoSrcARegister.Display("Register 1", $"{SrcA.Output}");
        InfoSrcBRegister.Display("Register 2", $"{SrcB.Output}");
        registerOutputVisualizer.UIRegisterPanel.Display($"{DataInstructionMemory.Memory[0]}", $"{DataInstructionMemory.Memory[4]}", $"{DataInstructionMemory.Memory[8]}", $"{DataInstructionMemory.Memory[12]}");

        registerSrcAVisualizer.ForceUpdateWriteEnableVisualization(SrcA.WriteEnable);
        registerSrcBVisualizer.ForceUpdateWriteEnableVisualization(SrcB.WriteEnable);
        registerOutputVisualizer.ForceUpdateWriteEnableVisualization(DataInstructionMemory.MemoryWrite);
    }
}
