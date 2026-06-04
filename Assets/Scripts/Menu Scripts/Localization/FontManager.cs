using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class FontManager : MonoBehaviour
{
    [System.Serializable]
    public class FontGroup
    {
        public string groupName;
        public string tableCollectionName = "Font Table"; // table name
        public string entryName;                          // key, for example: "titanBoldFont"
        public List<TextMeshProUGUI> labels;
    }

    [SerializeField] private List<FontGroup> fontGroups;

    private void OnEnable()
    {
        LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;
        StartCoroutine(InitializeFonts());
    }

    private void OnDisable()
    {
        LocalizationSettings.SelectedLocaleChanged -= OnLocaleChanged;
    }

    private IEnumerator InitializeFonts()
    {
        // Waiting for the localization to initialize
        yield return LocalizationSettings.InitializationOperation;
        ApplyAllFonts(LocalizationSettings.SelectedLocale);
    }

    private void OnLocaleChanged(Locale locale)
    {
        ApplyAllFonts(locale);
    }

    private async void ApplyAllFonts(Locale locale)
    {
        var isRtl = locale.Identifier.Code == "ar";

        foreach (var group in fontGroups)
        {
            var op = LocalizationSettings.AssetDatabase.GetLocalizedAssetAsync<TMP_FontAsset>(
                group.tableCollectionName,
                group.entryName
            );

            await op.Task;

            if (op.Result == null)
            {
                Debug.LogWarning($"[FontManager] Font not found: {group.tableCollectionName}/{group.entryName}");
                continue;
            }

            foreach (var label in group.labels)
            {
                label.font = op.Result;
                label.isRightToLeftText = isRtl;
            }
        }
    }
}