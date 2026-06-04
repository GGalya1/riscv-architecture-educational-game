using System.Collections;
using UnityEngine;

public class LevelFourthRegisseur : LevelThirdRegisseur
{
    protected override void OnLevelStart()
    {
        // Initialization of logical components
        SrcA = new Register(0); SrcA.WriteEnable = true;
        SrcB = new Register(8); SrcB.WriteEnable = true;
        DataIntructionMemory = new DataInstMemory(); DataIntructionMemory.MemoryWrite = true;
        DataIntructionMemory.LoadWord(0, 0);
        DataIntructionMemory.LoadWord(4, 3);
        DataIntructionMemory.LoadWord(8, -256);
        DataIntructionMemory.LoadWord(12, -1024);

        // Caching of UI panels for visualizers
        InfoSrcARegister = registerSrcAVisualizer.UIRegisterPanel;
        InfoSrcBRegister = registerSrcBVisualizer.UIRegisterPanel;
        InfoDataMemory = registerOutputVisualizer.UIRegisterPanel;


        SetLevelTargetText(levelTargetDescription);

        UpdateVizualizers();
    }

    protected override bool CheckWinCondition()
    {
        return DataIntructionMemory.Memory[0] < DataIntructionMemory.Memory[correctAnswer];
    }

    protected override void HandleClockUpdate() {
        var path = multiplexerVisualizer.CurrentChosenMuxPath;
        int[] inputs = { SrcA.Output, SrcB.Output };
        var res = 0;

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
        SrcA.WriteEnable = registerSrcAVisualizer.isWriteEnabled;
        SrcB.WriteEnable = registerSrcBVisualizer.isWriteEnabled;
        DataIntructionMemory.MemoryWrite = registerOutputVisualizer.isWriteEnabled;

        // implementation
        if (DataIntructionMemory.Memory.ContainsKey(SrcA.Output))
        {
            SrcB.Input = DataIntructionMemory.Memory[SrcA.Output];
        }
        else
        {
            SrcB.Input = 0;
            // if(dataIntructionMemory.MemoryWrite)
            //  XXX
        }

        DataIntructionMemory.Address = SrcA.Output;
        DataIntructionMemory.WriteData = SrcB.Output;


        // Debug.Log($"[0]: {dataIntructionMemory._memory[0]} \n[4]: {dataIntructionMemory._memory[4]} \n[8]: {dataIntructionMemory._memory[8]}\n[12]: {dataIntructionMemory._memory[12]}");

        var p = multiplexerVisualizer.CurrentChosenMuxPath;
        if (p == -1)
        {
            Debug.LogError("MUX path is -1. No value will be propagated");
            SrcA.Input = 0;
        }
        else if (p == 0)
        {
            SrcA.Input = Alu.Calculate(SrcA.Output, 4, aluVizualizer.CurrentAluOperation);
        }
        else if (p == 1)
        {
            SrcA.Input = Alu.Calculate(SrcB.Output, 4, aluVizualizer.CurrentAluOperation);
        }
        else
        {
            Debug.LogError($"MUX path is incorrect! Expected [-1, 1] but got {p}");
            SrcA.Input = 0;
        }



        SrcA.PreClockUpdate();
        SrcB.PreClockUpdate();
        DataIntructionMemory.PreClockUpdate();


        // Only if WriteEnable = true, call Clock
        SrcA.Clock();
        SrcB.Clock();
        DataIntructionMemory.Clock();
    }

    protected override IEnumerator RunBusVisualizations() {
        if (CurrentBus >= 0 && CurrentBus < maxTickNumber)
        {
            busController.StartBusSignal(busController.busSegments[0], SrcA.Output);
            busController.StartBusSignal(busController.busSegments[6], SrcA.Output);

            // should be after a short delay
            if (DataIntructionMemory.Memory.ContainsKey(SrcA.Output))
            {
                yield return StartCoroutine(DelayedBusSignal(busController.busSegments[1], DataIntructionMemory.Memory[SrcA.Output]));
            }
            else
            {
                yield return StartCoroutine(DelayedBusSignal(busController.busSegments[1], 0));
            }


            // should follow the first one with a short delay
            yield return StartCoroutine(DelayedBusSignals(busController.busSegments[2], busController.busSegments[7], SrcB.Output, SrcB.Output));

            var propagationVal = 0;
            if (multiplexerVisualizer.CurrentChosenMuxPath == -1)
            {
                yield return StartCoroutine(DelayedBusSignal(busController.busSegments[4], 0));
            }
            else
            {

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

                yield return StartCoroutine(DelayedBusSignals(busController.busSegments[3], busController.busSegments[4], propagationVal, 4));
            }


            // from ALU to first register
            yield return StartCoroutine(DelayedBusSignal(busController.busSegments[5], Alu.Calculate(propagationVal, 4, aluVizualizer.CurrentAluOperation)));

            CurrentBus++;
        }

        yield return new WaitUntil(() => busController.NoActiveSignals);
    }
    protected override IEnumerator ReverseBusVisualizations() {
        if (CurrentBus >= 1 && CurrentBus <= maxTickNumber)
        {
            busController.StartBusSignal(busController.busSegments[5], SrcA.Input, true);

            if (TickStateValues[TickCounter] is LevelThreeState s)
            {
                var upperBusSignal = 0;
                if (multiplexerVisualizer.CurrentChosenMuxPath == 0)
                {
                    upperBusSignal = s.RegisterPCValue;
                }
                else if (multiplexerVisualizer.CurrentChosenMuxPath == 1)
                {
                    upperBusSignal = s.RegisterInstrValue;
                }
                yield return StartCoroutine(DelayedBusSignals(busController.busSegments[3], busController.busSegments[4], upperBusSignal, 4, true, true));

                yield return StartCoroutine(DelayedBusSignals(busController.busSegments[7], busController.busSegments[2], s.RegisterInstrValue, s.RegisterInstrValue, true, true));

                yield return StartCoroutine(DelayedBusSignals(busController.busSegments[6], busController.busSegments[1], s.RegisterPCValue, SrcB.Input, true, true));
            }

            // yield return StartCoroutine(DelayedBusSignal(_busController.busSegments[1], srcB.Input, true));

            if (TickStateValues[TickCounter] is LevelThreeState st)
            {
                yield return StartCoroutine(DelayedBusSignal(busController.busSegments[0], st.RegisterPCValue, true));
            }


            CurrentBus--;
        }

        yield return new WaitUntil(() => busController.NoActiveSignals);
    }


}
