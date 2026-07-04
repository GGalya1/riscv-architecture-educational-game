using UnityEngine;

public enum ProcessorType
{
    FullProcessor,  // Mehrtaktprozessor
    OneTick         // Eintaktprozessor
}

public class ProcessorLevelLoader : MonoBehaviour
{
    [SerializeField] private ProcessorInitialState levelData;
    [SerializeField] private OneTickProcessorInitialState oneTickLevelData;
    [SerializeField] private MainMenu menuController;

    [Tooltip("Mehrtaktprozessor = FullProcessor, Eintaktprozessor = OneTick")]
    [SerializeField] private ProcessorType processorType = ProcessorType.FullProcessor;
    
    public void OpenProcessorLevel(int idx)
    {
        switch (processorType)
        {
            case ProcessorType.OneTick:
                OneTickRegisseur.Initial = oneTickLevelData;
                break;
 
            case ProcessorType.FullProcessor:
            default:
                FullProcessorRegisseur.Initial = levelData;
                break;
        }

        menuController.OpenLevel(idx);
    }
}
