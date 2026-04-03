using UnityEngine;
using System.Collections;

public class IntructionDataMemoryVizualizer: BaseVizualizer
{
    private InstrMemoryControlPanel _uiController;
    public InstrMemoryControlPanel UIRegisterPanel => _uiController;

    [Header("Write Enable Visualization")]
    [Tooltip("Object that controls WE-signal and Stop-image")]
    [SerializeField] private GameObject _writeEnableIndicator;
    public bool isWriteEnabled;

    [Header("Blinker of sequential component")]
    [SerializeField] private Blinker _blinker;

    protected override void Awake()
    {
        base.Awake();

        // Set the initial state for STOP indicator
        if (_writeEnableIndicator != null)
        {
            _writeEnableIndicator.SetActive(false);
            isWriteEnabled = true;
            _uiController.WEButton.onClick.AddListener(SwitchWriteEnableVisualization);
        }
    }

    /// <summary>
    /// Concrete implementation of the base class initialization. 
    /// Retrieves the InfoPanelUI component from the instantiated UI prefab.
    /// </summary>
    protected override void InitializePanelController()
    {
        _uiController = _panelInstance.GetComponent<InstrMemoryControlPanel>();
        if (_uiController == null)
        {
            Debug.LogError($"InstrMemoryControlPanel component not found on the prefab for {gameObject.name}!");
        }
    }
    public void SwitchWriteEnableVisualization()
    {
        if (_writeEnableIndicator != null)
        {
            // if WE is true -> indicator must be inactive
            // if WE is false -> indicator must be active
            isWriteEnabled = !isWriteEnabled;
            _writeEnableIndicator.SetActive(!isWriteEnabled);
            HideData();
        }
    }
    public void ForceUpdateWriteEnableVisualization(bool flag)
    {
        if (_writeEnableIndicator != null)
        {
            isWriteEnabled = flag;
            _writeEnableIndicator.SetActive(!isWriteEnabled);
        }
    }

    public void TriggerBlink() {
        _blinker.Trigger();
    }

    public override void ResetVizualization() { }
}
