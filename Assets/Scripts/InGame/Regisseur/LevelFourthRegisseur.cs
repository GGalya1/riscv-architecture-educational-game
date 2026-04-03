using System.Collections;
using UnityEngine;

public class LevelFourthRegisseur : LevelThirdRegisseur
{
    protected override void OnLevelStart()
    {
        // Инициализация логических компонентов
        srcA = new Register(0); srcA.WriteEnable = true;
        srcB = new Register(8); srcB.WriteEnable = true;
        dataIntructionMemory = new DataInstMemory(); dataIntructionMemory.MemoryWrite = true;
        dataIntructionMemory.LoadWord(0, 0);
        dataIntructionMemory.LoadWord(4, 3);
        dataIntructionMemory.LoadWord(8, -256);
        dataIntructionMemory.LoadWord(12, -1024);

        // Кэширование UI-панелей визуализаторов
        _infoSrcARegister = _registerSrcAVisualizer.UIRegisterPanel;
        _infoSrcBRegister = _registerSrcBVisualizer.UIRegisterPanel;
        _infoDataMemory = _registerOutputVisualizer.UIRegisterPanel;


        if (_levelTargetDescription == null || _levelTargetDescription.Length == 0)
        {
            _levelTargetText.text = $"Ziel: \r\nSchreibe in Speicher zwei Werte an Adressen 0 und 8.\nWobei Wert an Adresse 0 muss kleiner sein als an Adresse 8.";
        }
        else
        {
            _levelTargetText.text = _levelTargetDescription;
        }

        UpdateVizualizers();
    }

    protected override bool CheckWinCondition()
    {
        return dataIntructionMemory._memory[0] < dataIntructionMemory._memory[_correctAnswer];
    }

    protected override void HandleClockUpdate() {
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
        else
        {
            srcB.Input = 0;
            // if(dataIntructionMemory.MemoryWrite)
            //  XXX
        }

        dataIntructionMemory.Adress = srcA.Output;
        dataIntructionMemory.WriteData = srcB.Output;


        // Debug.Log($"[0]: {dataIntructionMemory._memory[0]} \n[4]: {dataIntructionMemory._memory[4]} \n[8]: {dataIntructionMemory._memory[8]}\n[12]: {dataIntructionMemory._memory[12]}");

        int p = _multiplexerVisualizer.CurrentChoosenMuxPath;
        if (p == -1)
        {
            Debug.LogError("MUX path is -1. No value will be propagated");
            srcA.Input = 0;
        }
        else if (p == 0)
        {
            srcA.Input = ALU.calculate(srcA.Output, 4, _aluVizualizer.CurrentALUOperation);
        }
        else if (p == 1)
        {
            srcA.Input = ALU.calculate(srcB.Output, 4, _aluVizualizer.CurrentALUOperation);
        }
        else
        {
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

    protected override IEnumerator RunBusVisualizations() {
        if (_currentBus >= 0 && _currentBus < _maxTickNumber)
        {
            _busController.StartBusSignal(_busController.busSegments[0], srcA.Output);
            _busController.StartBusSignal(_busController.busSegments[6], srcA.Output);

            // должна с коротким делеем
            if (dataIntructionMemory._memory.ContainsKey(srcA.Output))
            {
                yield return StartCoroutine(DelayedBusSignal(_busController.busSegments[1], dataIntructionMemory._memory[srcA.Output]));
            }
            else
            {
                yield return StartCoroutine(DelayedBusSignal(_busController.busSegments[1], 0));
            }


            // должна после первого с коротким делеем
            yield return StartCoroutine(DelayedBusSignals(_busController.busSegments[2], _busController.busSegments[7], srcB.Output, srcB.Output));

            int propagationVal = 0;
            if (_multiplexerVisualizer.CurrentChoosenMuxPath == -1)
            {
                yield return StartCoroutine(DelayedBusSignal(_busController.busSegments[4], 0));
            }
            else
            {

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
    protected override IEnumerator ReverseBusVisualizations() {
        if (_currentBus >= 1 && _currentBus <= _maxTickNumber)
        {
            _busController.StartBusSignal(_busController.busSegments[5], srcA.Input, true);

            if (_tickStateValues[_tickCounter] is LevelThreeState s)
            {
                int upperBusSignal = 0;
                if (_multiplexerVisualizer.CurrentChoosenMuxPath == 0)
                {
                    upperBusSignal = s.RegisterPCValue;
                }
                else if (_multiplexerVisualizer.CurrentChoosenMuxPath == 1)
                {
                    upperBusSignal = s.RegisterInstrValue;
                }
                yield return StartCoroutine(DelayedBusSignals(_busController.busSegments[3], _busController.busSegments[4], upperBusSignal, 4, true, true));

                yield return StartCoroutine(DelayedBusSignals(_busController.busSegments[7], _busController.busSegments[2], s.RegisterInstrValue, s.RegisterInstrValue, true, true));

                yield return StartCoroutine(DelayedBusSignals(_busController.busSegments[6], _busController.busSegments[1], s.RegisterPCValue, srcB.Input, true, true));
            }

            // yield return StartCoroutine(DelayedBusSignal(_busController.busSegments[1], srcB.Input, true));

            if (_tickStateValues[_tickCounter] is LevelThreeState st)
            {
                yield return StartCoroutine(DelayedBusSignal(_busController.busSegments[0], st.RegisterPCValue, true));
            }


            _currentBus--;
        }

        yield return new WaitUntil(() => _busController.NoActiveSignals);
    }


}
