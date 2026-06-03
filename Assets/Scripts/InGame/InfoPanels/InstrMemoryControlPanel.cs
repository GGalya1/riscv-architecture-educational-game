using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Serialization;

public class InstrMemoryControlPanel: InfoPanelUI
{
    [FormerlySerializedAs("_weButton")] [SerializeField] private Button weButton;
    public Button WeButton => weButton;

    [FormerlySerializedAs("firstAddresValue")] public TextMeshProUGUI firstAddressValue;
    [FormerlySerializedAs("secondAddresValue")] public TextMeshProUGUI secondAddressValue;
    [FormerlySerializedAs("thirdAddresValue")] public TextMeshProUGUI thirdAddressValue;
    [FormerlySerializedAs("fourthAddresValue")] public TextMeshProUGUI fourthAddressValue;

    public void Display(string firstVal, string secondVal, string thirdVal, string fourthVal)
    {
        firstAddressValue.text = firstVal;
        secondAddressValue.text = secondVal;
        thirdAddressValue.text = thirdVal;
        fourthAddressValue.text = fourthVal;
    }
}
