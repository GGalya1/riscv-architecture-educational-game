using UnityEngine;

public class SingleCycleProcessorView : MonoBehaviour
{
    [Header("PC & Branching Buses")] [SerializeField]
    private LineRenderer pcNextBus; // Вход в регистр PC (после мультиплексора)

    [SerializeField] private LineRenderer pcBus; // Выход из PC до разветвления
    [SerializeField] private LineRenderer pcToInstrMemBus; // От PC к Instruction Memory (A)
    [SerializeField] private LineRenderer pcToPlus4AdderBus; // От PC к верхнему сумматору (+)
    [SerializeField] private LineRenderer pcToBtaAdderBus; // От PC к нижнему сумматору переходов (+)
    [SerializeField] private LineRenderer pcPlus4Bus; // Выход верхнего сумматора (PC + 4) к Mux(0)
    [SerializeField] private LineRenderer btaBus; // Branch Target Address: выход нижнего сумматора к Mux(1)

    [Header("Instruction Fields Buses")] [SerializeField]
    private LineRenderer instrFullBus; // Основной выход RD из Instruction Memory

    [SerializeField] private LineRenderer opCodeBus; // Инструкция [6:0] -> в Control Unit
    [SerializeField] private LineRenderer funct3Bus; // Инструкция [14:12] -> в Control Unit
    [SerializeField] private LineRenderer funct7Bus; // Инструкция [30] -> в Control Unit
    [SerializeField] private LineRenderer addrA1Bus; // Инструкция [19:15] -> Регистр-источник 1 (A1)
    [SerializeField] private LineRenderer addrA2Bus; // Инструкция [24:20] -> Регистр-источник 2 (A2)
    [SerializeField] private LineRenderer addrA3Bus; // Инструкция [11:7] -> Регистр назначения (A3)
    [SerializeField] private LineRenderer immFieldsBus; // Инструкция [31:7] -> В блок Extend

    [Header("Register File & ALU Data Buses")] [SerializeField]
    private LineRenderer rd1SrcABus; // Выход RD1 из Register File -> Вход SrcA процессора ALU

    [SerializeField] private LineRenderer rd2Bus; // Выход RD2 из Register File до разветвления
    [SerializeField] private LineRenderer rd2ToAluMuxBus; // От RD2 к мультиплексору ALU Mux(0)
    [SerializeField] private LineRenderer writeDataBus; // От RD2 к Data Memory (WD)
    [SerializeField] private LineRenderer immExtBus; // Выход ImmExt из Extend до разветвления
    [SerializeField] private LineRenderer immExtToAluMuxBus; // От ImmExt к мультиплексору ALU Mux(1)
    [SerializeField] private LineRenderer immExtToBtaBus; // От ImmExt к сумматору переходов BTA (+)
    [SerializeField] private LineRenderer srcBBus; // Выход мультиплексора -> Вход SrcB процессора ALU

    [Header("Memory & Writeback Buses")] [SerializeField]
    private LineRenderer aluResultBus; // Выход ALUResult до разветвления

    [SerializeField] private LineRenderer aluToDataMemAddrBus; // От ALUResult к адресу Data Memory (A)
    [SerializeField] private LineRenderer aluToResultMuxBus; // От ALUResult к финальному Mux(0)
    [SerializeField] private LineRenderer readDataBus; // Выход RD из Data Memory -> к финальному Mux(1)
    [SerializeField] private LineRenderer resultBus; // Выход финального Mux -> возвращается на WD3 в Register File
}