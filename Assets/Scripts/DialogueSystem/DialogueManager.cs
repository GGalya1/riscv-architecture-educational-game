using System;
using UnityEngine;

/// <summary>
/// Orchestrates the dialogue flow by connecting data (DialogueGraph) with the interface (DialogueUI).
/// Manages node transitions, branching logic, and dialogue events.
/// </summary>
public class DialogueManager : MonoBehaviour 
{
    [Header("References")]
    [SerializeField] private DialogueUI _ui;
    [SerializeField] private DialogueGraph _activeGraph;
    [SerializeField] private DialogueGraph _hintGraph;

    public event Action OnDialogueEnd;
    public event Action OnDialogueBegin;
    public event Action OnHintEnabled;

    private DialogueNode _currentNode;
    private int _currentNodeIndex;

    public void Start()
    {
        // Automatically start the dialogue if a graph is assigned
        if (_activeGraph != null)
        {
            StartDialogue(_activeGraph);
        }
    

        // Subscribe to UI events
        _ui.OnNextRequested += HandleNextQuote;
        _ui.OnSpecificPathRequested += HandleBranching;
    }
    
    /// <summary>
    /// Switches the active conversation to the hint graph.
    /// </summary>
    public void SetupHintDialogue() {
        if (_hintGraph == null) return;

        _currentNodeIndex = 0;

        OnHintEnabled?.Invoke();
        StartDialogue(_hintGraph);
    }

    /// <summary>
    /// Advances the dialogue to the next sequential node.
    /// </summary>
    private void HandleNextQuote()
    {
        _currentNodeIndex++;

        if (_currentNodeIndex < _activeGraph.Nodes.Count)
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
        int targetIndex = -1;

        if (branchIndex == 1)
        {
            targetIndex = _currentNode.firstOption;
        }
        else if (branchIndex == 2)
        {
            targetIndex = _currentNode.secondOption;
        }
        else if (branchIndex == 3)
        {
            targetIndex = _currentNode.thirdOption;
        }
        else
        {
            Debug.LogWarning($"[DialogueManager] Unkommon path selected! selectedPath: {branchIndex}");
            return;
        }

        // Check if the target node exists in the graph
        if (targetIndex >= 0 && targetIndex < _activeGraph.Nodes.Count)
        {
            SetNode(targetIndex);
        }
        else
        {
            Debug.LogWarning($"[DialogueManager] Target index {targetIndex} is out of bounds. Ending dialogue.");
            EndDialogue();
        }
    }

    #region helpers

    /// <summary>
    /// Starts a conversation using the provided dialogue graph.
    /// </summary>
    private void StartDialogue(DialogueGraph graph)
    {
        if (graph == null || graph.Nodes == null || graph.Nodes.Count == 0)
        {
            Debug.LogError($"[DialogueManager] Cannot start dialogue: Graph is null or empty!");
            return;
        }

        _activeGraph = graph;
        OnDialogueBegin?.Invoke();
        SetNode(0);
    }
    /// <summary>
    /// Updates the current state and refreshes the UI.
    /// </summary>
    private void SetNode(int index)
    {
        _currentNodeIndex = index;
        _currentNode = _activeGraph.Nodes[_currentNodeIndex];
        _ui.UpdateVisuals(_currentNode);
    }
    private void EndDialogue()
    {
        Debug.Log("[DialogueManager] Dialogue sequence finished.");
        OnDialogueEnd?.Invoke();
        _ui.StopAnimatingText();
    }

    public void SetActiveGraph(DialogueGraph g) {
        _activeGraph = g;
    }
    #endregion


    private void OnDestroy()
    {
        if (_ui != null) 
        {
            _ui.OnNextRequested -= HandleNextQuote;
            _ui.OnSpecificPathRequested -= HandleBranching;
        }
    }
}
