using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Serialization;

public class AluControlPanel: MonoBehaviour
{
    public TextMeshProUGUI titleText;
    [FormerlySerializedAs("_firstOperationButton")] [SerializeField] private Button firstOperationButton;
    [FormerlySerializedAs("_secondOperationButton")] [SerializeField] private Button secondOperationButton;
    [FormerlySerializedAs("_thirdOperationButton")] [SerializeField] private Button thirdOperationButton;
    [FormerlySerializedAs("_fourthOperationButton")] [SerializeField] private Button fourthOperationButton;

    public Button FirstOperationButton => firstOperationButton;
    public Button SecondOperationButton => secondOperationButton;
    public Button ThirdOperationButton => thirdOperationButton;
    public Button FourthOperationButton => fourthOperationButton;

    public void Setup(string title)
    {
        titleText.text = title;
    }
}
