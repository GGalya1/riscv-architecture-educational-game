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

public class SubExtendedRegisseur : BaseLevelRegisseur<SubExtendedSevenLevelState>
{
    [FormerlySerializedAs("_registerSrcAVisualizer")] [SerializeField] protected RegisterVisualizer registerSrcAVisualizer;
    [FormerlySerializedAs("_registerImmediateVisualizer")] [SerializeField] protected RegisterVisualizer registerImmediateVisualizer;
    [FormerlySerializedAs("_registerA3Visualizer")] [SerializeField] protected RegisterVisualizer registerA3Visualizer;
    [FormerlySerializedAs("_registerWD3Visualizer")] [SerializeField] protected RegisterVisualizer registerWd3Visualizer;

    [FormerlySerializedAs("aluVizualizer")] [FormerlySerializedAs("_aluVizualizer")] [SerializeField] protected AluVisualiser aluVisualizer;

    [FormerlySerializedAs("_registerFileVisualizer")] [SerializeField] protected RegisterFileVisualizer registerFileVisualizer;
    [FormerlySerializedAs("extenderVizualizer")] [FormerlySerializedAs("_extenderVizualizer")] [SerializeField] private ExtenderVisualizer extenderVisualizer;
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
        SrcA = new Register(4)
        {
            WriteEnable = true
        };
        ImmValue = new Register(268566547)
        {
            WriteEnable = true
        };
        A3 = new Register()
        {
            WriteEnable = true
        };
        Wd3 = new Register()
        {
            WriteEnable = true
        };

        RegisterFile = new RegisterFile
        {
            RegisterWriteEnable = true
        };
        RegisterFile.InitializeRegisters(new [] { -98, 1, 39, 43, 0, 6, 8,
                                                     40, 3, 39, 13, 56, 64, 20,
                                                     50, 51, 0, 12, 53, 65, 29,
                                                     60, 61, 0, 25, 54, 0, 28,
                                                     70, 30, 31, 0});

        // Caching of UI panels for visualizers
        InfoSrcARegister = registerSrcAVisualizer.UIRegisterPanel;
        InfoImmRegister = registerImmediateVisualizer.UIRegisterPanel;
        InfoA3Register = registerA3Visualizer.UIRegisterPanel;
        InfoWd3Register = registerWd3Visualizer.UIRegisterPanel;
        
