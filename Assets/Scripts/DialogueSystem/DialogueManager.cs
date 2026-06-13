using System;
using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// Orchestrates the dialogue flow by connecting data (DialogueGraph) with the interface (DialogueUI).
/// Manages node transitions, branching logic, and dialogue events.
/// </summary>
public class DialogueManager : MonoBehaviour 
{
    [FormerlySerializedAs("_ui")]
    [Header("References")]
    [SerializeField] private DialogueUI ui;
    [FormerlySerializedAs("_activeGraph")] [SerializeField] private DialogueGraph activeGraph;
    [FormerlySerializedAs("_hintGraph")] [SerializeField] private DialogueGraph hintGraph;

    public event Action OnDialogueEnd;
    public event Action OnDialogueBegin;
    public event Action OnHintEnabled;

    private DialogueNode _currentNode;
    private int _currentNodeIndex;

    public void Start()
    {
        // Automatically start the dialogue if a graph is assigned
        if (activeGraph != null)
        {
            StartDialogue(activeGraph);
        }
    

        // Subscribe to UI events
        ui.OnNextRequested += HandleNextQuote;
        ui.OnSpecificPathRequested += HandleBranching;
    }
    
    /// <summary>
    /// Switches the active conversation to the hint graph.
    /// </summary>
    public void SetupHintDialogue() {
        if (hintGraph == null) return;

        OnHintEnabled?.Invoke();
        StartDialogue(hintGraph);
    }

    /// <summary>
    /// Advances the dialogue to the next sequential node.
    /// </summary>
    private void HandleNextQuote()
    {
        _currentNodeIndex++;

        if (_currentNodeIndex < activeGraph.nodes.Count)
        {
            SetNode(_currentNodeIndex);

        }
        else
        {
            EndDialogue();
        }
    }

    /// <summary>
    /// Jumps to a specific node index based on the player's choice.
    /// </summary>
    /// <param name="branchIndex">The ID of the chosen answer (1, 2, or 3).</param>
    private void HandleBranching(int branchIndex)
    {
        int targetIndex;

        switch (branchIndex)
        {
            case 1:
                targetIndex = _currentNode.firstOption;
                break;
            case 2:
                targetIndex = _currentNode.secondOption;
                break;
            case 3:
                targetIndex = _currentNode.thirdOption;
                break;
            default:
                CustomLog.LogEditorWarning($"[DialogueManager] Uncommon path selected! selectedPath: {branchIndex}");
                return;
        }

        // Check if the target node exists in the graph
        if (targetIndex >= 0 && targetIndex < activeGraph.nodes.Count)
        {
            SetNode(targetIndex);
        }
        else
        {
            CustomLog.LogEditorWarning($"[DialogueManager] Target index {targetIndex} is out of bounds. Ending dialogue.");
            EndDialogue();
        }
    }

    #region helpers

    /// <summary>
    /// Starts a conversation using the provided dialogue graph.
    /// </summary>
    private void StartDialogue(DialogueGraph graph)
    {
        if (graph == null || graph.nodes == null || graph.nodes.Count == 0)
        {
            CustomLog.LogEditorError($"[DialogueManager] Cannot start dialogue: Graph is null or empty!");
            return;
        }

        activeGraph = graph;
        OnDialogueBegin?.Invoke();
        SetNode(0);
    }
    /// <summary>
    /// Updates the current state and refreshes the UI.
    /// </summary>
    private void SetNode(int index)
    {
        _currentNodeIndex = index;
        _currentNode = activeGraph.nodes[_currentNodeIndex];
        ui.UpdateVisuals(_currentNode);
    }
    private void EndDialogue()
    {
        CustomLog.LogEditor("[DialogueManager] Dialogue sequence finished.");
        OnDialogueEnd?.Invoke();
        ui.StopAnimatingText();
    }

    public void SetActiveGraph(DialogueGraph g) {
        activeGraph = g;
    }
    #endregion


    private void OnDestroy()
    {
        if (ui == null) return;
        ui.OnNextRequested -= HandleNextQuote;
        ui.OnSpecificPathRequested -= HandleBranching;
    }
}
