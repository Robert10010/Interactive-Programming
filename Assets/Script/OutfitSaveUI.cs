using System;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI 物件須在場景中（Canvas/Panel 下），由 Editor 選單建立並儲存場景，編輯模式即可看見。
/// </summary>
public class OutfitSaveUI : MonoBehaviour
{
    [Header("References")]
    public UIController characterUI;
    public TMP_FontAsset uiFont;

    [Header("Canvas Panel")]
    public Button listButton;
    public GameObject savePanel;
    public Button saveButton;
    public Button closeButton;
    public Transform listContent;
    public ScrollRect listScrollRect;
    public GameObject listRowTemplate;

    TMP_FontAsset resolvedFont;

    void Awake()
    {
        if (characterUI == null)
            characterUI = FindFirstObjectByType<UIController>();

        resolvedFont = uiFont != null ? uiFont : OutfitSaveUiFont.Resolve(characterUI);
    }

    void OnEnable()
    {
        WireButtons();
    }

    void Start()
    {
        if (!Application.isPlaying)
            return;

        OutfitSaveService.Reload();

        if (listRowTemplate != null)
            listRowTemplate.SetActive(false);

        EnsureListButtonReceivesClicks();

        if (savePanel != null)
            savePanel.SetActive(false);
    }

    void EnsureListButtonReceivesClicks()
    {
        if (listButton == null)
            return;

        listButton.transform.SetAsLastSibling();

        if (savePanel != null)
        {
            var panelImage = savePanel.GetComponent<Image>();
            if (panelImage != null)
                panelImage.raycastTarget = false;
        }

        foreach (var tmp in listButton.GetComponentsInChildren<TextMeshProUGUI>(true))
            tmp.raycastTarget = false;
    }

