using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

public struct SubExtendedSevenLevelState
{
    public int RegisterSrcAValue;
    public int RegisterImmValue;
    public int RegisterA3Value;
    public int RegisterWd3Value;

    public int[] RegisterFieldValue;

    public bool RegisterSrcAwe;
    public bool RegisterImmWe;
    public bool RegisterA3We;
    public bool RegisterWd3We;

    public int AluOperation;

    public int ExtenderOperation;

    public int MuxPath;
}

public class SubExtendedRegisseur : BaseLevelRegisseur
{
    [FormerlySerializedAs("_registerSrcAVisualizer")] [SerializeField] protected RegisterVisualizer registerSrcAVisualizer;
    [FormerlySerializedAs("_registerImmediateVisualizer")] [SerializeField] protected RegisterVisualizer registerImmediateVisualizer;
    [FormerlySerializedAs("_registerA3Visualizer")] [SerializeField] protected RegisterVisualizer registerA3Visualizer;
    [FormerlySerializedAs("_registerWD3Visualizer")] [SerializeField] protected RegisterVisualizer registerWd3Visualizer;

    [FormerlySerializedAs("_aluVizualizer")] [SerializeField] protected AluVisualiser aluVizualizer;

    [FormerlySerializedAs("_registerFileVisualizer")] [SerializeField] protected RegisterFileVisualizer registerFileVisualizer;
    [FormerlySerializedAs("_extenderVizualizer")] [SerializeField] private ExtenderVisualizer extenderVizualizer;
    [FormerlySerializedAs("_MUXVisualizer")] [SerializeField] private MultiplexerVisualizer muxVisualizer;

    #region CACHED UI REFERENCES
    protected InfoPanelUI InfoSrcARegister;
    protected InfoPanelUI InfoImmRegister;
    protected InfoPanelUI InfoA3Register;
    protected InfoPanelUI InfoWd3Register;
    #endregion

    // Intern components for computations
    protected Register SrcA;
    protected Register ImmValue;
    protected Register A3;
    protected Register Wd3;

    protected RegisterFile RegisterFile;

    protected int CurrentBus;

    protected override void OnLevelStart()
    {
        // addi x0, x4, 256
        SrcA = new Register(4); SrcA.WriteEnable = true;
        ImmValue = new Register(268566547); ImmValue.WriteEnable = true;
        A3 = new Register(0); A3.WriteEnable = true;
        Wd3 = new Register(0); Wd3.WriteEnable = true;

        RegisterFile = new RegisterFile(); RegisterFile.RegisterWriteEnable = true;
        RegisterFile.InitializeRegisters(new int[] { -98, 1, 39, 43, 0, 6, 8,
                                                     40, 3, 39, 13, 56, 64, 20,
                                                     50, 51, 0, 12, 53, 65, 29,
                                                     60, 61, 0, 25, 54, 0, 28,
                                                     70, 30, 31, 0});

        // Caching of UI panels for visualizers
        InfoSrcARegister = registerSrcAVisualizer.UIRegisterPanel;
        InfoImmRegister = registerImmediateVisualizer.UIRegisterPanel;
        InfoA3Register = registerA3Visualizer.UIRegisterPanel;
        InfoWd3Register = registerWd3Visualizer.UIRegisterPanel;


        if (levelTargetDescription == null || levelTargetDescription.Length == 0)
        {
            levelTargetText.text = $"Hier Ziel schreiben";
        }
        else
        {
            levelTargetText.text = levelTargetDescription;
        }

        UpdateVizualizers();
    }

    protected override void ApplyState(object state)
    {
        var s = (SubExtendedSevenLevelState)state;

        SrcA = new Register(s.RegisterSrcAValue);
        ImmValue = new Register(s.RegisterImmValue);
        A3 = new Register(s.RegisterA3Value);
        Wd3 = new Register(s.RegisterWd3Value);

        RegisterFile.InitializeRegisters(s.RegisterFieldValue);

        SrcA.WriteEnable = s.RegisterSrcAwe;
        ImmValue.WriteEnable = s.RegisterImmWe;
        A3.WriteEnable = s.RegisterA3We;
        Wd3.WriteEnable = s.RegisterWd3We;

        aluVizualizer.ChooseAluOperation(s.AluOperation);
        extenderVizualizer.ChooseAluOperation(s.ExtenderOperation);

        var currentPath = s.MuxPath;
        if (currentPath == -1)
        {
            muxVisualizer.ResetVisualisation();
        }
        else if (currentPath == 0)
        {
            muxVisualizer.SelectPath(0);
        }
        else if (currentPath == 1)
        {
            muxVisualizer.SelectPath(1);
        }
        else if (currentPath == 2)
        {
            muxVisualizer.SelectPath(2);
        }
        else
        {
            Debug.LogError($"Saved multiplexer value {currentPath} is not in [0, 3]");
        }
    }

