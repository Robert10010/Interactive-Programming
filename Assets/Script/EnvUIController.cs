using UnityEngine;
using UnityEngine.UIElements;


public class EnvUIController : MonoBehaviour
{
    public UIDocument uiDocument;
    public EnvironmentManager envManager;
    VisualElement root;

    void Start()
    {
        root = uiDocument.rootVisualElement;
    }


}