    void WireButtons()
    {
        if (listButton != null)
        {
            listButton.onClick.RemoveListener(OnListButtonClicked);
            listButton.onClick.AddListener(OnListButtonClicked);
        }

        if (saveButton != null)
        {
            saveButton.onClick.RemoveListener(OnSaveClicked);
            saveButton.onClick.AddListener(OnSaveClicked);
        }

        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(OnClearAllSavesClicked);
            closeButton.onClick.AddListener(OnClearAllSavesClicked);
        }
    }

    public void OnListButtonClicked()
    {
        if (savePanel == null)
            return;

        if (savePanel.activeSelf)
        {
            CloseSavePanel();
            return;
        }

        savePanel.SetActive(true);
        EnsureListButtonReceivesClicks();
        RefreshSaveList();
    }

    public void OnClearAllSavesClicked()
    {
        OutfitSaveService.ClearAllEntries();
        RefreshSaveList();
        Debug.Log("已清除所有服飾存檔。");
    }

    public void CloseSavePanel()
    {
        if (savePanel != null)
            savePanel.SetActive(false);

        EnsureListButtonReceivesClicks();
    }

    public void OnSaveClicked()
    {
        if (characterUI == null)
        {
            Debug.LogWarning("OutfitSaveUI：找不到 UIController。");
            return;
        }

        CharacterOutfitSnapshot snapshot = characterUI.CaptureCurrentOutfit();
        string displayName = $"服飾 {OutfitSaveService.Collection.entries.Count + 1}";
        OutfitSaveService.AddEntry(displayName, snapshot);
        RefreshSaveList();
        Debug.Log($"已儲存「{displayName}」→ {OutfitSaveService.SaveFilePath}");
    }

    public void RefreshSaveList()
    {
        if (!Application.isPlaying)
            return;

        if (listContent == null || listRowTemplate == null)
            return;

        for (int i = listContent.childCount - 1; i >= 0; i--)
            Destroy(listContent.GetChild(i).gameObject);

        var entries = OutfitSaveService.Collection.entries;
        for (int i = 0; i < entries.Count; i++)
            CreateListRow(entries[i]);

        EnsureScrollContentLayout();
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)listContent);

        if (listScrollRect != null)
            listScrollRect.verticalNormalizedPosition = 1f;
    }

    void EnsureScrollContentLayout()
    {
        if (listContent == null)
            return;

        var contentRect = (RectTransform)listContent;
        contentRect.anchorMin = new Vector2(0f, 1f);
        contentRect.anchorMax = new Vector2(1f, 1f);
        contentRect.pivot = new Vector2(0.5f, 1f);
        contentRect.anchoredPosition = Vector2.zero;
        contentRect.sizeDelta = Vector2.zero;

        var verticalLayout = listContent.GetComponent<VerticalLayoutGroup>();
        if (verticalLayout == null)
            verticalLayout = listContent.gameObject.AddComponent<VerticalLayoutGroup>();

        verticalLayout.spacing = 4f;
        verticalLayout.padding = new RectOffset(4, 4, 4, 4);
        verticalLayout.childAlignment = TextAnchor.UpperCenter;
        verticalLayout.childControlWidth = true;
        verticalLayout.childForceExpandWidth = true;
        verticalLayout.childControlHeight = true;
        verticalLayout.childForceExpandHeight = false;

        var fitter = listContent.GetComponent<ContentSizeFitter>();
        if (fitter == null)
            fitter = listContent.gameObject.AddComponent<ContentSizeFitter>();
        fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        var contentImage = listContent.GetComponent<Image>();
        if (contentImage != null)
            contentImage.color = new Color(1f, 1f, 1f, 0f);

        if (listScrollRect != null && listScrollRect.viewport != null)
        {
            FixScrollViewportForText(listScrollRect.viewport);
        }
    }

    static void FixScrollViewportForText(RectTransform viewport)
    {
        var viewportGo = viewport.gameObject;

        var legacyMask = viewportGo.GetComponent<Mask>();
        if (legacyMask != null)
        {
            if (Application.isPlaying)
                Destroy(legacyMask);
            else
                DestroyImmediate(legacyMask);
        }

        if (viewportGo.GetComponent<RectMask2D>() == null)
            viewportGo.AddComponent<RectMask2D>();

        var viewportImage = viewportGo.GetComponent<Image>();
        if (viewportImage != null)
        {
            viewportImage.color = new Color(1f, 1f, 1f, 0.01f);
            viewportImage.raycastTarget = false;
        }
    }

    public static void ConfigureListRowRect(GameObject row)
    {
        var rect = row.GetComponent<RectTransform>();
        if (rect == null)
            return;

        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(0f, 36f);

        var layoutElement = row.GetComponent<LayoutElement>();
        if (layoutElement == null)
            layoutElement = row.AddComponent<LayoutElement>();
        layoutElement.minHeight = 36f;
        layoutElement.preferredHeight = 36f;
        layoutElement.flexibleWidth = 1f;

        ApplyTransparentListRowBackground(row);
    }

    public static void ApplyTransparentListRowBackground(GameObject row)
    {
        var image = row.GetComponent<Image>();
        if (image == null)
            return;

        image.color = new Color(0f, 0f, 0f, 0.25f);
        image.raycastTarget = true;
    }

    public static void StyleListRowLabel(TextMeshProUGUI label, TMP_FontAsset font)
    {
        if (label == null)
            return;

        if (font != null)
        {
            label.font = font;
            label.fontSharedMaterial = font.material;
        }

        label.raycastTarget = false;
        label.enableVertexGradient = false;
        label.outlineWidth = 0f;
        label.fontStyle = FontStyles.Normal;
        label.fontSize = 20f;
        label.enableWordWrapping = false;
        label.alignment = TextAlignmentOptions.MidlineLeft;
        label.margin = new Vector4(10f, 0f, 10f, 0f);
        label.color = new Color32(255, 255, 255, 255);

        if (label.canvasRenderer != null)
            label.canvasRenderer.cullTransparentMesh = false;

        label.SetAllDirty();
        label.ForceMeshUpdate(true);
    }

    void CreateListRow(OutfitSaveEntry entry)
    {
        GameObject row = Instantiate(listRowTemplate, listContent);
        row.SetActive(true);
        row.name = $"OutfitRow_{entry.displayName}";
        ConfigureListRowRect(row);

        var label = row.GetComponentInChildren<TextMeshProUGUI>();
        if (label != null)
        {
            label.text = FormatEntryLabel(entry);
            StyleListRowLabel(label, resolvedFont);
        }

        var button = row.GetComponent<Button>();
        if (button == null)
            button = row.GetComponentInChildren<Button>();

        if (button != null)
        {
            string entryId = entry.id;
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => LoadOutfit(entryId));
        }
    }

    void LoadOutfit(string entryId)
    {
        if (!OutfitSaveService.TryGetEntry(entryId, out OutfitSaveEntry entry) || characterUI == null)
            return;

        characterUI.ApplyOutfit(entry.snapshot);
        Debug.Log($"已載入「{entry.displayName}」");
    }

    static string FormatEntryLabel(OutfitSaveEntry entry)
    {
        string timeText = entry.savedUtc;
        if (DateTime.TryParse(entry.savedUtc, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out DateTime savedTime))
            timeText = savedTime.ToLocalTime().ToString("yyyy/MM/dd HH:mm");

        return $"{entry.displayName}  ({timeText})";
    }
}
