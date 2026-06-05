using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

public struct ExtendedFirstLevelState
{
    public int RegisterOutputValue;

    public int MuXup;
    public int MuXmiddle;
    public int MuXdown;
    public int MuXoutput;
}

public class LevelOneExtended : BaseLevelRegisseur<ExtendedFirstLevelState>
{
    [FormerlySerializedAs("_registerOutputVisualizer")]
    [Header("MUXes specific components")]
    [SerializeField] private RegisterVisualizer registerOutputVisualizer;
    [FormerlySerializedAs("_upperMUXVisualizer")] [SerializeField] private MultiplexerVisualizer upperMuxVisualizer;
    [FormerlySerializedAs("_middleMUXVisualizer")] [SerializeField] private MultiplexerVisualizer middleMuxVisualizer;
    [FormerlySerializedAs("_downMUXVisualizer")] [SerializeField] private MultiplexerVisualizer downMuxVisualizer;
    [FormerlySerializedAs("_outputMUXVisualizer")] [SerializeField] private MultiplexerVisualizer outputMuxVisualizer;

    [FormerlySerializedAs("_numberBlinkers")] [SerializeField] private Blinker[] numberBlinkers;

    protected override int RightAnswerValue => 12;

    private Register _output;
    private InfoPanelUI _infoOutputRegister;

    protected int CurrentBus; // [0, 2]

    protected override void OnLevelStart()
    {
        _infoOutputRegister = registerOutputVisualizer.UIRegisterPanel;
        _output = new Register(11)
        {
            WriteEnable = true
        };

        UpdateVisualizers();
    }

    protected override void ApplyState(ExtendedFirstLevelState s) // "s" for state
    {
        _output = new Register(s.RegisterOutputValue);
        
        ApplyMuxState(s.MuXup, upperMuxVisualizer);
        ApplyMuxState(s.MuXmiddle, middleMuxVisualizer);
        ApplyMuxState(s.MuXdown, downMuxVisualizer);
        ApplyMuxState(s.MuXoutput, outputMuxVisualizer);
    }

    protected override void BlinkClockedComponents()
    {
        foreach (var b in numberBlinkers) {
            b.Trigger();
        }
    }

    protected override bool CheckWinCondition()
    {
        return _output.Output == RightAnswerValue;
    }

    protected override ExtendedFirstLevelState GetCurrentState()
    {
        return new ExtendedFirstLevelState
        {
            RegisterOutputValue = _output.Output,

            MuXup = upperMuxVisualizer.CurrentChosenMuxPath,
            MuXmiddle = middleMuxVisualizer.CurrentChosenMuxPath,
            MuXdown = downMuxVisualizer.CurrentChosenMuxPath,
            MuXoutput = outputMuxVisualizer.CurrentChosenMuxPath,
        };
    }

    protected override void HandleClockUpdate()
    {
        var up = EvaluateMux(upperMuxVisualizer.CurrentChosenMuxPath, -4, 0, -1);
        var left = EvaluateMux(middleMuxVisualizer.CurrentChosenMuxPath, 8, 12, -1);
        var down = EvaluateMux(downMuxVisualizer.CurrentChosenMuxPath, left, -8, -12);

        _output.Input = EvaluateMux(outputMuxVisualizer.CurrentChosenMuxPath, up, 4, down);

        _output.PreClockUpdate();
        _output.Clock();
    }

    protected override IEnumerator ReverseBusVisualizations()
    {
        if (CurrentBus >= 1 && CurrentBus <= maxTickNumber)
        {
            var up = upperMuxVisualizer.CurrentChosenMuxPath == 0 ? -4 : 0;
            var left = middleMuxVisualizer.CurrentChosenMuxPath == 0 ? 8 : 12;
            var down = EvaluateMux(downMuxVisualizer.CurrentChosenMuxPath, left, -8, -12);

            busController.StartBusSignal(busController.busSegments[10], _output.Input, true);
            yield return new WaitUntil(() => busController.NoActiveSignals);

            busController.StartBusSignal(busController.busSegments[7], up, true);
            busController.StartBusSignal(busController.busSegments[8], down, true);
            busController.StartBusSignal(busController.busSegments[9], 4, true);
            yield return new WaitUntil(() => busController.NoActiveSignals);

            busController.StartBusSignal(busController.busSegments[2], left, true);
            busController.StartBusSignal(busController.busSegments[3], -8, true);
            busController.StartBusSignal(busController.busSegments[4], -12, true);

            busController.StartBusSignal(busController.busSegments[5], -4, true);
            busController.StartBusSignal(busController.busSegments[6], 0, true);
            yield return new WaitUntil(() => busController.NoActiveSignals);

            busController.StartBusSignal(busController.busSegments[0], 8, true);
            busController.StartBusSignal(busController.busSegments[1], 12, true);

            CurrentBus--;
        }

        yield return new WaitUntil(() => busController.NoActiveSignals);
    }

    protected override IEnumerator RunBusVisualizations()
    {
        if (CurrentBus >= 0 && CurrentBus < maxTickNumber)
        {
            var up = EvaluateMux(upperMuxVisualizer.CurrentChosenMuxPath, -4, 0, -1);
            var left = EvaluateMux(middleMuxVisualizer.CurrentChosenMuxPath, 8, 12, -1);
            var down = EvaluateMux(downMuxVisualizer.CurrentChosenMuxPath, left, -8, -12);
            var output = EvaluateMux(outputMuxVisualizer.CurrentChosenMuxPath, up, 4, down);

            busController.StartBusSignal(busController.busSegments[0], 8);
            busController.StartBusSignal(busController.busSegments[1], 12);

            yield return new WaitUntil(() => busController.NoActiveSignals);

            busController.StartBusSignal(busController.busSegments[2], left);
            busController.StartBusSignal(busController.busSegments[3], -8);
            busController.StartBusSignal(busController.busSegments[4], -12);

            busController.StartBusSignal(busController.busSegments[5], -4);
            busController.StartBusSignal(busController.busSegments[6], 0);

            yield return new WaitUntil(() => busController.NoActiveSignals);

            busController.StartBusSignal(busController.busSegments[7], up);
            busController.StartBusSignal(busController.busSegments[8], down);
            busController.StartBusSignal(busController.busSegments[9], 4);

            yield return new WaitUntil(() => busController.NoActiveSignals);

            busController.StartBusSignal(busController.busSegments[10], output);

            CurrentBus++;
        }

        yield return new WaitUntil(() => busController.NoActiveSignals);
    }

    protected override void UpdateVisualizers()
    {
        _infoOutputRegister.Display("Register 1", $"{_output.Output}");
    }
}
