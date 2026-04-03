using UnityEngine;
using TMPro;

public class ALUVizualiser : BaseVizualizer
{
    [Header("Operations Renderers (Optional 3D Text/Objects)")]
    [SerializeField] protected GameObject _operationBanner;
    [SerializeField] protected TMP_Text _symbolForOperation;

    protected ALUControlPanel _uiController;
    public ALUControlPanel UIController => _uiController;

    protected int _operation;
    public int CurrentALUOperation => _operation;


    protected override void Awake()
    {
        base.Awake();

        if (_uiController != null)
        {
            _uiController.Setup("ALU");
        }

        _uiController.FirstOperationButton.onClick.AddListener(() => ChooseALUOperation(0));
        _uiController.SecondOperationButton.onClick.AddListener(() => ChooseALUOperation(1));
        _uiController.ThirdOperationButton.onClick.AddListener(() => ChooseALUOperation(2));
        _uiController.FourthOperationButton.onClick.AddListener(() => ChooseALUOperation(3));

        ResetVizualization();
    }
    protected override void InitializePanelController()
    {
        // Специфичная для этого класса инициализация контроллера
        _uiController = _panelInstance.GetComponent<ALUControlPanel>();
        if (_uiController == null)
        {
            Debug.LogError($"ALUControlPanel component not found on the prefab for {gameObject.name}!");
        }
    }

    public override void ResetVizualization(){
        if (_operationBanner.activeSelf) {
            _operationBanner.SetActive(false);
        }
    }

    public void ChooseALUOperation(int operation)
    {
        HideData();
        string symbol;
        switch (operation)
        {
            case 0:
                symbol = "+"; // ADD
                break;
            case 1:
                symbol = "-"; // SUBTRACT
                break;
            case 2:
                symbol = "&"; // MULTIPLY (или логическое И / AND)
                break;
            case 3:
                symbol = "|"; // DIVIDE (или логическое ИЛИ / OR)
                break;
            default:
                Debug.LogWarning($"ALU operation is not valid and is equal {operation}. Displaying '?'");
                symbol = "?";
                break;
        }

        _symbolForOperation.text = symbol;
        _operation = operation;

        if (!_operationBanner.activeSelf) {
            _operationBanner.SetActive(true);
        }
    }
}
