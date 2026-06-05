using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

public struct LevelSevenState
{
    public int RegisterAValue;
    public int RegisterBValue;
    public int RegisterOutputValue;

    public bool RegisterAwe;
    public bool RegisterBwe;
    public bool RegisterOutputWe;
    public bool RegisterFileWe;

    public int AluOperation;
}

public class LevelSevenRegisseur : BaseLevelRegisseur<LevelSevenState>
{
    [FormerlySerializedAs("_registerSrcAVisualizer")]
    [Header("Level 7 Specific Components")]
    [SerializeField] protected RegisterVisualizer registerSrcAVisualizer;
    [FormerlySerializedAs("_registerSrcBVisualizer")] [SerializeField] protected RegisterVisualizer registerSrcBVisualizer;
    [FormerlySerializedAs("_registerOutputVisualizer")] [SerializeField] protected RegisterVisualizer registerOutputVisualizer;
    [FormerlySerializedAs("_registerFileVisualizer")] [SerializeField] protected RegisterFileVisualizer registerFileVisualizer;
    [FormerlySerializedAs("aluVisualizer")] [FormerlySerializedAs("_aluVizualizer")] [SerializeField] protected AluVisualiser aluVisualizer;

    #region CACHED UI REFERENCES
    protected InfoPanelUI InfoSrcARegister;
    protected InfoPanelUI InfoSrcBRegister;
    protected InfoPanelUI InfoOutputRegister;
    #endregion

    // Intern components for computations
    protected Register SrcA;
    protected Register SrcB;
    protected Register Output;
    protected RegisterFile RegisterFile;

    protected override int RightAnswerValue => 42;


    protected int CurrentBus; // [0, 2]

    protected override void OnLevelStart()
    {
        // Initialization of logical components
        SrcA = new Register(6)
        {
            WriteEnable = true
        };
        SrcB = new Register(7)
        {
            WriteEnable = true
        };
        Output = new Register()
        {
            WriteEnable = true
        };

        RegisterFile = new RegisterFile
        {
            RegisterWriteEnable = true
        };
        RegisterFile.InitializeRegisters(new [] { 0, 1, 39, 43, 5, 6, 2,
                                                     40, 1, 39, 13, 56, 64, 20,
                                                     50, 51, 0, 12, 53, 65, 29,
                                                     60, 61, 0, 1, 54, 0, 28,
                                                     70, 30, 31, 0});

        // Caching of UI panels for visualizers
        InfoSrcARegister = registerSrcAVisualizer.UIRegisterPanel;
        InfoSrcBRegister = registerSrcBVisualizer.UIRegisterPanel;
        InfoOutputRegister = registerOutputVisualizer.UIRegisterPanel;
        

        UpdateVisualizers();
        UpdateRegisterFileVisualisation();
    }

    protected override void ApplyState(LevelSevenState s)
    {
        SrcA = new Register(s.RegisterAValue);
        SrcB = new Register(s.RegisterBValue);
        Output = new Register(s.RegisterOutputValue);

        SrcA.WriteEnable = s.RegisterAwe;
        SrcB.WriteEnable = s.RegisterBwe;
        Output.WriteEnable = s.RegisterOutputWe;
        RegisterFile.RegisterWriteEnable = s.RegisterFileWe;

        aluVisualizer.ChooseAluOperation(s.AluOperation);
    }

    protected override void BlinkClockedComponents()
    {
        registerSrcAVisualizer.TriggerBlink();
        registerSrcBVisualizer.TriggerBlink();
        registerOutputVisualizer.TriggerBlink();

        registerFileVisualizer.TriggerBlink();
    }

    protected override bool CheckWinCondition()
    {
        return (Output.Output == RightAnswerValue);
    }

    protected override LevelSevenState GetCurrentState()
    {
        return new LevelSevenState
        {
            RegisterAValue = SrcA.Output,
            RegisterBValue = SrcB.Output,
            RegisterOutputValue = Output.Output,

            RegisterAwe = SrcA.WriteEnable,
            RegisterBwe = SrcB.WriteEnable,
            RegisterOutputWe = Output.WriteEnable,
            RegisterFileWe = RegisterFile.RegisterWriteEnable,

            AluOperation = aluVisualizer.CurrentAluOperation,
        };
    }

