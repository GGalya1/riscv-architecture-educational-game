using System.Collections.Generic;

public class DataInstMemory
{
    #region INPUTS
    // WE
    public bool MemoryWrite { get; set; }

    // WD
    public int WriteData { get; set; }

    // A
    public int Adress { get; set; }
    #endregion

    #region OUTPUTS
    // RD
    public int ReadData { get; private set; }
    #endregion

    // all information is stored as a pair (adress - object). For Objects stays instructions and data
    public Dictionary<int, int> _memory = new Dictionary<int, int>();

    public void LoadWord(int adress, int data) { 
        _memory[adress] = data;
    }

    #region Sequential logic
    public void PreClockUpdate() {
        if (_memory.ContainsKey(Adress)) {
            ReadData = _memory[Adress];
        }
        else {
            ReadData = 0;
        }
    }

    public void Clock() {
        if (MemoryWrite) {
            _memory[Adress] = WriteData;
        }
    }
    #endregion
}
