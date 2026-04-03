using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RegisterFieldPanelUI : MonoBehaviour
{
    [SerializeField] private Button _weButton;
    public Button WEButton => _weButton;

    public TextMeshProUGUI titleText;

    public TextMeshProUGUI[] registerText;

    public void Awake()
    {
        titleText.text = "Register Field";
    }

    public void Display(int[] values) {
        for (int i = 0; i < registerText.Length; i++) { 
            if (i == 15) {
                registerText[i].text = $"r{i} (pc):\n{values[i]}";
            }
            else
                registerText[i].text = $"r{i}: {values[i]}";
        }
    }
}
