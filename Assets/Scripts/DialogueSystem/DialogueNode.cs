using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// Represents a single step (node) in a dialogue tree.
/// Stores character data, dialogue text, and branching options.
/// </summary>
[CreateAssetMenu(fileName = "NewDialogueNode", menuName = "Dialogue System/Node")]
public class DialogueNode : ScriptableObject
{
    [FormerlySerializedAs("DialogueText")]
    [Header("Content")]
    [Tooltip("The text spoken by the character to the player.")]
    [TextArea(3, 10)]
    public string dialogueText;

    [Tooltip("The name of the character speaking this line.")]
    private string _characterName;

    [Tooltip("Index used to fetch a specific emotion sprite from the character's sprite array.")]
    public EmotionType emotionIndex;

    [Header("Response Options")]
    [TextArea(3, 10)] public string firstAnswer;
    [TextArea(3, 10)] public string secondAnswer;
    [TextArea(3, 10)] public string thirdAnswer;

    [Header("Branching Indices")]
    [Tooltip("Index of the DialogueNode to load if the first option is selected.")]
    public int firstOption;

    [Tooltip("Index of the DialogueNode to load if the second option is selected.")]
    public int secondOption;

    [Tooltip("Index of the DialogueNode to load if the third option is selected.")]
    public int thirdOption;
}