        UpdateVisualizers();
    }

    protected override void ApplyState(SubExtendedSevenLevelState s)
    {
        SrcA = new Register(s.RegisterSrcAValue);
        ImmValue = new Register(s.RegisterImmValue);
        A3 = new Register(s.RegisterA3Value);
        Wd3 = new Register(s.RegisterWd3Value);

        RegisterFile.InitializeRegisters(s.RegisterFieldValue);

        SrcA.WriteEnable = s.RegisterSrcAwe;
        ImmValue.WriteEnable = s.RegisterImmWe;
        A3.WriteEnable = s.RegisterA3We;
        Wd3.WriteEnable = s.RegisterWd3We;

        aluVisualizer.ChooseAluOperation(s.AluOperation);
        extenderVisualizer.ChooseAluOperation(s.ExtenderOperation);

        ApplyMuxState(s.MuxPath, muxVisualizer);
    }

    protected override void BlinkClockedComponents()
    {
        registerSrcAVisualizer.TriggerBlink();
        registerImmediateVisualizer.TriggerBlink();
        registerA3Visualizer.TriggerBlink();
        registerWd3Visualizer.TriggerBlink();

        registerFileVisualizer.TriggerBlink();
    }

    protected override void BlockInGameInteractable()
    {
        registerSrcAVisualizer.UIRegisterPanel.WeButton.interactable = false;
        registerImmediateVisualizer.UIRegisterPanel.WeButton.interactable = false;
        registerA3Visualizer.UIRegisterPanel.WeButton.interactable = false;
        registerWd3Visualizer.UIRegisterPanel.WeButton.interactable = false;

        registerFileVisualizer.UIRegisterPanel.WeButton.interactable = false;

        aluVisualizer.uiController.FirstOperationButton.interactable = false;
        aluVisualizer.uiController.SecondOperationButton.interactable = false;
        aluVisualizer.uiController.ThirdOperationButton.interactable = false;
        aluVisualizer.uiController.FourthOperationButton.interactable = false;

        extenderVisualizer.uiController.FirstOperationButton.interactable = false;
        extenderVisualizer.uiController.SecondOperationButton.interactable = false;
        extenderVisualizer.uiController.ThirdOperationButton.interactable = false;
        extenderVisualizer.uiController.FourthOperationButton.interactable = false;

        muxVisualizer.UIController.FirstWayButton.interactable = false;
        muxVisualizer.UIController.SecondWayButton.interactable = false;
        muxVisualizer.UIController.ThirdWayButton.interactable = false;
    }

    protected override bool CheckWinCondition()
    {
        return RegisterFile.Registers[0] == 256;
    }

    protected override SubExtendedSevenLevelState GetCurrentState()
    {
        return new SubExtendedSevenLevelState
        {
            RegisterSrcAValue = SrcA.Output,
            RegisterImmValue = ImmValue.Output,
            RegisterA3Value = A3.Output,
            RegisterWd3Value = Wd3.Output,

            RegisterFieldValue = (int[])RegisterFile.Registers.Clone(),

            RegisterSrcAwe = SrcA.WriteEnable,
            RegisterImmWe = ImmValue.WriteEnable,
            RegisterA3We = A3.WriteEnable,
            RegisterWd3We = Wd3.WriteEnable,

            AluOperation = aluVisualizer.CurrentAluOperation,

            ExtenderOperation = extenderVisualizer.CurrentAluOperation,

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
        if (SrcA.Output is > 0 and < 16)
            a = RegisterFile.Registers[SrcA.Output];
        var ext = Extender.Evaluate(extenderVisualizer.CurrentAluOperation, (uint)ImmValue.Output);
        var muxVal = EvaluateMux(0, ext, -1, muxVisualizer.CurrentChosenMuxPath);

        Wd3.Input = Alu.Calculate(a, muxVal, aluVisualizer.CurrentAluOperation);

        if (TickCounter - 1 >= 0)
        {
            RegisterFile.WriteAdress = TickStateValues[TickCounter - 1].RegisterA3Value;
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

    protected override void ReleaseInGameInteractable()
    {
        registerSrcAVisualizer.UIRegisterPanel.WeButton.interactable = true;
        registerImmediateVisualizer.UIRegisterPanel.WeButton.interactable = true;
        registerA3Visualizer.UIRegisterPanel.WeButton.interactable = true;
        registerWd3Visualizer.UIRegisterPanel.WeButton.interactable = true;

        registerFileVisualizer.UIRegisterPanel.WeButton.interactable = true;

        aluVisualizer.uiController.FirstOperationButton.interactable = true;
        aluVisualizer.uiController.SecondOperationButton.interactable = true;
        aluVisualizer.uiController.ThirdOperationButton.interactable = true;
        aluVisualizer.uiController.FourthOperationButton.interactable = true;

        extenderVisualizer.uiController.FirstOperationButton.interactable = true;
        extenderVisualizer.uiController.SecondOperationButton.interactable = true;
        extenderVisualizer.uiController.ThirdOperationButton.interactable = true;
        extenderVisualizer.uiController.FourthOperationButton.interactable = true;

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

            var ext = Extender.Evaluate(extenderVisualizer.CurrentAluOperation, (uint)ImmValue.Output);
            var mux = EvaluateMux(0, ext, -1, muxVisualizer.CurrentChosenMuxPath);

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
            if (SrcA.Output is > 0 and < 16)
                a = RegisterFile.Registers[SrcA.Output];

            var ext = Extender.Evaluate(extenderVisualizer.CurrentAluOperation, (uint)ImmValue.Output);
            var mux = EvaluateMux(0, ext, -1, muxVisualizer.CurrentChosenMuxPath);

            busController.StartBusSignal(busController.busSegments[4], 0);
            busController.StartBusSignal(busController.busSegments[5], ext);
            yield return new WaitUntil(() => busController.NoActiveSignals);

            busController.StartBusSignal(busController.busSegments[3], a);
            busController.StartBusSignal(busController.busSegments[6], mux);
            yield return new WaitUntil(() => busController.NoActiveSignals);

            busController.StartBusSignal(busController.busSegments[7], Alu.Calculate(a, mux, aluVisualizer.CurrentAluOperation));

            yield return new WaitUntil(() => busController.NoActiveSignals);

            busController.StartBusSignal(busController.busSegments[8], Wd3.Output);

            CurrentBus++;
        }

        yield return new WaitUntil(() => busController.NoActiveSignals);
    }

    protected override void UpdateVisualizers()
    {
        InfoSrcARegister.Display("Register A1", $"{SrcA.Output}");
        InfoImmRegister.Display("Register A2", RiscVDecoder.CommandBuilder((uint)ImmValue.Output));
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
}
