using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class RegisterFieldPanelUI : MonoBehaviour
{
    private const string LOG_PATTERN_PC = "r15 (pc):\n{0}";
    private const string LOG_PATTERN_REGULAR = "r{0}: {1}";

    [FormerlySerializedAs("_weButton")] [SerializeField]
    private Button weButton;

    public TextMeshProUGUI titleText;

    public TextMeshProUGUI[] registerText;
    public Button WeButton => weButton;

    public void Awake()
    {
        titleText.text = "Register Field";
    }

    public void Display(int[] values)
    {
        for (var i = 0; i < registerText.Length; i++)
            if (i == 15)
                registerText[i].SetText(LOG_PATTERN_PC, values[i]);
            else
                registerText[i].SetText(LOG_PATTERN_REGULAR, i, values[i]);
    }
}