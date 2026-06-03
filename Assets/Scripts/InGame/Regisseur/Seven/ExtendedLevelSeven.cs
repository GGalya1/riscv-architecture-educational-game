using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

public struct ExtendedSevenLevelState
{
    public int RegisterSrcAValue;
    public int RegisterSrcBValue;
    public int RegisterA3Value;
    public int RegisterWd3Value;

    public int[] RegisterFieldValue;

    public bool RegisterSrcAwe;
    public bool RegisterSrcBwe;
    public bool RegisterA3We;
    public bool RegisterWd3We;

    public int AluOperation;
}

public class ExtendedLevelSeven : BaseLevelRegisseur
{
    [FormerlySerializedAs("_registerSrcAVisualizer")]
    [Header("Level Seven Components")]
    [SerializeField] protected RegisterVisualizer registerSrcAVisualizer;
    [FormerlySerializedAs("_registerSrcBVisualizer")] [SerializeField] protected RegisterVisualizer registerSrcBVisualizer;
    [FormerlySerializedAs("_registerA3Visualizer")] [SerializeField] protected RegisterVisualizer registerA3Visualizer;
    [FormerlySerializedAs("_registerWD3Visualizer")] [SerializeField] protected RegisterVisualizer registerWd3Visualizer;

    [FormerlySerializedAs("_aluVizualizer")] [SerializeField] protected AluVisualiser aluVizualizer;

    [FormerlySerializedAs("_registerFileVisualizer")] [SerializeField] protected RegisterFileVisualizer registerFileVisualizer;

    #region CACHED UI REFERENCES
    protected InfoPanelUI InfoSrcARegister;
    protected InfoPanelUI InfoSrcBRegister;
    protected InfoPanelUI InfoA3Register;
    protected InfoPanelUI InfoWd3Register;
    #endregion

    // Intern components for computations
    protected Register SrcA;
    protected Register SrcB;
    protected Register A3;
    protected Register Wd3;

    protected RegisterFile RegisterFile;


    protected int CurrentBus; // [0, 10]

    protected override void OnLevelStart()
    {
        SrcA = new Register(2); SrcA.WriteEnable = true;
        SrcB = new Register(8); SrcB.WriteEnable = true;
        A3 = new Register(0); A3.WriteEnable = true;
        Wd3 = new Register(0); Wd3.WriteEnable = true;

        RegisterFile = new RegisterFile(); RegisterFile.RegisterWriteEnable = true;
        RegisterFile.InitializeRegisters(new int[] { 0, 1, 39, 43, 5, 6, 8,
                                                     40, 3, 39, 13, 56, 64, 20,
                                                     50, 51, 0, 12, 53, 65, 29,
                                                     60, 61, 0, 25, 54, 0, 28,
                                                     70, 30, 31, 0});

        // Caching of UI panels for visualizers
        InfoSrcARegister = registerSrcAVisualizer.UIRegisterPanel;
        InfoSrcBRegister = registerSrcBVisualizer.UIRegisterPanel;
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
        var s = (ExtendedSevenLevelState)state;

        SrcA = new Register(s.RegisterSrcAValue);
        SrcB = new Register(s.RegisterSrcBValue);
        A3 = new Register(s.RegisterA3Value);
        Wd3 = new Register(s.RegisterWd3Value);

        RegisterFile.InitializeRegisters(s.RegisterFieldValue);

        SrcA.WriteEnable = s.RegisterSrcAwe;
        SrcB.WriteEnable = s.RegisterSrcBwe;
        A3.WriteEnable = s.RegisterA3We;
        Wd3.WriteEnable = s.RegisterWd3We;

        aluVizualizer.ChooseAluOperation(s.AluOperation);
    }

    protected override void BlinkClockedComponents()
    {
        registerSrcAVisualizer.TriggerBlink();
        registerSrcBVisualizer.TriggerBlink();
        registerA3Visualizer.TriggerBlink();
        registerWd3Visualizer.TriggerBlink();

        registerFileVisualizer.TriggerBlink();

    }

    protected override void BlockIngameInteractables()
    {
        registerFileVisualizer.UIRegisterPanel.WeButton.interactable = false;

        registerSrcAVisualizer.UIRegisterPanel.WeButton.interactable = false;
        registerSrcBVisualizer.UIRegisterPanel.WeButton.interactable = false;
        registerA3Visualizer.UIRegisterPanel.WeButton.interactable = false;
        registerWd3Visualizer.UIRegisterPanel.WeButton.interactable = false;

        aluVizualizer.uiController.FirstOperationButton.interactable = false;
        aluVizualizer.uiController.SecondOperationButton.interactable = false;
        aluVizualizer.uiController.ThirdOperationButton.interactable = false;
        aluVizualizer.uiController.FourthOperationButton.interactable = false;
    }

    protected override bool CheckWinCondition()
    {
        return RegisterFile.Registers[0] == 42;
    }

    protected override object GetCurrentState()
    {
        return new ExtendedSevenLevelState {
            RegisterSrcAValue = SrcA.Output,
            RegisterSrcBValue = SrcB.Output,
            RegisterA3Value = A3.Output,
            RegisterWd3Value = Wd3.Output,

            RegisterFieldValue = RegisterFile.Registers,

            RegisterSrcAwe = SrcA.WriteEnable,
            RegisterSrcBwe = SrcB.WriteEnable,
            RegisterA3We = A3.WriteEnable,
            RegisterWd3We = Wd3.WriteEnable,

            AluOperation = aluVizualizer.CurrentAluOperation,
        };
    }

