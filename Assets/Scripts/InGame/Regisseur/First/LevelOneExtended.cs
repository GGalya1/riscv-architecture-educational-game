using System.Collections;
using UnityEngine;

public struct ExtendedFirstLevelState
{
    public int RegisterOutputValue;

    public int MUXup;
    public int MUXmiddle;
    public int MUXdown;
    public int MUXoutput;
}

public class LevelOneExtended : BaseLevelRegisseur
{
    [Header("MUXes specific components")]
    [SerializeField] private RegisterVizualizer _registerOutputVisualizer;
    [SerializeField] private MuiltiplexerVizualizer _upperMUXVisualizer;
    [SerializeField] private MuiltiplexerVizualizer _middleMUXVisualizer;
    [SerializeField] private MuiltiplexerVizualizer _downMUXVisualizer;
    [SerializeField] private MuiltiplexerVizualizer _outputMUXVisualizer;

    [SerializeField] private Blinker[] _numberBlinkers;

    protected override int RightAnswerValue => 12;

    private Register output;
    private InfoPanelUI _infoOutputRegister;

    protected int _currentBus = 0; // [0, 2]

    protected override void OnLevelStart()
    {
        _infoOutputRegister = _registerOutputVisualizer.UIRegisterPanel;
        output = new Register(11); output.WriteEnable = true;

        UpdateVizualizers();
    }

    protected override void ApplyState(object state)
    {
        ExtendedFirstLevelState s = (ExtendedFirstLevelState)state;

        output = new Register(s.RegisterOutputValue);

        muxVizualizerHelper(s.MUXup, _upperMUXVisualizer);
        muxVizualizerHelper(s.MUXmiddle, _middleMUXVisualizer);
        muxVizualizerHelper(s.MUXdown, _downMUXVisualizer);
        muxVizualizerHelper(s.MUXoutput, _outputMUXVisualizer);
    }

    protected override void BlinkClockedComponents()
    {
        foreach (Blinker b in _numberBlinkers) {
            b.Trigger();
        }
    }

    protected override void BlockIngameInteractables()
    {
        _registerOutputVisualizer.UIRegisterPanel.WEButton.interactable = false;

        SwitchMUXInteractables(false, _upperMUXVisualizer);
        SwitchMUXInteractables(false, _middleMUXVisualizer);
        SwitchMUXInteractables(false, _downMUXVisualizer);
        SwitchMUXInteractables(false, _outputMUXVisualizer);
    }

    protected override bool CheckWinCondition()
    {
        return output.Output == RightAnswerValue;
    }

    protected override object GetCurrentState()
    {
        return new ExtendedFirstLevelState
        {
            RegisterOutputValue = output.Output,

            MUXup = _upperMUXVisualizer.CurrentChoosenMuxPath,
            MUXmiddle = _middleMUXVisualizer.CurrentChoosenMuxPath,
            MUXdown = _downMUXVisualizer.CurrentChoosenMuxPath,
            MUXoutput = _outputMUXVisualizer.CurrentChoosenMuxPath,
        };
    }

    protected override void HandleClockUpdate()
    {
        int up = calculateMUX(_upperMUXVisualizer.CurrentChoosenMuxPath, -4, 0, -1);
        int left = calculateMUX(_middleMUXVisualizer.CurrentChoosenMuxPath, 8, 12, -1);
        int down = calculateMUX(_downMUXVisualizer.CurrentChoosenMuxPath, left, -8, -12);

        output.Input = calculateMUX(_outputMUXVisualizer.CurrentChoosenMuxPath, up, 4, down);

        output.PreClockUpdate();
        output.Clock();
    }

    protected override void ReleaseIngameInteractables()
    {
        _registerOutputVisualizer.UIRegisterPanel.WEButton.interactable = true;

        SwitchMUXInteractables(true, _upperMUXVisualizer);
        SwitchMUXInteractables(true, _middleMUXVisualizer);
        SwitchMUXInteractables(true, _downMUXVisualizer);
        SwitchMUXInteractables(true, _outputMUXVisualizer);
    }

