using System.Collections;
using UnityEngine;

public class RegisterFileVizualizer : BaseVizualizer
{

    private RegisterFieldPanelUI _uiController;
    public RegisterFieldPanelUI UIRegisterPanel => _uiController;

    [Header("Write Enable Visualization")]
    [Tooltip("Object that controls WE-signal and Stop-image")]
    [SerializeField] private GameObject _writeEnableIndicator;
    public bool isWriteEnabled;

    [Header("Blinker of sequential component")]
    [SerializeField] private Blinker _blinker;

    protected override void Awake()
    {
        base.Awake();

        // Set the initial/default data on the UI panel
        if (_uiController != null)
        {
            _uiController.Display(new int[16]);
        }

        // Set the initial state for STOP indicator
        if (_writeEnableIndicator != null)
        {
            _writeEnableIndicator.SetActive(false);
            isWriteEnabled = true;
            _uiController.WEButton.onClick.AddListener(SwitchWriteEnableVisualization);
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

    protected override void InitializePanelController()
    {
        _uiController = _panelInstance.GetComponent<RegisterFieldPanelUI>();
        if (_uiController == null)
        {
            Debug.LogError($"InfoPanelUI component not found on the prefab for {gameObject.name}!");
        }
    }

    public override void ResetVizualization()
    {
        throw new System.NotImplementedException();
    }

    public void TriggerBlink()
    {
        _blinker.Trigger();
    }
}
