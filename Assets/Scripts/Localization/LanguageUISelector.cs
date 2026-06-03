using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

public class LanguageUISelector : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown dropdown;

    private IEnumerator Start()
    {
        yield return LocalizationSettings.InitializationOperation;

        var locales = LocalizationSettings.AvailableLocales.Locales;

        dropdown.ClearOptions();
        foreach (var locale in locales)
            dropdown.options.Add(new TMP_Dropdown.OptionData(locale.LocaleName));

        // Set the current language as a chosen one
        var current = LocalizationSettings.SelectedLocale;
        dropdown.SetValueWithoutNotify(locales.IndexOf(current));
        dropdown.RefreshShownValue();

        dropdown.onValueChanged.AddListener(LanguageManager.Instance.SetLocaleByIndex);

        // Change the choose if it was changed somewhere else
        LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;
    }

    private void OnDestroy()
    {
        LocalizationSettings.SelectedLocaleChanged -= OnLocaleChanged;
        dropdown.onValueChanged.RemoveListener(LanguageManager.Instance.SetLocaleByIndex);
    }

    private void OnLocaleChanged(Locale locale)
    {
        var locales = LocalizationSettings.AvailableLocales.Locales;
        dropdown.SetValueWithoutNotify(locales.IndexOf(locale));
        dropdown.RefreshShownValue();
    }
}