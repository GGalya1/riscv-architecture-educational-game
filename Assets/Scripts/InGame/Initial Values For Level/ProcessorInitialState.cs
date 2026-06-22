using UnityEngine;

[CreateAssetMenu(fileName = "ProcessorInitialState", menuName = "Scriptable Objects/ProcessorInitialState")]
public class ProcessorInitialState : BaseProcessorInitialState
{
    [Header("StartFields")]
    public int firstMemoWord;
    public int secondMemoWord;
    public int thirdMemoWord;
    public int fourthMemoWord;
}
