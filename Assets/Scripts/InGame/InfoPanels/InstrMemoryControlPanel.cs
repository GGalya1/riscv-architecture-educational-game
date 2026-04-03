using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InstrMemoryControlPanel: InfoPanelUI
{
    [SerializeField] private Button _weButton;
    public Button WEButton => _weButton;

    public TextMeshProUGUI firstAddresValue;
    public TextMeshProUGUI secondAddresValue;
    public TextMeshProUGUI thirdAddresValue;
    public TextMeshProUGUI fourthAddresValue;

    public void Display(string firstVal, string secondVal, string thirdVal, string fourthVal)
    {
        firstAddresValue.text = firstVal;
        secondAddresValue.text = secondVal;
        thirdAddresValue.text = thirdVal;
        fourthAddresValue.text = fourthVal;
    }
}
