using System.Collections.Generic;

public class DataInstMemory
{
    #region INPUTS
    // WE
    public bool MemoryWrite { get; set; }

    // WD
    public int WriteData { get; set; }

    // A
    public int Address { get; set; }
    #endregion

    #region OUTPUTS
    // RD
    public int ReadData { get; private set; }
    #endregion

    // all information is stored as a pair (adress - object). For Objects stays instructions and data
    public readonly Dictionary<int, int> Memory = new Dictionary<int, int>();

    public void LoadWord(int address, int data) { 
        Memory[address] = data;
    }

    #region Sequential logic
    public void PreClockUpdate()
    {
        ReadData = Memory.GetValueOrDefault(Address, 0);
    }

    public void Clock() {
        if (MemoryWrite) {
            Memory[Address] = WriteData;
        }
    }
    #endregion
}
