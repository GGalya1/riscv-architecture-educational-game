using UnityEngine;

/// <summary>
/// Holds the initial configuration for a single Eintaktprozessor (single-cycle) sublevel.
/// Unlike ProcessorInitialState (Mehrtaktprozessor), this explicitly separates
/// Instruction Memory and Data Memory.
/// </summary>
[CreateAssetMenu(fileName = "OneTickProcessorInitialState",
                 menuName = "Scriptable Objects/OneTickProcessorInitialState")]
public class OneTickProcessorInitialState : BaseProcessorInitialState
{
    // Instruction Memory
    [Header("Instruction Memory  [addresses 0 / 4 / 8 / 12]")]
    public int firstInstructionWord;
    public int secondInstructionWord;
    public int thirdInstructionWord;
    public int fourthInstructionWord;

    // Data Memory
    [Header("Data Memory  [addresses 0 / 4 / 8 / 12]")]
    public int firstDataWord;
    public int secondDataWord;
    public int thirdDataWord;
    public int fourthDataWord;
}