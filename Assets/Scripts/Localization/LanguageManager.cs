using System.Collections;
using UnityEngine;
using UnityEngine.Localization.Settings;

public class LanguageManager : MonoBehaviour
{
    public static LanguageManager Instance { get; private set; }

    private const string PREFS_KEY = "SelectedLocaleCode";

    // This Script/ Object will be Singleton, so the translation is on all scenes needed
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private IEnumerator Start()
    {
        // wait until Localization is initialized
        yield return LocalizationSettings.InitializationOperation;
        LoadSavedLocale();
    }

    /// <summary>
    /// Change language with ISO-code ("en", "ru", "de")
    /// and save it to PlayerPrefs for next sessions
    /// </summary>
    private void SetLocale(string code)
    {
        StartCoroutine(SetLocaleCoroutine(code));
    }

    /// <summary>
    /// Change Language by Index
    /// </summary>
    public void SetLocaleByIndex(int index)
    {
        var locales = LocalizationSettings.AvailableLocales.Locales;
        if (index < 0 || index >= locales.Count) return;
        SetLocale(locales[index].Identifier.Code);
    }

    /// <summary>
    /// Change to the next available language (in the ring)
    /// </summary>
    public void CycleLocale()
    {
        var locales = LocalizationSettings.AvailableLocales.Locales;
        var current = locales.IndexOf(LocalizationSettings.SelectedLocale);
        var next = (current + 1) % locales.Count;
        SetLocale(locales[next].Identifier.Code);
    }

    private IEnumerator SetLocaleCoroutine(string code)
    {
        yield return LocalizationSettings.InitializationOperation;

        foreach (var locale in LocalizationSettings.AvailableLocales.Locales)
        {
            if (locale.Identifier.Code == code)
            {
                LocalizationSettings.SelectedLocale = locale;
                PlayerPrefs.SetString(PREFS_KEY, code);
                PlayerPrefs.Save();
                yield break;
            }
        }
        Debug.LogWarning($"[LanguageManager] Locale '{code}' not found.");
    }

    private void LoadSavedLocale()
    {
        if (PlayerPrefs.HasKey(PREFS_KEY))
        {
            var saved = PlayerPrefs.GetString(PREFS_KEY);
            StartCoroutine(SetLocaleCoroutine(saved));
        }
        else
        {
            // first time we check the local language of the system and set is as default
            AutoDetectLocale();
        }
    }

    private void AutoDetectLocale()
    {
        var code = Application.systemLanguage switch
        {
            SystemLanguage.Russian => "ru",
            SystemLanguage.English => "en",
            SystemLanguage.German => "de",
            _ => "en"
        };
        StartCoroutine(SetLocaleCoroutine(code));
    }
}