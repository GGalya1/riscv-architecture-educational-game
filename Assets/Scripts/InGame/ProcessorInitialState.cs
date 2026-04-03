using UnityEngine;

[CreateAssetMenu(fileName = "ProcessorInitialState", menuName = "Scriptable Objects/ProcessorInitialState")]
public class ProcessorInitialState : ScriptableObject
{
    [Header("Level target description")]
    public string levelTarget;

    [Header("StartFields")]
    public int firstMemoWord;
    public int secondMemoWord;
    public int thirdMemoWord;
    public int fourthMemoWord;
    public int pcRegisterInitialValue;

    [Header("Answer Fields")]
    public ExerciseTyp _aufgabeTyp = ExerciseTyp.REGISTER_FIELD;
    public int RegisterFieldAdressAnswer;
    public int RegisterFieldValueAnswer;
    public int MemoryAdressAnswer;
    public int MemoryValueAnswer;
    public int pcValueAnswer;

    [Header("Level Dialogues")]
    public DialogueGraph customDialogueGraph;

    [Header("Next processor initial state")]
    public ProcessorInitialState nextSceneInitial;
}
