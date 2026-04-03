using TMPro;
using UnityEngine;
using static UnityEngine.Rendering.HDROutputUtils;

public class ExternderVizualizer : ALUVizualiser
{
    protected override void Awake()
    {
        base.Awake();

        if (_uiController != null)
        {
            _uiController.Setup("Extender");
        }

        _uiController.FirstOperationButton.onClick.AddListener(() => ChooseExtenderType(0));
        _uiController.SecondOperationButton.onClick.AddListener(() => ChooseExtenderType(1));
        _uiController.ThirdOperationButton.onClick.AddListener(() => ChooseExtenderType(2));
        _uiController.FourthOperationButton.onClick.AddListener(() => ChooseExtenderType(3));

        ResetVizualization();
    }
    protected override void InitializePanelController()
    {
        _uiController = _panelInstance.GetComponent<ALUControlPanel>();
        if (_uiController == null)
        {
            Debug.LogError($"ExtenderControlPanel component not found on the prefab for {gameObject.name}!");
        }
    }

    public void ChooseExtenderType(int operationType)
    {
        string symbol;
        switch (operationType)
        {
            case 0:
                symbol = "I"; // I-Type (lw=> immediate)
                break;
            case 1:
                symbol = "S"; // S-Type (store)
                break;
            case 2:
                symbol = "B"; // B-Type (branch)
                break;
            case 3:
                symbol = "J"; // J-Type (jump)
                break;
            default:
                Debug.LogWarning($"Extender operation is not valid and is equal {operationType}. Displaying '?'");
                symbol = "?";
                break;
        }

        _symbolForOperation.text = symbol;
        _operation = operationType;

        if (!_operationBanner.activeSelf)
        {
            _operationBanner.SetActive(true);
        }
    }
}