    protected override void HandleClockUpdate()
    {
        SrcA.WriteEnable = registerSrcAVisualizer.isWriteEnabled;
        SrcB.WriteEnable = registerSrcBVisualizer.isWriteEnabled;
        A3.WriteEnable = registerA3Visualizer.isWriteEnabled;
        Wd3.WriteEnable = registerWd3Visualizer.isWriteEnabled;

        RegisterFile.RegisterWriteEnable = registerFileVisualizer.isWriteEnabled;

        // implementation
        // A1: [19:15] (Register Source 1)
        RegisterFile.ReadAdress1 = SrcA.Output;

        // A2: [24:20] (Register Source 2)
        RegisterFile.ReadAdress2 = SrcB.Output;

        RegisterFile.ReadRegisters();

        // A3: [11:7] (Register Destination / rd)

        var a = RegisterFile.ReadData1;
        var b = RegisterFile.ReadData2;

        Wd3.Input = Alu.Calculate(a, b, aluVizualizer.CurrentAluOperation);
        if (TickCounter - 1 >= 0)
        {
            var legcyState = (ExtendedSevenLevelState)TickStateValues[TickCounter - 1];

            RegisterFile.WriteAdress = legcyState.RegisterA3Value;
        }
        RegisterFile.WriteData = Wd3.Output;


        SrcA.PreClockUpdate();
        SrcB.PreClockUpdate();
        A3.PreClockUpdate();
        Wd3.PreClockUpdate();

        SrcA.Clock();
        SrcB.Clock();
        A3.Clock();
        Wd3.Clock();
        RegisterFile.Clock();
    }

    protected override void ReleaseIngameInteractables()
    {
        registerFileVisualizer.UIRegisterPanel.WeButton.interactable = true;

        registerSrcAVisualizer.UIRegisterPanel.WeButton.interactable = true;
        registerSrcBVisualizer.UIRegisterPanel.WeButton.interactable = true;
        registerA3Visualizer.UIRegisterPanel.WeButton.interactable = true;
        registerWd3Visualizer.UIRegisterPanel.WeButton.interactable = true;

        aluVizualizer.uiController.FirstOperationButton.interactable = true;
        aluVizualizer.uiController.SecondOperationButton.interactable = true;
        aluVizualizer.uiController.ThirdOperationButton.interactable = true;
        aluVizualizer.uiController.FourthOperationButton.interactable = true;
    }

    protected override IEnumerator ReverseBusVisualizations()
    {
        if (CurrentBus >= 1 && CurrentBus <= maxTickNumber)
        {
            busController.StartBusSignal(busController.busSegments[6], Wd3.Input, true);

            yield return new WaitUntil(() => busController.NoActiveSignals);

            busController.StartBusSignal(busController.busSegments[5], Wd3.Input, true);

            yield return new WaitUntil(() => busController.NoActiveSignals);

            var a = 0;
            var b = 0;
            if (SrcA.Output > 0 && SrcA.Output < 16)
                a = RegisterFile.Registers[SrcA.Output];

            if (SrcB.Output > 0 & SrcB.Output < 16)
                b = RegisterFile.Registers[SrcB.Output];

            busController.StartBusSignal(busController.busSegments[3], a, true);
            busController.StartBusSignal(busController.busSegments[4], b, true);

            yield return new WaitUntil(() => busController.NoActiveSignals);

            busController.StartBusSignal(busController.busSegments[0], SrcA.Output, true);
            busController.StartBusSignal(busController.busSegments[1], SrcB.Output, true);
            busController.StartBusSignal(busController.busSegments[2], A3.Output, true);

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
            busController.StartBusSignal(busController.busSegments[2], A3.Output);

            yield return new WaitUntil(() => busController.NoActiveSignals);

            var a = 0;
            var b = 0;
            if (SrcA.Output > 0 && SrcA.Output < 16)
                a = RegisterFile.Registers[SrcA.Output];
            
            if(SrcB.Output > 0 & SrcB.Output < 16)
                b = RegisterFile.Registers[SrcB.Output];
            

            busController.StartBusSignal(busController.busSegments[3], a);
            busController.StartBusSignal(busController.busSegments[4], b);

            yield return new WaitUntil(() => busController.NoActiveSignals);

            busController.StartBusSignal(busController.busSegments[5], Alu.Calculate(a, b, aluVizualizer.CurrentAluOperation));

            yield return new WaitUntil(() => busController.NoActiveSignals);

            busController.StartBusSignal(busController.busSegments[6], Wd3.Output);

            CurrentBus++;
        }

        yield return new WaitUntil(() => busController.NoActiveSignals);
    }

    protected override void UpdateVizualizers()
    {
        InfoSrcARegister.Display("Register A1", $"{SrcA.Output}");
        InfoSrcBRegister.Display("Register A2", $"{SrcB.Output}");
        InfoA3Register.Display("Register A3", $"{A3.Output}");
        InfoWd3Register.Display("Register WD3", $"{Wd3.Output}");


        registerFileVisualizer.UIRegisterPanel.Display(RegisterFile.Registers);


        // ==============================  WE SECTION  =====================================
        registerSrcAVisualizer.ForceUpdateWriteEnableVisualization(SrcA.WriteEnable);
        registerSrcBVisualizer.ForceUpdateWriteEnableVisualization(SrcB.WriteEnable);
        registerA3Visualizer.ForceUpdateWriteEnableVisualization(A3.WriteEnable);
        registerWd3Visualizer.ForceUpdateWriteEnableVisualization(Wd3.WriteEnable);

        registerFileVisualizer.ForceUpdateWriteEnableVisualization(RegisterFile.RegisterWriteEnable);
    }
}
