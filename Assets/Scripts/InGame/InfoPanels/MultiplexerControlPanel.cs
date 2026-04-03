using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MultiplexerControlPanel: MonoBehaviour
{
    public TextMeshProUGUI titleText;
    [SerializeField] private Button _firstWayButton;
    [SerializeField] private Button _secondWayButton;
    [SerializeField] private Button _thirdWayButton;

    public Button FirstWayButton => _firstWayButton;
    public Button SecondWayButton => _secondWayButton;
    public Button ThirdWayButton => _thirdWayButton;

    public void Setup(bool firstButton, bool secondButton, bool thirdButton, string title)
    {
        if (_firstWayButton != null) _firstWayButton.gameObject.SetActive(firstButton);
        if (_secondWayButton != null) _secondWayButton.gameObject.SetActive(secondButton);
        if (_thirdWayButton != null) _thirdWayButton.gameObject.SetActive(thirdButton);

        titleText.text = title;
    }


}
