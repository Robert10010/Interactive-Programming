using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    [Header("UI Element Reference")]
    //Unity UI Elements
    public GameObject facePanel;
    public GameObject armorPanel;
    public TMP_Dropdown hairTypeDropDown, hairColorDropDown;
    public TMP_Dropdown faceHairTypeDropDown, faceHairColorDropDown;
    public TMP_Dropdown eyesTypeDropDown, eyesColorDropDown;
    public TMP_Dropdown eyebrowTypeDropDown, eyebrowColorDropDown;
    public TMP_Dropdown noseTypeDropDown;
    public TMP_Dropdown earTypeDropDown;
    public TMP_Dropdown headTypeDropDown, headColorDropDown;
    public TMP_Dropdown chestTypeDropDown, chestColorDropDown;
    public TMP_Dropdown armTypeDropDown, armColorDropDown;
    public TMP_Dropdown beltTypeDropDown, beltColorDropDown;
    public TMP_Dropdown legsTypeDropDown, legsColorDropDown;
    public TMP_Dropdown feetTypeDropDown, feetColorDropDown;
    public Slider scaleSlider;

    [Header("Character Reference")]
    //Character GameObject
    public GameObject hairGameObject;
    public GameObject faceHairGameObject;
    public GameObject eyesGameObject;
    public GameObject eyebrowGameObject;
    public GameObject noseGameObject;
    public GameObject earGameObject;
    public GameObject headGameObject;
    public GameObject chestGameObject;
    public GameObject armGameObject;
    public GameObject beltGameObject;
    public GameObject legsGameObject;
    public GameObject feetGameObject;
    public GameObject characterGameObject;

    [Header("Scene Object Reference")]
    public Transform cam_Transform;
    public Vector3 camPos1, camPos2;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        setupDropDownOptions();
        activateFirstItem();
        enableFacePanel();
        setupScaleSlider();
    }

    // Update is called once per frame
    void Update()
    {

    }

    void enableFacePanel()
    {
        facePanel.SetActive(true);
        armorPanel.SetActive(false);
    }
    void enableArmorPanel()
    {
        facePanel.SetActive(false);
        armorPanel.SetActive(true);
    }

    public void switchPanel()
    {
        if (facePanel.activeSelf)
        {
            enableArmorPanel();
        }

        else
        {
            enableFacePanel();
        }
            
    }

    public void onHairItemChanged()
    {
        int selectedIndex = hairTypeDropDown.value * hairColorDropDown.options.Count + hairColorDropDown.value;
        activateItem(hairGameObject, selectedIndex);
        
    }

    public void onFaceHairItemChanged()
    {
        int selectedIndex = faceHairTypeDropDown.value * faceHairColorDropDown.options.Count + faceHairColorDropDown.value;
        activateItem(faceHairGameObject, selectedIndex);
    }

    public void onEyesItemChanged()
    {
        int selectedIndex = eyesTypeDropDown.value * eyesColorDropDown.options.Count + eyesColorDropDown.value;
        activateItem(eyesGameObject, selectedIndex);
    }

    public void onEyebrowItemChanged()
    {
        int selectedIndex = eyebrowTypeDropDown.value * eyebrowColorDropDown.options.Count + eyebrowColorDropDown.value;
        activateItem(eyebrowGameObject, selectedIndex);
    }

    public void onNoseItemChanged()
    {
        int selectedIndex = noseTypeDropDown.value;
        activateItem(noseGameObject, selectedIndex);
    }

    public void onEarItemChanged()
    {
        int selectedIndex = earTypeDropDown.value;
        activateItem(earGameObject, selectedIndex);
    }

    public void onHeadItemChanged()
    {
        int selectedIndex = headTypeDropDown.value * headColorDropDown.options.Count + headColorDropDown.value;
        activateItem(headGameObject, selectedIndex);
    }

    public void onChestItemChanged()
    {
        int selectedIndex = chestTypeDropDown.value * chestColorDropDown.options.Count + chestColorDropDown.value;
        activateItem(chestGameObject, selectedIndex);
    }

    public void onArmItemChanged()
    {
        int selectedIndex = armTypeDropDown.value * armColorDropDown.options.Count + armColorDropDown.value;
        activateItem(armGameObject, selectedIndex);
    }

    public void onBeltItemChanged()
    {
        int selectedIndex = beltTypeDropDown.value * beltColorDropDown.options.Count + beltColorDropDown.value;
        activateItem(beltGameObject, selectedIndex);
    }

    public void onLegItemChanged()
    {
        int selectedIndex = legsTypeDropDown.value * legsColorDropDown.options.Count + legsColorDropDown.value;
        activateItem(legsGameObject, selectedIndex);
    }

    public void onFeetItemChanged()
    {
        int selectedIndex = feetTypeDropDown.value * feetColorDropDown.options.Count + feetColorDropDown.value;
        activateItem(feetGameObject, selectedIndex);
    }
    void setupScaleSlider()
    {
        if (scaleSlider == null || characterGameObject == null)
            return;

        scaleSlider.onValueChanged.AddListener(onScaleValueChanged);
        onScaleValueChanged(scaleSlider.value);
    }

    public void onScaleValueChanged(float value)
    {
        characterGameObject.transform.localScale = new Vector3(0.1f + value , 0.1f + value, 0.1f + value);
    }
    private void setupDropDownOptions()
    {
        setupTypeDropDown(hairTypeDropDown);
        setupColorDropDown(hairColorDropDown, 5);
        setupTypeDropDown(faceHairTypeDropDown);
        setupColorDropDown(faceHairColorDropDown, 5);
        setupTypeDropDown(eyesTypeDropDown);
        setupColorDropDown(eyesColorDropDown, 5);
        setupTypeDropDown(eyebrowTypeDropDown);
        setupColorDropDown(eyebrowColorDropDown, 5);
        setupTypeDropDown(noseTypeDropDown);
        setupTypeDropDown(earTypeDropDown);
        setupTypeDropDown(headTypeDropDown);
        setupColorDropDown(headColorDropDown, 3);
        setupTypeDropDown(chestTypeDropDown);
        setupColorDropDown(chestColorDropDown, 3);
        setupTypeDropDown(armTypeDropDown);
        setupColorDropDown(armColorDropDown, 3);
        setupTypeDropDown(beltTypeDropDown);
        setupColorDropDown(beltColorDropDown, 3);
        setupTypeDropDown(legsTypeDropDown);
        setupColorDropDown(legsColorDropDown, 3);
        setupTypeDropDown(feetTypeDropDown);
        setupColorDropDown(feetColorDropDown, 3);
    }

    private void setupTypeDropDown(TMP_Dropdown dropDown)
    {
        setDropDownOptions(dropDown, "Type", 5);
    }

    private void setupColorDropDown(TMP_Dropdown dropDown, int optionCount)
    {
        setDropDownOptions(dropDown, "Color", optionCount);
    }

    private void setDropDownOptions(TMP_Dropdown dropDown, string prefix, int optionCount)
    {
        if (dropDown == null)
            return;

        List<string> optionNames = new List<string>();
        for (int i = 1; i <= optionCount; i++)
            optionNames.Add(prefix + i);

        dropDown.ClearOptions();
        dropDown.AddOptions(optionNames);
        dropDown.value = 0;
        dropDown.RefreshShownValue();
    }

    private void activateFirstItem()
    {
        
    }
    private void activateItem(GameObject item, int index)
    {
        for(int i = 0; i < item.transform.childCount; i++)
        {
            if(i != index)
                item.transform.GetChild(i).gameObject.SetActive(false);
            else
                item.transform.GetChild(i).gameObject.SetActive(true);
        }
    }

    public CharacterOutfitSnapshot CaptureCurrentOutfit()
    {
        return new CharacterOutfitSnapshot
        {
            hairType = hairTypeDropDown != null ? hairTypeDropDown.value : 0,
            hairColor = hairColorDropDown != null ? hairColorDropDown.value : 0,
            faceHairType = faceHairTypeDropDown != null ? faceHairTypeDropDown.value : 0,
            faceHairColor = faceHairColorDropDown != null ? faceHairColorDropDown.value : 0,
            eyesType = eyesTypeDropDown != null ? eyesTypeDropDown.value : 0,
            eyesColor = eyesColorDropDown != null ? eyesColorDropDown.value : 0,
            eyebrowType = eyebrowTypeDropDown != null ? eyebrowTypeDropDown.value : 0,
            eyebrowColor = eyebrowColorDropDown != null ? eyebrowColorDropDown.value : 0,
            noseType = noseTypeDropDown != null ? noseTypeDropDown.value : 0,
            earType = earTypeDropDown != null ? earTypeDropDown.value : 0,
            headType = headTypeDropDown != null ? headTypeDropDown.value : 0,
            headColor = headColorDropDown != null ? headColorDropDown.value : 0,
            chestType = chestTypeDropDown != null ? chestTypeDropDown.value : 0,
            chestColor = chestColorDropDown != null ? chestColorDropDown.value : 0,
            armType = armTypeDropDown != null ? armTypeDropDown.value : 0,
            armColor = armColorDropDown != null ? armColorDropDown.value : 0,
            beltType = beltTypeDropDown != null ? beltTypeDropDown.value : 0,
            beltColor = beltColorDropDown != null ? beltColorDropDown.value : 0,
            legsType = legsTypeDropDown != null ? legsTypeDropDown.value : 0,
            legsColor = legsColorDropDown != null ? legsColorDropDown.value : 0,
            feetType = feetTypeDropDown != null ? feetTypeDropDown.value : 0,
            feetColor = feetColorDropDown != null ? feetColorDropDown.value : 0,
            scale = scaleSlider != null ? scaleSlider.value : 0f
        };
    }

    public void ApplyOutfit(CharacterOutfitSnapshot snapshot)
    {
        if (snapshot == null)
            return;

        SetDropDownValue(hairTypeDropDown, snapshot.hairType);
        SetDropDownValue(hairColorDropDown, snapshot.hairColor);
        onHairItemChanged();

        SetDropDownValue(faceHairTypeDropDown, snapshot.faceHairType);
        SetDropDownValue(faceHairColorDropDown, snapshot.faceHairColor);
        onFaceHairItemChanged();

        SetDropDownValue(eyesTypeDropDown, snapshot.eyesType);
        SetDropDownValue(eyesColorDropDown, snapshot.eyesColor);
        onEyesItemChanged();

        SetDropDownValue(eyebrowTypeDropDown, snapshot.eyebrowType);
        SetDropDownValue(eyebrowColorDropDown, snapshot.eyebrowColor);
        onEyebrowItemChanged();

        SetDropDownValue(noseTypeDropDown, snapshot.noseType);
        onNoseItemChanged();

        SetDropDownValue(earTypeDropDown, snapshot.earType);
        onEarItemChanged();

        SetDropDownValue(headTypeDropDown, snapshot.headType);
        SetDropDownValue(headColorDropDown, snapshot.headColor);
        onHeadItemChanged();

        SetDropDownValue(chestTypeDropDown, snapshot.chestType);
        SetDropDownValue(chestColorDropDown, snapshot.chestColor);
        onChestItemChanged();

        SetDropDownValue(armTypeDropDown, snapshot.armType);
        SetDropDownValue(armColorDropDown, snapshot.armColor);
        onArmItemChanged();

        SetDropDownValue(beltTypeDropDown, snapshot.beltType);
        SetDropDownValue(beltColorDropDown, snapshot.beltColor);
        onBeltItemChanged();

        SetDropDownValue(legsTypeDropDown, snapshot.legsType);
        SetDropDownValue(legsColorDropDown, snapshot.legsColor);
        onLegItemChanged();

        SetDropDownValue(feetTypeDropDown, snapshot.feetType);
        SetDropDownValue(feetColorDropDown, snapshot.feetColor);
        onFeetItemChanged();

        if (scaleSlider != null)
        {
            scaleSlider.SetValueWithoutNotify(snapshot.scale);
            onScaleValueChanged(snapshot.scale);
        }
    }

    static void SetDropDownValue(TMP_Dropdown dropDown, int value)
    {
        if (dropDown == null || dropDown.options == null || dropDown.options.Count == 0)
            return;

        int clamped = Mathf.Clamp(value, 0, dropDown.options.Count - 1);
        dropDown.SetValueWithoutNotify(clamped);
        dropDown.RefreshShownValue();
    }
}
