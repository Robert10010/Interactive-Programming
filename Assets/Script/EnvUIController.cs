using System.IO;
using UnityEngine;
using UnityEngine.UIElements;


public class EnvUIController : MonoBehaviour
{
    public UIDocument uiDocument;
    public EnvironmentManager envManager;
    public EnvironmentData initialData;
    VisualElement root;

    //Runtime copy, so experimenting never changes the initialData asset
    private EnvironmentData currentEnvData;
    public EnvironmentData CurrentEnvData => currentEnvData;

    Button applyBtn, saveBtn, loadBtn;
    TextField fileNameTextField;

    void Awake()
    {
        currentEnvData = ScriptableObject.CreateInstance<EnvironmentData>();
        if (initialData != null)
            currentEnvData.setData(initialData);
    }

    void Start()
    {
        root = uiDocument.rootVisualElement;

        //UI <-> Data: the bindings in the UXML read and write currentEnvData
        root.Q<VisualElement>("ControllPanel").dataSource = currentEnvData;

        //Data -> Scene: apply whenever a value changes (from the UI, a loaded file or the weather)
        currentEnvData.propertyChanged += (sender, evt) => envManager.applyEnvironmentData(currentEnvData);
        envManager.applyEnvironmentData(currentEnvData);

        applyBtn = root.Q<Button>("ApplyBtn");
        applyBtn.clicked += onApplyBtnClicked;

        saveBtn = root.Q<Button>("SaveBtn");
        saveBtn.clicked += onSaveBtnClicked;

        loadBtn = root.Q<Button>("LoadBtn");
        loadBtn.clicked += onLoadBtnClicked;

        fileNameTextField = root.Q<TextField>("FileNameTextField");
    }

    public void onApplyBtnClicked()
    {
        envManager.applyEnvironmentData(currentEnvData);
    }

    string getSavePath()
    {
        string folder = Path.Combine(Application.persistentDataPath, "EnvironmentSaves");
        Directory.CreateDirectory(folder);

        string fileName = fileNameTextField.value.Trim();
        if (fileName.Length == 0)
            fileName = "Environment.json";
        if (!fileName.EndsWith(".json"))
            fileName += ".json";

        return Path.Combine(folder, fileName);
    }

    public void onSaveBtnClicked()
    {
        string path = getSavePath();
        File.WriteAllText(path, currentEnvData.getSaveJson());
        Debug.Log("Saved to: " + path);
    }

    public void onLoadBtnClicked()
    {
        string path = getSavePath();
        if (!File.Exists(path))
        {
            Debug.LogWarning("Save file not found: " + path);
            return;
        }

        currentEnvData.loadFromJson(File.ReadAllText(path));
        envManager.applyEnvironmentData(currentEnvData);
        Debug.Log("Loaded from: " + path);
    }
}
