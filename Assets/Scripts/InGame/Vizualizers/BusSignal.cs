using UnityEngine;

public class BusSignal : BaseVizualizer
{
    private InfoPanelUI _uiController;
    public InfoPanelUI UIRegisterPanel => _uiController;

    protected override void Awake()
    {
        // panelLocalOffset = new Vector3(0, 0.8f, 0);

        base.Awake();

        if (_uiController != null)
        {
            _uiController.Display("", "N/A");
        }
    }

    public override void ResetVizualization()
    {
        throw new System.NotImplementedException();
    }

    protected override void InitializePanelController()
    {
        _uiController = _panelInstance.GetComponent<InfoPanelUI>();
        if (_uiController == null)
        {
            Debug.LogError($"InfoPanelUI component not found on the prefab for {gameObject.name}!");
        }
    }
}