    protected override void BlinkClockedComponents()
    {
        registerSrcAVisualizer.TriggerBlink();
        registerImmediateVisualizer.TriggerBlink();
        registerA3Visualizer.TriggerBlink();
        registerWd3Visualizer.TriggerBlink();

        registerFileVisualizer.TriggerBlink();
    }

    protected override void BlockIngameInteractables()
    {
        registerSrcAVisualizer.UIRegisterPanel.WeButton.interactable = false;
        registerImmediateVisualizer.UIRegisterPanel.WeButton.interactable = false;
        registerA3Visualizer.UIRegisterPanel.WeButton.interactable = false;
        registerWd3Visualizer.UIRegisterPanel.WeButton.interactable = false;

        registerFileVisualizer.UIRegisterPanel.WeButton.interactable = false;

        aluVizualizer.uiController.FirstOperationButton.interactable = false;
        aluVizualizer.uiController.SecondOperationButton.interactable = false;
        aluVizualizer.uiController.ThirdOperationButton.interactable = false;
        aluVizualizer.uiController.FourthOperationButton.interactable = false;

        extenderVizualizer.uiController.FirstOperationButton.interactable = false;
        extenderVizualizer.uiController.SecondOperationButton.interactable = false;
        extenderVizualizer.uiController.ThirdOperationButton.interactable = false;
        extenderVizualizer.uiController.FourthOperationButton.interactable = false;

        muxVisualizer.UIController.FirstWayButton.interactable = false;
        muxVisualizer.UIController.SecondWayButton.interactable = false;
        muxVisualizer.UIController.ThirdWayButton.interactable = false;
    }

    protected override bool CheckWinCondition()
    {
        return RegisterFile.Registers[0] == 256;
    }

    protected override object GetCurrentState()
    {
        return new SubExtendedSevenLevelState
        {
            RegisterSrcAValue = SrcA.Output,
            RegisterImmValue = ImmValue.Output,
            RegisterA3Value = A3.Output,
            RegisterWd3Value = Wd3.Output,

            RegisterFieldValue = RegisterFile.Registers,

            RegisterSrcAwe = SrcA.WriteEnable,
            RegisterImmWe = ImmValue.WriteEnable,
            RegisterA3We = A3.WriteEnable,
            RegisterWd3We = Wd3.WriteEnable,

            AluOperation = aluVizualizer.CurrentAluOperation,

            ExtenderOperation = extenderVizualizer.CurrentAluOperation,

            MuxPath = muxVisualizer.CurrentChosenMuxPath,
        };
    }

    protected override void HandleClockUpdate()
    {
        // sinchronyse vizualisers and concrete objects
        SrcA.WriteEnable = registerSrcAVisualizer.isWriteEnabled;
        ImmValue.WriteEnable = registerImmediateVisualizer.isWriteEnabled;
        A3.WriteEnable = registerA3Visualizer.isWriteEnabled;
        Wd3.WriteEnable = registerWd3Visualizer.isWriteEnabled;
        
        RegisterFile.RegisterWriteEnable = registerFileVisualizer.isWriteEnabled;

        // implementation
        RegisterFile.ReadAdress1 = SrcA.Output;
        RegisterFile.ReadAdress2 = 0;

        RegisterFile.ReadRegisters();

        var a = 0;
        if (SrcA.Output > 0 && SrcA.Output < 16)
            a = RegisterFile.Registers[SrcA.Output];
        var ext = Extender.Evaluate(extenderVizualizer.CurrentAluOperation, (uint)ImmValue.Output);
        var muxVal = CalculateMux(0, ext, -1, muxVisualizer.CurrentChosenMuxPath);

        Wd3.Input = Alu.Calculate(a, muxVal, aluVizualizer.CurrentAluOperation);

        if (TickCounter - 1 >= 0)
        {
            var legcyState = (SubExtendedSevenLevelState)TickStateValues[TickCounter - 1];

            RegisterFile.WriteAdress = legcyState.RegisterA3Value;
        }

        RegisterFile.WriteData = Wd3.Output;



        SrcA.PreClockUpdate();
        ImmValue.PreClockUpdate();
        A3.PreClockUpdate();
        Wd3.PreClockUpdate();


        // Only if WriteEnable = true, call Clock
        SrcA.Clock();
        ImmValue.Clock();
        A3.Clock();
        Wd3.Clock();
        RegisterFile.Clock();
    }

