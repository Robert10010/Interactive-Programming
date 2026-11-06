using UnityEngine;
using UnityEngine.UIElements;


public class EnvUIController : MonoBehaviour
{
    public UIDocument uiDocument;
    public EnvironmentManager envManager;
    VisualElement root;

    Toggle rockToggle, grassToggle, flowerToggle, campFireToggle, fenceToggle, plantToggle, woodToggle;
    Slider fireSizeSlider, fireIntensitySlider, sunIntensitySlider, sunRotationSlider;
    DropdownField skyThemeDropdown;

    void Start()
    {
        root = uiDocument.rootVisualElement;

        //Toggle -> show / hide objects
        rockToggle = root.Q<Toggle>("Rock");
        rockToggle.RegisterValueChangedCallback(evt => envManager.activateRocks(evt.newValue));

        grassToggle = root.Q<Toggle>("Grass");
        grassToggle.RegisterValueChangedCallback(evt => envManager.activateGrass(evt.newValue));

        flowerToggle = root.Q<Toggle>("flower");
        flowerToggle.RegisterValueChangedCallback(evt => envManager.activateFlowers(evt.newValue));

        campFireToggle = root.Q<Toggle>("CampFire");
        campFireToggle.RegisterValueChangedCallback(evt => envManager.activateCampFire(evt.newValue));

        fenceToggle = root.Q<Toggle>("Fence");
        fenceToggle.RegisterValueChangedCallback(evt => envManager.activateFence(evt.newValue));

        plantToggle = root.Q<Toggle>("Plant");
        plantToggle.RegisterValueChangedCallback(evt => envManager.activatePlants(evt.newValue));

        woodToggle = root.Q<Toggle>("Wood");
        woodToggle.RegisterValueChangedCallback(evt => envManager.activateWoods(evt.newValue));

        //Slider -> change environment
        fireSizeSlider = root.Q<Slider>("FireSizeSlider");
        fireSizeSlider.RegisterValueChangedCallback(evt => envManager.setCampFireSize(evt.newValue));

        fireIntensitySlider = root.Q<Slider>("FireIntensitySlider");
        fireIntensitySlider.RegisterValueChangedCallback(evt => envManager.setCampFireIntensity(evt.newValue));

        sunIntensitySlider = root.Q<Slider>("SunIntensitySlider");
        sunIntensitySlider.RegisterValueChangedCallback(evt => envManager.setSunLightIntensity(evt.newValue));

        sunRotationSlider = root.Q<Slider>("SunRotationSlider");
        sunRotationSlider.RegisterValueChangedCallback(evt => envManager.setSunLightRotation(evt.newValue));

        //Dropdown -> change sky theme
        skyThemeDropdown = root.Q<DropdownField>("SkyThemeDropdown");
        skyThemeDropdown.RegisterValueChangedCallback(evt => envManager.setSkyboxTheme(evt.newValue));
    }
}
