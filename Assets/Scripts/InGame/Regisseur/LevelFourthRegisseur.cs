using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class LevelFourthBusSegments : LevelThirdBusSegments
{
    [Tooltip("SrcB output -> Data Memory write data (WD)")]
    public LineRenderer srcBToDataMemWd;

    public void RegisterAll(BusController c)
    {
        c.RegisterSegment(pcToMemAddr);
        c.RegisterSegment(pcToAdrMux);
        c.RegisterSegment(memDataToSrcB);
        c.RegisterSegment(srcBToExt);
        c.RegisterSegment(srcBToDataMemWd);
        c.RegisterSegment(muxToAdder);
        c.RegisterSegment(constFourToAdder);
        c.RegisterSegment(adderToSrcA);
    }
}

public class LevelFourthRegisseur : LevelThirdRegisseur
{
    [Header("Bus Segments (Level 4)")] [SerializeField]
    private LevelFourthBusSegments buses4;

    protected override void Start()
    {
        base.Start();
        buses4.RegisterAll(busController);
    }

    protected override void OnLevelStart()
    {
        // Initialization of logical components
        SrcA = new Register
        {
            WriteEnable = true
        };
        SrcB = new Register(8)
        {
            WriteEnable = true
        };
        DataInstructionMemory = new DataInstMemory
        {
            MemoryWrite = true
        };
        DataInstructionMemory.LoadWord(0, 0);
        DataInstructionMemory.LoadWord(4, 3);
        DataInstructionMemory.LoadWord(8, -256);
        DataInstructionMemory.LoadWord(12, -1024);

        // Caching of UI panels for visualizers
        InfoSrcARegister = registerSrcAVisualizer.UIRegisterPanel;
        InfoSrcBRegister = registerSrcBVisualizer.UIRegisterPanel;
        InfoDataMemory = registerOutputVisualizer.UIRegisterPanel;


        UpdateVisualizers();
    }

    protected override bool CheckWinCondition()
    {
        return DataInstructionMemory.Memory[0] < DataInstructionMemory.Memory[correctAnswer];
    }

    protected override void HandleClockUpdate()
    {
        // synchronize visualizer and concrete objects
        SrcA.WriteEnable = registerSrcAVisualizer.isWriteEnabled;
        SrcB.WriteEnable = registerSrcBVisualizer.isWriteEnabled;
        DataInstructionMemory.MemoryWrite = registerOutputVisualizer.isWriteEnabled;

        // implementation
        SrcB.Input = DataInstructionMemory.Memory.GetValueOrDefault(SrcA.Output, 0);

        DataInstructionMemory.Address = SrcA.Output;
        DataInstructionMemory.WriteData = SrcB.Output;

        var p = multiplexerVisualizer.CurrentChosenMuxPath;
        if (p == -1)
        {
            Debug.LogError("MUX path is -1. No value will be propagated");
            SrcA.Input = 0;
        }
        else if (p == 0)
        {
            SrcA.Input = Alu.Calculate(SrcA.Output, 4, aluVisualizer.CurrentAluOperation);
        }
        else if (p == 1)
        {
            SrcA.Input = Alu.Calculate(SrcB.Output, 4, aluVisualizer.CurrentAluOperation);
        }
        else
        {
            Debug.LogError($"MUX path is incorrect! Expected [-1, 1] but got {p}");
            SrcA.Input = 0;
        }


        SrcA.PreClockUpdate();
        SrcB.PreClockUpdate();
        DataInstructionMemory.PreClockUpdate();


        // Only if WriteEnable = true, call Clock
        SrcA.Clock();
        SrcB.Clock();
        DataInstructionMemory.Clock();
    }

    protected override IEnumerator RunBusVisualizations()
    {
        if (CurrentBus >= 0 && CurrentBus < maxTickNumber)
        {
            busController.StartBusSignal(buses.pcToMemAddr, SrcA.Output);
            busController.StartBusSignal(buses.pcToAdrMux, SrcA.Output);

            // should be after a short delay
            if (DataInstructionMemory.Memory.TryGetValue(SrcA.Output, out var value))
                yield return StartCoroutine(DelayedSignal(buses.memDataToSrcB, value));
            else
                yield return StartCoroutine(DelayedSignal(buses.memDataToSrcB, 0));


            // should follow the first one with a short delay
            yield return StartCoroutine(
                DelayedSignals(buses.srcBToExt, SrcB.Output, buses4.srcBToDataMemWd, SrcB.Output));

            var propagationVal = 0;
            if (multiplexerVisualizer.CurrentChosenMuxPath == -1)
            {
                yield return StartCoroutine(DelayedSignal(buses.constFourToAdder, 0));
            }
            else
            {
                if (multiplexerVisualizer.CurrentChosenMuxPath == 0)
                    propagationVal = SrcA.Output;
                else if (multiplexerVisualizer.CurrentChosenMuxPath == 1)
                    propagationVal = SrcB.Output;
                else
                    Debug.LogError($"Unexpected MUX path {multiplexerVisualizer.CurrentChosenMuxPath}");

                yield return StartCoroutine(DelayedSignals(buses.muxToAdder, propagationVal, buses.constFourToAdder,
                    4));
            }


            // from ALU to first register
            yield return StartCoroutine(DelayedSignal(buses.adderToSrcA,
                Alu.Calculate(propagationVal, 4, aluVisualizer.CurrentAluOperation)));

            CurrentBus++;
        }

        yield return new WaitUntil(() => busController.NoActiveSignals);
    }

    protected override IEnumerator ReverseBusVisualizations()
    {
        if (CurrentBus >= 1 && CurrentBus <= maxTickNumber)
        {
            busController.StartBusSignal(buses.adderToSrcA, SrcA.Input, true);


            var upperBusSignal = 0;
            if (multiplexerVisualizer.CurrentChosenMuxPath == 0)
                upperBusSignal = TickStateValues[TickCounter].RegisterPCValue;
            else if (multiplexerVisualizer.CurrentChosenMuxPath == 1)
                upperBusSignal = TickStateValues[TickCounter].RegisterInstrValue;
            yield return StartCoroutine(DelayedSignals(buses.muxToAdder, upperBusSignal, buses.constFourToAdder, 4,
                true, true));

            yield return StartCoroutine(DelayedSignals(buses4.srcBToDataMemWd,
                TickStateValues[TickCounter].RegisterInstrValue, buses.srcBToExt,
                TickStateValues[TickCounter].RegisterInstrValue, true, true));

            yield return StartCoroutine(DelayedSignals(buses.pcToAdrMux, TickStateValues[TickCounter].RegisterPCValue,
                buses.memDataToSrcB, SrcB.Input, true, true));


            yield return StartCoroutine(DelayedSignal(buses.pcToMemAddr, TickStateValues[TickCounter].RegisterPCValue,
                true));


            CurrentBus--;
        }

        yield return new WaitUntil(() => busController.NoActiveSignals);
    }
}