using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Serialization;

public class MultiplexerControlPanel: MonoBehaviour
{
    public TextMeshProUGUI titleText;
    [FormerlySerializedAs("_firstWayButton")] [SerializeField] private Button firstWayButton;
    [FormerlySerializedAs("_secondWayButton")] [SerializeField] private Button secondWayButton;
    [FormerlySerializedAs("_thirdWayButton")] [SerializeField] private Button thirdWayButton;

    public Button FirstWayButton => firstWayButton;
    public Button SecondWayButton => secondWayButton;
    public Button ThirdWayButton => thirdWayButton;

    public void Setup(bool firstButton, bool secondButton, bool thirdButton, string title)
    {
        if (firstWayButton != null) firstWayButton.gameObject.SetActive(firstButton);
        if (secondWayButton != null) secondWayButton.gameObject.SetActive(secondButton);
        if (thirdWayButton != null) thirdWayButton.gameObject.SetActive(thirdButton);

        titleText.text = title;
    }


}
