using UnityEngine;
using UnityEngine.UIElements;

public class CameraUIController : MonoBehaviour
{
    public UIDocument uiDocument;
    public CameraController cameraController;

    Button forwardButton, backwardButton, leftButton, rightButton;

    void Start()
    {
        VisualElement root = uiDocument.rootVisualElement;

        forwardButton = root.Q<Button>("ForwardButton");
        forwardButton.clicked += cameraController.moveForward;

        backwardButton = root.Q<Button>("BackwardButton");
        backwardButton.clicked += cameraController.moveBackward;

        leftButton = root.Q<Button>("LeftButton");
        leftButton.clicked += cameraController.moveLeft;

        rightButton = root.Q<Button>("RightButton");
        rightButton.clicked += cameraController.moveRight;
    }
}