    protected override void HandleClockUpdate()
    {
        // sinchronyse vizualisers and concrete objects
        SrcA.WriteEnable = registerSrcAVisualizer.isWriteEnabled;
        SrcB.WriteEnable = registerSrcBVisualizer.isWriteEnabled;
        Output.WriteEnable = registerOutputVisualizer.isWriteEnabled;
        RegisterFile.RegisterWriteEnable = registerFileVisualizer.isWriteEnabled;

        // implementation
        RegisterFile.ReadAdress1 = SrcA.Output;
        RegisterFile.ReadAdress2 = SrcB.Output;
        Output.Input = Alu.Calculate(RegisterFile.ReadData1, RegisterFile.ReadData2, aluVisualizer.CurrentAluOperation);

        SrcA.PreClockUpdate();
        SrcB.PreClockUpdate();
        Output.PreClockUpdate();
        RegisterFile.PreClockUpdate();


        // Only if WriteEnable = true, call Clock
        SrcA.Clock();
        SrcB.Clock();
        Output.Clock();
        RegisterFile.Clock();
    }

    protected override IEnumerator ReverseBusVisualizations()
    {
        if (CurrentBus >= 1 && CurrentBus <= maxTickNumber)
        {
            busController.StartBusSignal(busController.busSegments[4], Output.Input, true);
            yield return new WaitUntil(() => busController.NoActiveSignals);


                busController.StartBusSignal(busController.busSegments[2], RegisterFile.Registers[TickStateValues[TickCounter].RegisterAValue], true);
                busController.StartBusSignal(busController.busSegments[3], RegisterFile.Registers[TickStateValues[TickCounter].RegisterBValue], true);
                yield return new WaitUntil(() => busController.NoActiveSignals);

                busController.StartBusSignal(busController.busSegments[0], TickStateValues[TickCounter].RegisterAValue, true);
                busController.StartBusSignal(busController.busSegments[1], TickStateValues[TickCounter].RegisterBValue, true);
            
            

            CurrentBus--;
        }

        yield return new WaitUntil(() => busController.NoActiveSignals);
    }

    protected override IEnumerator RunBusVisualizations()
    {
        if (CurrentBus >= 0 && CurrentBus < maxTickNumber)
        {
            busController.StartBusSignal(busController.busSegments[0], SrcA.Output);
            busController.StartBusSignal(busController.busSegments[1], SrcB.Output);
            yield return new WaitUntil(() => busController.NoActiveSignals);

            busController.StartBusSignal(busController.busSegments[2], RegisterFile.ReadData1);
            busController.StartBusSignal(busController.busSegments[3], RegisterFile.ReadData2);
            yield return new WaitUntil(() => busController.NoActiveSignals);

            busController.StartBusSignal(busController.busSegments[4], Alu.Calculate(RegisterFile.ReadData1, RegisterFile.ReadData2, aluVisualizer.CurrentAluOperation));

            CurrentBus++;
        }

        yield return new WaitUntil(() => busController.NoActiveSignals);
    }

    protected override void UpdateVisualizers()
    {
        InfoSrcARegister.Display("Register 1", $"{SrcA.Output}");
        InfoSrcBRegister.Display("Register 2", $"{SrcB.Output}");
        InfoOutputRegister.Display("Register 3", $"{Output.Output}");

        registerSrcAVisualizer.ForceUpdateWriteEnableVisualization(SrcA.WriteEnable);
        registerSrcBVisualizer.ForceUpdateWriteEnableVisualization(SrcB.WriteEnable);
        registerOutputVisualizer.ForceUpdateWriteEnableVisualization(Output.WriteEnable);
        registerFileVisualizer.ForceUpdateWriteEnableVisualization(RegisterFile.RegisterWriteEnable);
    }

    private void UpdateRegisterFileVisualisation() {
        registerFileVisualizer.UIRegisterPanel.Display(RegisterFile.Registers);
    }
}
