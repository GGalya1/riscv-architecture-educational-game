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

public class LevelOneExtended : BaseLevelRegisseur
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
        _output = new Register(11); _output.WriteEnable = true;

        UpdateVizualizers();
    }

    protected override void ApplyState(object state)
    {
        var s = (ExtendedFirstLevelState)state;

        _output = new Register(s.RegisterOutputValue);

        MuxVizualizerHelper(s.MuXup, upperMuxVisualizer);
        MuxVizualizerHelper(s.MuXmiddle, middleMuxVisualizer);
        MuxVizualizerHelper(s.MuXdown, downMuxVisualizer);
        MuxVizualizerHelper(s.MuXoutput, outputMuxVisualizer);
    }

    protected override void BlinkClockedComponents()
    {
        foreach (var b in numberBlinkers) {
            b.Trigger();
        }
    }

    protected override void BlockIngameInteractables()
    {
        registerOutputVisualizer.UIRegisterPanel.WeButton.interactable = false;

        SwitchMuxInteractables(false, upperMuxVisualizer);
        SwitchMuxInteractables(false, middleMuxVisualizer);
        SwitchMuxInteractables(false, downMuxVisualizer);
        SwitchMuxInteractables(false, outputMuxVisualizer);
    }

    protected override bool CheckWinCondition()
    {
        return _output.Output == RightAnswerValue;
    }

    protected override object GetCurrentState()
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
        var up = CalculateMux(upperMuxVisualizer.CurrentChosenMuxPath, -4, 0, -1);
        var left = CalculateMux(middleMuxVisualizer.CurrentChosenMuxPath, 8, 12, -1);
        var down = CalculateMux(downMuxVisualizer.CurrentChosenMuxPath, left, -8, -12);

        _output.Input = CalculateMux(outputMuxVisualizer.CurrentChosenMuxPath, up, 4, down);

        _output.PreClockUpdate();
        _output.Clock();
    }

    protected override void ReleaseIngameInteractables()
    {
        registerOutputVisualizer.UIRegisterPanel.WeButton.interactable = true;

        SwitchMuxInteractables(true, upperMuxVisualizer);
        SwitchMuxInteractables(true, middleMuxVisualizer);
        SwitchMuxInteractables(true, downMuxVisualizer);
        SwitchMuxInteractables(true, outputMuxVisualizer);
    }

    protected override IEnumerator ReverseBusVisualizations()
    {
        if (CurrentBus >= 1 && CurrentBus <= maxTickNumber)
        {
            var up = upperMuxVisualizer.CurrentChosenMuxPath == 0 ? -4 : 0;
            var left = middleMuxVisualizer.CurrentChosenMuxPath == 0 ? 8 : 12;
            var down = CalculateMux(downMuxVisualizer.CurrentChosenMuxPath, left, -8, -12);

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
            var up = CalculateMux(upperMuxVisualizer.CurrentChosenMuxPath, -4, 0, -1);
            var left = CalculateMux(middleMuxVisualizer.CurrentChosenMuxPath, 8, 12, -1);
            var down = CalculateMux(downMuxVisualizer.CurrentChosenMuxPath, left, -8, -12);
            var output = CalculateMux(outputMuxVisualizer.CurrentChosenMuxPath, up, 4, down);

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

    protected override void UpdateVizualizers()
    {
        _infoOutputRegister.Display("Register 1", $"{_output.Output}");
    }

    #region helpers
    private void MuxVizualizerHelper(int currentPath, MultiplexerVisualizer mux)
    {
        if (currentPath == -1)
        {
            mux.ResetVisualisation();
        }
        else if (currentPath == 0)
        {
            mux.SelectPath(0);
        }
        else if (currentPath == 1)
        {
            mux.SelectPath(1);
        }
        else if (currentPath == 2)
        {
            mux.SelectPath(2);
        }
        else
        {
            Debug.LogError($"Saved multiplexer value {currentPath} is not in [0, 3]");
        }
    }
    private void SwitchMuxInteractables(bool trigger, MultiplexerVisualizer target)
    {
        target.UIController.FirstWayButton.interactable = trigger;
        target.UIController.SecondWayButton.interactable = trigger;
        target.UIController.ThirdWayButton.interactable = trigger;
    }
    private int CalculateMux(int muxCurrentPath, int first, int second, int third)
    {
        var result = 0;
        if (muxCurrentPath == 0)
        {
            result = first;
        }
        else if (muxCurrentPath == 1)
        {
            result = second;
        }
        else if (muxCurrentPath == 2)
        {
            result = third;
        }
        /*else
        {
            Debug.LogError($"Unexpected MUX path {muxCurrentPath}. Expected value: [0, 3]");
        }*/
        return result;
    }
    #endregion
}
