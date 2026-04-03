using UnityEngine;
using TMPro;

public class InfoPanelUI: MonoBehaviour
{
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI bodyText;

    public void Display(string title, string body) {
        titleText.text = title;
        bodyText.text = body;
    }
}
