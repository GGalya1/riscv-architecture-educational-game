using UnityEngine;
using UnityEngine.Serialization;

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

    [FormerlySerializedAs("_aufgabeTyp")] [Header("Answer Fields")]
    public ExerciseTyp aufgabeTyp = ExerciseTyp.REGISTER_FIELD;
    [FormerlySerializedAs("registerFieldAdressAnswer")] [FormerlySerializedAs("RegisterFieldAdressAnswer")] public int registerFieldAddressAnswer;
    [FormerlySerializedAs("RegisterFieldValueAnswer")] public int registerFieldValueAnswer;
    [FormerlySerializedAs("memoryAdressAnswer")] [FormerlySerializedAs("MemoryAdressAnswer")] public int memoryAddressAnswer;
    [FormerlySerializedAs("MemoryValueAnswer")] public int memoryValueAnswer;
    public int pcValueAnswer;

    [Header("Level Dialogues")]
    public DialogueGraph customDialogueGraph;

    [Header("Next processor initial state")]
    public ProcessorInitialState nextSceneInitial;
}