    protected override IEnumerator ReverseBusVisualizations()
    {
        if (_currentBus >= 1 && _currentBus <= _maxTickNumber)
        {
            int up = _upperMUXVisualizer.CurrentChoosenMuxPath == 0 ? -4 : 0;
            int left = _middleMUXVisualizer.CurrentChoosenMuxPath == 0 ? 8 : 12;
            int down = calculateMUX(_downMUXVisualizer.CurrentChoosenMuxPath, left, -8, -12);

            _busController.StartBusSignal(_busController.busSegments[10], output.Input, true);
            yield return new WaitUntil(() => _busController.NoActiveSignals);

            _busController.StartBusSignal(_busController.busSegments[7], up, true);
            _busController.StartBusSignal(_busController.busSegments[8], down, true);
            _busController.StartBusSignal(_busController.busSegments[9], 4, true);
            yield return new WaitUntil(() => _busController.NoActiveSignals);

            _busController.StartBusSignal(_busController.busSegments[2], left, true);
            _busController.StartBusSignal(_busController.busSegments[3], -8, true);
            _busController.StartBusSignal(_busController.busSegments[4], -12, true);

            _busController.StartBusSignal(_busController.busSegments[5], -4, true);
            _busController.StartBusSignal(_busController.busSegments[6], 0, true);
            yield return new WaitUntil(() => _busController.NoActiveSignals);

            _busController.StartBusSignal(_busController.busSegments[0], 8, true);
            _busController.StartBusSignal(_busController.busSegments[1], 12, true);

            _currentBus--;
        }

        yield return new WaitUntil(() => _busController.NoActiveSignals);
    }

    protected override IEnumerator RunBusVisualizations()
    {
        if (_currentBus >= 0 && _currentBus < _maxTickNumber)
        {
            int up = calculateMUX(_upperMUXVisualizer.CurrentChoosenMuxPath, -4, 0, -1);
            int left = calculateMUX(_middleMUXVisualizer.CurrentChoosenMuxPath, 8, 12, -1);
            int down = calculateMUX(_downMUXVisualizer.CurrentChoosenMuxPath, left, -8, -12);
            int output = calculateMUX(_outputMUXVisualizer.CurrentChoosenMuxPath, up, 4, down);

            _busController.StartBusSignal(_busController.busSegments[0], 8);
            _busController.StartBusSignal(_busController.busSegments[1], 12);

            yield return new WaitUntil(() => _busController.NoActiveSignals);

            _busController.StartBusSignal(_busController.busSegments[2], left);
            _busController.StartBusSignal(_busController.busSegments[3], -8);
            _busController.StartBusSignal(_busController.busSegments[4], -12);

            _busController.StartBusSignal(_busController.busSegments[5], -4);
            _busController.StartBusSignal(_busController.busSegments[6], 0);

            yield return new WaitUntil(() => _busController.NoActiveSignals);

            _busController.StartBusSignal(_busController.busSegments[7], up);
            _busController.StartBusSignal(_busController.busSegments[8], down);
            _busController.StartBusSignal(_busController.busSegments[9], 4);

            yield return new WaitUntil(() => _busController.NoActiveSignals);

            _busController.StartBusSignal(_busController.busSegments[10], output);

            _currentBus++;
        }

        yield return new WaitUntil(() => _busController.NoActiveSignals);
    }

    protected override void UpdateVizualizers()
    {
        _infoOutputRegister.Display("Register 1", $"{output.Output}");
    }

    #region helpers
    private void muxVizualizerHelper(int currentPath, MuiltiplexerVizualizer mux)
    {
        if (currentPath == -1)
        {
            mux.ResetVizualization();
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
    private void SwitchMUXInteractables(bool trigger, MuiltiplexerVizualizer target)
    {
        target.UIController.FirstWayButton.interactable = trigger;
        target.UIController.SecondWayButton.interactable = trigger;
        target.UIController.ThirdWayButton.interactable = trigger;
    }
    private int calculateMUX(int muxCurrentPath, int first, int second, int third)
    {
        int result = 0;
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
