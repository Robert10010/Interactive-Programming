#if UNITY_EDITOR
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
public static class OutfitSaveUIBuilder
{
    const string MenuPath = "Tools/建立服飾存檔 UI（Canvas Panel）";

    [InitializeOnLoadMethod]
    static void EnsureUiWhenEditorLoads()
    {
        EditorApplication.delayCall += TryAutoBuildIfMissing;
    }

    static void TryAutoBuildIfMissing()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
            return;

        Transform panel = FindPanel();
        if (panel == null)
            return;

        if (panel.Find("OutfitListButton") != null && panel.GetComponent<OutfitSaveUI>() != null)
            return;

        BuildInActiveScene(silent: true);
    }

    [MenuItem(MenuPath)]
    public static void BuildFromMenu()
    {
        BuildInActiveScene(silent: false);
    }

    static void BuildInActiveScene(bool silent)
    {
        Transform panel = FindPanel();
        if (panel == null)
        {
            if (!silent)
                EditorUtility.DisplayDialog("建立失敗", "Canvas 下找不到名為 Panel 的物件。", "確定");
            return;
        }

        UIController uiController = Object.FindFirstObjectByType<UIController>();
        TMP_FontAsset font = OutfitSaveUiFont.Resolve(uiController);

        RemoveOld(panel);

        Transform switchButton = panel.Find("SwitchButton");
        Button listButton = CreateToolbarButton(panel, switchButton, "OutfitListButton", "列表", font, mirrorSwitchPosition: true);
        GameObject savePanel = CreateSavePanel(panel, font, out Transform content, out ScrollRect scroll, out Button saveBtn, out Button closeBtn, out GameObject rowTemplate);

        OutfitSaveUI saveUi = panel.GetComponent<OutfitSaveUI>();
        if (saveUi == null)
            saveUi = panel.gameObject.AddComponent<OutfitSaveUI>();

        if (uiController != null)
        {
            var oldOnController = uiController.GetComponent<OutfitSaveUI>();
            if (oldOnController != null && oldOnController != saveUi)
                Object.DestroyImmediate(oldOnController);
        }

        saveUi.characterUI = uiController;
        saveUi.uiFont = font;
        saveUi.listButton = listButton;
        saveUi.savePanel = savePanel;
        saveUi.saveButton = saveBtn;
        saveUi.closeButton = closeBtn;
        saveUi.listContent = content;
        saveUi.listScrollRect = scroll;
        saveUi.listRowTemplate = rowTemplate;

        var panelImage = savePanel.GetComponent<Image>();
        if (panelImage != null)
            panelImage.raycastTarget = false;

        listButton.gameObject.SetActive(true);
        savePanel.SetActive(false);
        rowTemplate.SetActive(false);
        listButton.transform.SetAsLastSibling();

        EditorUtility.SetDirty(panel.gameObject);
        EditorSceneManager.MarkSceneDirty(panel.gameObject.scene);
        Selection.activeGameObject = listButton.gameObject;

        if (!silent)
            Debug.Log("已在 Canvas/Panel 建立服飾存檔 UI（編輯模式即可在 Hierarchy 看見，請 Ctrl+S 儲存場景）。");
    }

    static Transform FindPanel()
    {
        Canvas canvas = Object.FindFirstObjectByType<Canvas>();
        if (canvas == null)
            return null;

        return canvas.transform.Find("Panel");
    }

    static void RemoveOld(Transform panel)
    {
        DestroyIfExists(panel, "OutfitListButton");
        DestroyIfExists(panel, "OutfitSavePanel");
        DestroyIfExists(panel, "OutfitSaveControls");
    }

    static void DestroyIfExists(Transform parent, string name)
    {
        Transform child = parent.Find(name);
        if (child != null)
            Object.DestroyImmediate(child.gameObject);
    }

    static Button CreateToolbarButton(Transform parent, Transform reference, string name, string label, TMP_FontAsset font, bool mirrorSwitchPosition)
    {
        var go = DefaultControls.CreateButton(new DefaultControls.Resources());
        go.name = name;
        go.layer = parent.gameObject.layer;
        go.transform.SetParent(parent, false);

        var rect = go.GetComponent<RectTransform>();
        if (reference != null)
        {
            var refRect = reference.GetComponent<RectTransform>();
            rect.anchorMin = refRect.anchorMin;
            rect.anchorMax = refRect.anchorMax;
            rect.pivot = refRect.pivot;
            rect.sizeDelta = refRect.sizeDelta;
            rect.anchoredPosition = refRect.anchoredPosition;
            if (mirrorSwitchPosition)
                rect.anchoredPosition = new Vector2(Mathf.Abs(refRect.anchoredPosition.x), refRect.anchoredPosition.y);
        }

        OutfitSaveUiFont.ApplyButtonLabel(go, label, font);
        return go.GetComponent<Button>();
    }

    static GameObject CreateSavePanel(Transform panel, TMP_FontAsset font, out Transform content, out ScrollRect scrollRect, out Button saveButton, out Button closeButton, out GameObject rowTemplate)
    {
        var panelGo = new GameObject("OutfitSavePanel", typeof(RectTransform), typeof(Image));
        panelGo.layer = panel.gameObject.layer;
        panelGo.transform.SetParent(panel, false);
        var panelRect = panelGo.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0f, 0f);
        panelRect.anchorMax = new Vector2(1f, 1f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.offsetMin = new Vector2(0f, 0f);
        panelRect.offsetMax = new Vector2(0f, -120f);

        saveButton = CreateToolbarButton(panelGo.transform, null, "SaveButton", "儲存", font, false);
        var saveRect = saveButton.GetComponent<RectTransform>();
        saveRect.anchorMin = new Vector2(0.5f, 1f);
        saveRect.anchorMax = new Vector2(0.5f, 1f);
        saveRect.pivot = new Vector2(0.5f, 1f);
        saveRect.anchoredPosition = new Vector2(0f, -12f);
        saveRect.sizeDelta = new Vector2(120f, 32f);

        closeButton = CreateToolbarButton(panelGo.transform, null, "CloseButton", "清除", font, false);
        var closeRect = closeButton.GetComponent<RectTransform>();
        closeRect.anchorMin = new Vector2(1f, 1f);
        closeRect.anchorMax = new Vector2(1f, 1f);
        closeRect.pivot = new Vector2(1f, 1f);
        closeRect.anchoredPosition = new Vector2(-12f, -12f);
        closeRect.sizeDelta = new Vector2(80f, 32f);

        var scrollGo = DefaultControls.CreateScrollView(new DefaultControls.Resources());
        scrollGo.name = "OutfitSaveScrollView";
        scrollGo.transform.SetParent(panelGo.transform, false);
        var scrollRectTransform = scrollGo.GetComponent<RectTransform>();
        scrollRectTransform.anchorMin = new Vector2(0f, 0f);
        scrollRectTransform.anchorMax = new Vector2(1f, 1f);
        scrollRectTransform.offsetMin = new Vector2(16f, 16f);
        scrollRectTransform.offsetMax = new Vector2(-16f, -56f);

        scrollRect = scrollGo.GetComponent<ScrollRect>();
        scrollRect.horizontal = false;
        content = scrollRect.content;

        if (scrollRect.viewport != null)
        {
            var viewportGo = scrollRect.viewport.gameObject;
            var legacyMask = viewportGo.GetComponent<Mask>();
            if (legacyMask != null)
                Object.DestroyImmediate(legacyMask);
            if (viewportGo.GetComponent<RectMask2D>() == null)
                viewportGo.AddComponent<RectMask2D>();

            var viewportImage = viewportGo.GetComponent<Image>();
            if (viewportImage != null)
            {
                viewportImage.color = new Color(1f, 1f, 1f, 0.01f);
                viewportImage.raycastTarget = false;
            }
        }

        var layout = content.gameObject.AddComponent<VerticalLayoutGroup>();
        layout.spacing = 4f;
        layout.padding = new RectOffset(4, 4, 4, 4);
        layout.childControlWidth = true;
        layout.childForceExpandWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandHeight = false;

        var fitter = content.gameObject.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        rowTemplate = DefaultControls.CreateButton(new DefaultControls.Resources());
        rowTemplate.name = "OutfitListRowTemplate";
        rowTemplate.transform.SetParent(panelGo.transform, false);
        OutfitSaveUiFont.ApplyButtonLabel(rowTemplate, "服飾", font);
        OutfitSaveUI.ConfigureListRowRect(rowTemplate);
        OutfitSaveUI.ApplyTransparentListRowBackground(rowTemplate);
        OutfitSaveUI.StyleListRowLabel(rowTemplate.GetComponentInChildren<TextMeshProUGUI>(), font);
        rowTemplate.SetActive(false);

        return panelGo;
    }
}
#endif
