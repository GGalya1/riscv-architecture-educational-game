using UnityEngine;
using UnityEngine.Serialization;

public class InstructionDataMemoryVisualizer: BaseVisualizer
{
    public InstrMemoryControlPanel UIRegisterPanel { get; private set; }

    [FormerlySerializedAs("_writeEnableIndicator")]
    [Header("Write Enable Visualization")]
    [Tooltip("Object that controls WE-signal and Stop-image")]
    [SerializeField] private GameObject writeEnableIndicator;
    public bool isWriteEnabled;

    [FormerlySerializedAs("_blinker")]
    [Header("Blinker of sequential component")]
    [SerializeField] private Blinker blinker;

    private bool _isWriteEnableIndicatorNull;

    private void Start()
    {
        _isWriteEnableIndicatorNull = writeEnableIndicator == null;
    }

    protected override void Awake()
    {
        base.Awake();

        // Set the initial state for STOP indicator
        if (writeEnableIndicator != null)
        {
            writeEnableIndicator.SetActive(false);
            isWriteEnabled = true;
            UIRegisterPanel.WeButton.onClick.AddListener(SwitchWriteEnableVisualization);
        }
    }

    /// <summary>
    /// Concrete implementation of the base class initialization. 
    /// Retrieves the InfoPanelUI component from the instantiated UI prefab.
    /// </summary>
    protected override void InitializePanelController()
    {
        UIRegisterPanel = panelInstance.GetComponent<InstrMemoryControlPanel>();
        if (UIRegisterPanel == null)
        {
            Debug.LogError($"InstrMemoryControlPanel component not found on the prefab for {gameObject.name}!");
        }
    }

    public override void SetInteractable(bool value)
    {
        UIRegisterPanel.WeButton.interactable = value;
    }

    private void SwitchWriteEnableVisualization()
    {
        if (writeEnableIndicator == null) return;
        // if WriteEnable is true -> indicator must be inactive
        // if WriteEnable is false -> indicator must be active
        isWriteEnabled = !isWriteEnabled;
        writeEnableIndicator.SetActive(!isWriteEnabled);
        HideData();
    }
    public void ForceUpdateWriteEnableVisualization(bool flag)
    {
        if (_isWriteEnableIndicatorNull) return;
        isWriteEnabled = flag;
        writeEnableIndicator.SetActive(!isWriteEnabled);
    }

    public void TriggerBlink() {
        blinker.Trigger();
    }

    public override void ResetVisualisation() { }
}
