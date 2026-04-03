public class RegisterFile: ISequentialLogic
{
    // data for 32 registers (0-31)
    private int[] _registers;

    #region INPUTS

    // WE
    public bool RegisterWriteEnable { get; set; }

    // WD3
    public int WriteData { get; set; }

    // A3 (aka Rd)
    public int WriteAdress { get; set; }

    // A1, A2 (aka Rs1, Rs2)
    public int ReadAdress1 { get; set; }
    public int ReadAdress2 { get; set; }
    #endregion

    #region OUTPUTS

    // RD1, RD2
    public int ReadData1 { get; set; }
    public int ReadData2 { get; set; }
    #endregion

    public int[] Registers => _registers;

    #region Sequential logic

    public void PreClockUpdate() {
        ReadRegisters();
    }

    public void Clock() {
        if (RegisterWriteEnable) {
            if (WriteAdress < _registers.Length && WriteAdress >= 0) {
                _registers[WriteAdress] = WriteData;
            }
        }
    }
    #endregion

    public RegisterFile()
    {
        _registers = new int[32];
    }

    public void InitializeRegisters(int[] values) {
        _registers = values;
    }

    // read of registers in Register file is O(1)
    public void ReadRegisters() {
        if (ReadAdress1 <= 0 || ReadAdress1 >= _registers.Length)
            ReadData1 = 0;
        else 
            ReadData1 = _registers[ReadAdress1];

        if (ReadAdress2 <= 0 || ReadAdress2 >= _registers.Length)
            ReadData2 = 0;
        else
            ReadData2 = _registers[ReadAdress2];
    }
}
