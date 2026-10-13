using TMPro;
using UnityEngine;

public static class OutfitSaveUiFont
{
    const string SilverFontPath = "Assets/font/Silver.asset";

    public static TMP_FontAsset Resolve(UIController characterUI)
    {
        if (characterUI != null &&
            characterUI.hairTypeDropDown != null &&
            characterUI.hairTypeDropDown.captionText != null &&
            characterUI.hairTypeDropDown.captionText.font != null &&
            !IsLiberationSans(characterUI.hairTypeDropDown.captionText.font))
        {
            return characterUI.hairTypeDropDown.captionText.font;
        }

        var canvas = Object.FindFirstObjectByType<Canvas>();
        if (canvas != null)
        {
            foreach (var tmp in canvas.GetComponentsInChildren<TextMeshProUGUI>(true))
            {
                if (tmp.font != null && !IsLiberationSans(tmp.font))
                    return tmp.font;
            }
        }

#if UNITY_EDITOR
        var silver = UnityEditor.AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(SilverFontPath);
        if (silver != null)
            return silver;
#endif

        return null;
    }

    public static void ApplyButtonLabel(GameObject buttonRoot, string text, TMP_FontAsset font)
    {
        if (buttonRoot == null)
            return;

        foreach (var legacy in buttonRoot.GetComponentsInChildren<UnityEngine.UI.Text>(true))
        {
            if (Application.isPlaying)
                Object.Destroy(legacy.gameObject);
            else
                Object.DestroyImmediate(legacy.gameObject);
        }

        foreach (var tmp in buttonRoot.GetComponentsInChildren<TextMeshProUGUI>(true))
        {
            if (Application.isPlaying)
                Object.Destroy(tmp.gameObject);
            else
                Object.DestroyImmediate(tmp.gameObject);
        }

        var labelGo = new GameObject("Text (TMP)", typeof(RectTransform), typeof(TextMeshProUGUI));
        labelGo.transform.SetParent(buttonRoot.transform, false);

        var rect = labelGo.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        var label = labelGo.GetComponent<TextMeshProUGUI>();
        if (font != null)
            label.font = font;
        label.alignment = TextAlignmentOptions.Center;
        label.fontSize = 18f;
        label.text = text;
        label.raycastTarget = false;

        var button = buttonRoot.GetComponent<UnityEngine.UI.Button>();
        if (button != null)
            button.targetGraphic = buttonRoot.GetComponent<UnityEngine.UI.Graphic>();
    }

    static bool IsLiberationSans(TMP_FontAsset font)
    {
        return font != null && font.name.Contains("LiberationSans");
    }
}
