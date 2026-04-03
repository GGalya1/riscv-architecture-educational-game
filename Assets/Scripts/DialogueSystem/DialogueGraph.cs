using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Acts as a container for a collection of dialogue nodes.
/// Represents a complete dialogue tree or a specific conversation flow.
/// </summary>
[CreateAssetMenu(fileName = "NewDialogueGraph", menuName = "Dialogue System/Graph")]
public class DialogueGraph : ScriptableObject
{
    /// <summary>
    /// The list of all nodes belonging to this dialogue graph.
    /// The indices of these nodes are referenced by DialogueNode's for branching.
    /// </summary>
    [Tooltip("List of all dialogue steps in this specific conversation.")]
    public List<DialogueNode> Nodes;
}
