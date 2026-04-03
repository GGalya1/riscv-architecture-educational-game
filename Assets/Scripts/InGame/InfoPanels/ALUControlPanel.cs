using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ALUControlPanel: MonoBehaviour
{
    public TextMeshProUGUI titleText;
    [SerializeField] private Button _firstOperationButton;
    [SerializeField] private Button _secondOperationButton;
    [SerializeField] private Button _thirdOperationButton;
    [SerializeField] private Button _fourthOperationButton;

    public Button FirstOperationButton => _firstOperationButton;
    public Button SecondOperationButton => _secondOperationButton;
    public Button ThirdOperationButton => _thirdOperationButton;
    public Button FourthOperationButton => _fourthOperationButton;

    public void Setup(string title)
    {
        titleText.text = title;
    }
}
