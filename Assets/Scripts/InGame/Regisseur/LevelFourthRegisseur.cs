using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelFourthRegisseur : LevelThirdRegisseur
{
    protected override void OnLevelStart()
    {
        // Initialization of logical components
        SrcA = new Register()
        {
            WriteEnable = true
        };
        SrcB = new Register(8)
        {
            WriteEnable = true
        };
        DataIntructionMemory = new DataInstMemory
        {
            MemoryWrite = true
        };
        DataIntructionMemory.LoadWord(0, 0);
        DataIntructionMemory.LoadWord(4, 3);
        DataIntructionMemory.LoadWord(8, -256);
        DataIntructionMemory.LoadWord(12, -1024);

        // Caching of UI panels for visualizers
        InfoSrcARegister = registerSrcAVisualizer.UIRegisterPanel;
        InfoSrcBRegister = registerSrcBVisualizer.UIRegisterPanel;
        InfoDataMemory = registerOutputVisualizer.UIRegisterPanel;
        

        UpdateVisualizers();
    }

    protected override bool CheckWinCondition()
    {
        return DataIntructionMemory.Memory[0] < DataIntructionMemory.Memory[correctAnswer];
    }

    protected override void HandleClockUpdate() {
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

        // synchronize visualizer and concrete objects
        SrcA.WriteEnable = registerSrcAVisualizer.isWriteEnabled;
        SrcB.WriteEnable = registerSrcBVisualizer.isWriteEnabled;
        DataIntructionMemory.MemoryWrite = registerOutputVisualizer.isWriteEnabled;

        // implementation
        SrcB.Input = DataIntructionMemory.Memory.GetValueOrDefault(SrcA.Output, 0);

        DataIntructionMemory.Address = SrcA.Output;
        DataIntructionMemory.WriteData = SrcB.Output;
        
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
            if (DataIntructionMemory.Memory.TryGetValue(SrcA.Output, out var value))
            {
                yield return StartCoroutine(DelayedSignal(busController.busSegments[1], value));
            }
            else
            {
                yield return StartCoroutine(DelayedSignal(busController.busSegments[1], 0));
            }


            // should follow the first one with a short delay
            yield return StartCoroutine(DelayedSignals(busController.busSegments[2], SrcB.Output, busController.busSegments[7], SrcB.Output));

            var propagationVal = 0;
            if (multiplexerVisualizer.CurrentChosenMuxPath == -1)
            {
                yield return StartCoroutine(DelayedSignal(busController.busSegments[4], 0));
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

                yield return StartCoroutine(DelayedSignals(busController.busSegments[3], propagationVal, busController.busSegments[4], 4));
            }


            // from ALU to first register
            yield return StartCoroutine(DelayedSignal(busController.busSegments[5], Alu.Calculate(propagationVal, 4, aluVizualizer.CurrentAluOperation)));

            CurrentBus++;
        }

        yield return new WaitUntil(() => busController.NoActiveSignals);
    }
    protected override IEnumerator ReverseBusVisualizations() {
        if (CurrentBus >= 1 && CurrentBus <= maxTickNumber)
        {
            busController.StartBusSignal(busController.busSegments[5], SrcA.Input, true);


                var upperBusSignal = 0;
                if (multiplexerVisualizer.CurrentChosenMuxPath == 0)
                {
                    upperBusSignal = TickStateValues[TickCounter].RegisterPCValue;
                }
                else if (multiplexerVisualizer.CurrentChosenMuxPath == 1)
                {
                    upperBusSignal = TickStateValues[TickCounter].RegisterInstrValue;
                }
                yield return StartCoroutine(DelayedSignals(busController.busSegments[3], upperBusSignal, busController.busSegments[4], 4, true, true));

                yield return StartCoroutine(DelayedSignals(busController.busSegments[7], TickStateValues[TickCounter].RegisterInstrValue, busController.busSegments[2], TickStateValues[TickCounter].RegisterInstrValue, true, true));

                yield return StartCoroutine(DelayedSignals(busController.busSegments[6], TickStateValues[TickCounter].RegisterPCValue, busController.busSegments[1], SrcB.Input, true, true));



                yield return StartCoroutine(DelayedSignal(busController.busSegments[0], TickStateValues[TickCounter].RegisterPCValue, true));


            CurrentBus--;
        }

        yield return new WaitUntil(() => busController.NoActiveSignals);
    }


}
