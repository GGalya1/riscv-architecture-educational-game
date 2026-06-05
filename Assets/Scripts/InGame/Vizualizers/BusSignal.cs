using UnityEngine;

public class BusSignal : BaseVisualizer
{
    public InfoPanelUI UIRegisterPanel { get; private set; }

    protected override void Awake()
    {
        // panelLocalOffset = new Vector3(0, 0.8f, 0);

        base.Awake();

        if (UIRegisterPanel != null)
        {
            UIRegisterPanel.Display("", "N/A");
        }
    }

    public override void ResetVisualisation()
    {
        throw new System.NotImplementedException();
    }

    protected override void InitializePanelController()
    {
        UIRegisterPanel = panelInstance.GetComponent<InfoPanelUI>();
        if (UIRegisterPanel == null)
        {
            Debug.LogError($"InfoPanelUI component not found on the prefab for {gameObject.name}!");
        }
    }

    public override void SetInteractable(bool value)
    {
        // nothing to do here
    }
}