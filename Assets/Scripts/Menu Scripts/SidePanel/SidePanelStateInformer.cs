using System.Collections.Generic;
using TMPro;
using UnityEngine;
using DG.Tweening;
using System.Runtime.CompilerServices;

public enum StateName { 
    FETCH = 0,
    DECODE = 1,
    MEM_ADRESS = 2,
    MEM_READ = 3,
    MEM_WB = 4,
    MEM_WRITE = 5,
    EXECUTE_R = 6,
    ALU_WB = 7,
    EXECUTE_I = 8,
    JAL = 9,
    BEQ = 10,
    UNKNOWN = 11,
}

public class SidePanelStateInformer : MonoBehaviour
{
    [Header("State Info")]
    [SerializeField] private List<ProcessStateInfo> stateList;

    [Header("UI elements")]
    [SerializeField] private TMP_Text _titelText;
    [SerializeField] private TMP_Text _stateDescription;
    [SerializeField] private TMP_Text _stateSignals;
    [SerializeField] private CanvasGroup _signalsGroup;
    [SerializeField] private CanvasGroup _buttonContainer;

    [SerializeField] private float _fadeDuration = 0.2f;

    public void SetStateInfo(int idx) {
        ProcessStateInfo s = stateList[idx];

        _titelText.text = s.titel;
        _stateDescription.text = s.stateInfo;

        if (s.doesHaveChoice) {
            SwitchUI(_buttonContainer, _signalsGroup);
        }
        else {
            _stateSignals.text = s.stateSignals;
            SwitchUI(_signalsGroup, _buttonContainer);
        }
    }

    private void SwitchUI(CanvasGroup toShow, CanvasGroup toHide)
    {
        toHide.DOFade(0, _fadeDuration);
        toHide.blocksRaycasts = false;
        toHide.interactable = false;

        toShow.DOFade(1, _fadeDuration);
        toShow.blocksRaycasts = true;
        toShow.interactable = true;
    }
}
