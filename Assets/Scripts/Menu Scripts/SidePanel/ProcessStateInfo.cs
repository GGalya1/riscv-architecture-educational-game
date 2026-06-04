using UnityEngine;

[CreateAssetMenu(fileName = "ProcessStateInfo", menuName = "Scriptable Objects/ProcessStateInfo")]
public class ProcessStateInfo : ScriptableObject
{
    public string titel;

    [TextArea(3, 20)]
    public string stateInfo;

    [TextArea(3, 20)]
    public string stateSignals;

    public bool doesHaveChoice;
}