    protected override void ReleaseIngameInteractables()
    {
        registerSrcAVisualizer.UIRegisterPanel.WeButton.interactable = true;
        registerImmediateVisualizer.UIRegisterPanel.WeButton.interactable = true;
        registerA3Visualizer.UIRegisterPanel.WeButton.interactable = true;
        registerWd3Visualizer.UIRegisterPanel.WeButton.interactable = true;

        registerFileVisualizer.UIRegisterPanel.WeButton.interactable = true;

        aluVizualizer.uiController.FirstOperationButton.interactable = true;
        aluVizualizer.uiController.SecondOperationButton.interactable = true;
        aluVizualizer.uiController.ThirdOperationButton.interactable = true;
        aluVizualizer.uiController.FourthOperationButton.interactable = true;

        extenderVizualizer.uiController.FirstOperationButton.interactable = true;
        extenderVizualizer.uiController.SecondOperationButton.interactable = true;
        extenderVizualizer.uiController.ThirdOperationButton.interactable = true;
        extenderVizualizer.uiController.FourthOperationButton.interactable = true;

        muxVisualizer.UIController.FirstWayButton.interactable = true;
        muxVisualizer.UIController.SecondWayButton.interactable = true;
        muxVisualizer.UIController.ThirdWayButton.interactable = true;
    }

    protected override IEnumerator ReverseBusVisualizations()
    {
        if (CurrentBus >= 1 && CurrentBus <= maxTickNumber)
        {
            busController.StartBusSignal(busController.busSegments[8], Wd3.Input, true);
            yield return new WaitUntil(() => busController.NoActiveSignals);

            busController.StartBusSignal(busController.busSegments[7], Wd3.Input, true);
            yield return new WaitUntil(() => busController.NoActiveSignals);

            var ext = Extender.Evaluate(extenderVizualizer.CurrentAluOperation, (uint)ImmValue.Output);
            var mux = CalculateMux(0, ext, -1, muxVisualizer.CurrentChosenMuxPath);

            busController.StartBusSignal(busController.busSegments[3], RegisterFile.Registers[SrcA.Output], true);
            busController.StartBusSignal(busController.busSegments[6], mux, true);
            yield return new WaitUntil(() => busController.NoActiveSignals);

            busController.StartBusSignal(busController.busSegments[4], 0, true);
            busController.StartBusSignal(busController.busSegments[5], ext, true);
            yield return new WaitUntil(() => busController.NoActiveSignals);

            busController.StartBusSignal(busController.busSegments[0], SrcA.Output);
            busController.StartBusSignal(busController.busSegments[1], A3.Output);
            busController.StartBusSignal(busController.busSegments[2], ImmValue.Output);

            CurrentBus--;
        }

        yield return new WaitUntil(() => busController.NoActiveSignals);
    }

    protected override IEnumerator RunBusVisualizations()
    {
        if (CurrentBus >= 0 && CurrentBus < maxTickNumber)
        {
            busController.StartBusSignal(busController.busSegments[0], SrcA.Output);
            busController.StartBusSignal(busController.busSegments[1], A3.Output);
            busController.StartBusSignal(busController.busSegments[2], ImmValue.Output);
            yield return new WaitUntil(() => busController.NoActiveSignals);

            var a = 0;
            if (SrcA.Output > 0 && SrcA.Output < 16)
                a = RegisterFile.Registers[SrcA.Output];

            var ext = Extender.Evaluate(extenderVizualizer.CurrentAluOperation, (uint)ImmValue.Output);
            var mux = CalculateMux(0, ext, -1, muxVisualizer.CurrentChosenMuxPath);

            busController.StartBusSignal(busController.busSegments[4], 0);
            busController.StartBusSignal(busController.busSegments[5], ext);
            yield return new WaitUntil(() => busController.NoActiveSignals);

            busController.StartBusSignal(busController.busSegments[3], a);
            busController.StartBusSignal(busController.busSegments[6], mux);
            yield return new WaitUntil(() => busController.NoActiveSignals);

            busController.StartBusSignal(busController.busSegments[7], Alu.Calculate(a, mux, aluVizualizer.CurrentAluOperation));

            yield return new WaitUntil(() => busController.NoActiveSignals);

            busController.StartBusSignal(busController.busSegments[8], Wd3.Output);

            CurrentBus++;
        }

        yield return new WaitUntil(() => busController.NoActiveSignals);
    }

    protected override void UpdateVizualizers()
    {
        InfoSrcARegister.Display("Register A1", $"{SrcA.Output}");
        InfoImmRegister.Display("Register A2", CommandBuilder((uint)ImmValue.Output));
        InfoA3Register.Display("Register A3", $"{A3.Output}");
        InfoWd3Register.Display("Register WD3", $"{Wd3.Output}");

        registerFileVisualizer.UIRegisterPanel.Display(RegisterFile.Registers);


        // ==============================  WE SECTION  =====================================
        registerSrcAVisualizer.ForceUpdateWriteEnableVisualization(SrcA.WriteEnable);
        registerImmediateVisualizer.ForceUpdateWriteEnableVisualization(ImmValue.WriteEnable);
        registerA3Visualizer.ForceUpdateWriteEnableVisualization(A3.WriteEnable);
        registerWd3Visualizer.ForceUpdateWriteEnableVisualization(Wd3.WriteEnable);

        registerFileVisualizer.ForceUpdateWriteEnableVisualization(RegisterFile.RegisterWriteEnable);
    }

    #region helpers
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
