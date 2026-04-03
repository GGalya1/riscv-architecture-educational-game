using UnityEngine;

public class ProcessorLevelLoader : MonoBehaviour
{
    [SerializeField] private ProcessorInitialState levelData;
    [SerializeField] private MainMenu menuController;

    public void OpenProcessorLevel(int idx)
    {
        FullProcessorRegiseur._initial = levelData;

        menuController.OpenLevel(idx);
    }
}
