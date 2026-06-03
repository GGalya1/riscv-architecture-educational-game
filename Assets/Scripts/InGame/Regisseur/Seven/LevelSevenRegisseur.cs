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

public class LevelSevenRegisseur : BaseLevelRegisseur
{
    [FormerlySerializedAs("_registerSrcAVisualizer")]
    [Header("Level 7 Specific Components")]
    [SerializeField] protected RegisterVisualizer registerSrcAVisualizer;
    [FormerlySerializedAs("_registerSrcBVisualizer")] [SerializeField] protected RegisterVisualizer registerSrcBVisualizer;
    [FormerlySerializedAs("_registerOutputVisualizer")] [SerializeField] protected RegisterVisualizer registerOutputVisualizer;
    [FormerlySerializedAs("_registerFileVisualizer")] [SerializeField] protected RegisterFileVisualizer registerFileVisualizer;
    [FormerlySerializedAs("_aluVizualizer")] [SerializeField] protected AluVisualiser aluVizualizer;

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
        SrcA = new Register(6); SrcA.WriteEnable = true;
        SrcB = new Register(7); SrcB.WriteEnable = true;
        Output = new Register(0); Output.WriteEnable = true;

        RegisterFile = new RegisterFile(); RegisterFile.RegisterWriteEnable = true;
        RegisterFile.InitializeRegisters(new int[] { 0, 1, 39, 43, 5, 6, 2,
                                                     40, 1, 39, 13, 56, 64, 20,
                                                     50, 51, 0, 12, 53, 65, 29,
                                                     60, 61, 0, 1, 54, 0, 28,
                                                     70, 30, 31, 0});

        // Caching of UI panels for visualizers
        InfoSrcARegister = registerSrcAVisualizer.UIRegisterPanel;
        InfoSrcBRegister = registerSrcBVisualizer.UIRegisterPanel;
        InfoOutputRegister = registerOutputVisualizer.UIRegisterPanel;


        if (levelTargetDescription == null || levelTargetDescription.Length == 0)
        {
            levelTargetText.text = $"Ziel: \r\nSchreibe in Register 3 die Summe von r7 und r6.";
        }
        else
        {
            levelTargetText.text = levelTargetDescription;
        }
        

        UpdateVizualizers();
        UpdateRegisterFileVizualization();
    }

    protected override void ApplyState(object state)
    {
        var s = (LevelSevenState)state;

        SrcA = new Register(s.RegisterAValue);
        SrcB = new Register(s.RegisterBValue);
        Output = new Register(s.RegisterOutputValue);

        SrcA.WriteEnable = s.RegisterAwe;
        SrcB.WriteEnable = s.RegisterBwe;
        Output.WriteEnable = s.RegisterOutputWe;
        RegisterFile.RegisterWriteEnable = s.RegisterFileWe;

        aluVizualizer.ChooseAluOperation(s.AluOperation);
    }

    protected override void BlinkClockedComponents()
    {
        registerSrcAVisualizer.TriggerBlink();
        registerSrcBVisualizer.TriggerBlink();
        registerOutputVisualizer.TriggerBlink();

        registerFileVisualizer.TriggerBlink();
    }

    protected override void BlockIngameInteractables()
    {
        registerSrcAVisualizer.UIRegisterPanel.WeButton.interactable = false;
        registerSrcBVisualizer.UIRegisterPanel.WeButton.interactable = false;
        registerOutputVisualizer.UIRegisterPanel.WeButton.interactable = false;

        registerFileVisualizer.UIRegisterPanel.WeButton.interactable = false;

        aluVizualizer.uiController.FirstOperationButton.interactable = false;
        aluVizualizer.uiController.SecondOperationButton.interactable = false;
        aluVizualizer.uiController.ThirdOperationButton.interactable = false;
        aluVizualizer.uiController.FourthOperationButton.interactable = false;
    }

    protected override bool CheckWinCondition()
    {
        return (Output.Output == RightAnswerValue);
    }

    protected override object GetCurrentState()
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

            AluOperation = aluVizualizer.CurrentAluOperation,
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
        Output.Input = Alu.Calculate(RegisterFile.ReadData1, RegisterFile.ReadData2, aluVizualizer.CurrentAluOperation);

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

    /*protected override bool IsStateEqual(object state)
    {
        if (!(state is LevelSevenState s)) return false;

        return (s.RegisterAValue == srcA.Output) &&
                (s.RegisterBValue == srcB.Output) &&
                (s.RegisterOutputValue == output.Output) &&

                (s.RegisterAWE == srcA.WriteEnable) &&
                (s.RegisterBWE == srcB.WriteEnable) &&
                (s.RegisterOutputWE == output.WriteEnable) &&
                (s.RegisterFileWE == registerFile.RegisterWriteEnable) &&

                (s.ALUOperation == _aluVizualizer.CurrentALUOperation);
    }*/

    protected override void ReleaseIngameInteractables()
    {
        registerSrcAVisualizer.UIRegisterPanel.WeButton.interactable = true;
        registerSrcBVisualizer.UIRegisterPanel.WeButton.interactable = true;
        registerOutputVisualizer.UIRegisterPanel.WeButton.interactable = true;

        registerFileVisualizer.UIRegisterPanel.WeButton.interactable = true;

        aluVizualizer.uiController.FirstOperationButton.interactable = true;
        aluVizualizer.uiController.SecondOperationButton.interactable = true;
        aluVizualizer.uiController.ThirdOperationButton.interactable = true;
        aluVizualizer.uiController.FourthOperationButton.interactable = true;
    }

    protected override IEnumerator ReverseBusVisualizations()
    {
        if (CurrentBus >= 1 && CurrentBus <= maxTickNumber)
        {
            busController.StartBusSignal(busController.busSegments[4], Output.Input, true);
            yield return new WaitUntil(() => busController.NoActiveSignals);

            if (TickStateValues[TickCounter] is LevelSevenState s)
            {
                busController.StartBusSignal(busController.busSegments[2], RegisterFile.Registers[s.RegisterAValue], true);
                busController.StartBusSignal(busController.busSegments[3], RegisterFile.Registers[s.RegisterBValue], true);
                yield return new WaitUntil(() => busController.NoActiveSignals);

                busController.StartBusSignal(busController.busSegments[0], s.RegisterAValue, true);
                busController.StartBusSignal(busController.busSegments[1], s.RegisterBValue, true);
            }
            

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

            busController.StartBusSignal(busController.busSegments[4], Alu.Calculate(RegisterFile.ReadData1, RegisterFile.ReadData2, aluVizualizer.CurrentAluOperation));

            CurrentBus++;
        }

        yield return new WaitUntil(() => busController.NoActiveSignals);
    }

    protected override void UpdateVizualizers()
    {
        InfoSrcARegister.Display("Register 1", $"{SrcA.Output}");
        InfoSrcBRegister.Display("Register 2", $"{SrcB.Output}");
        InfoOutputRegister.Display("Register 3", $"{Output.Output}");

        registerSrcAVisualizer.ForceUpdateWriteEnableVisualization(SrcA.WriteEnable);
        registerSrcBVisualizer.ForceUpdateWriteEnableVisualization(SrcB.WriteEnable);
        registerOutputVisualizer.ForceUpdateWriteEnableVisualization(Output.WriteEnable);
        registerFileVisualizer.ForceUpdateWriteEnableVisualization(RegisterFile.RegisterWriteEnable);
    }

    private void UpdateRegisterFileVizualization() {
        registerFileVisualizer.UIRegisterPanel.Display(RegisterFile.Registers);
    }
}
