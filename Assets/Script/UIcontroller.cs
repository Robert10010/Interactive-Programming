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
    public void onScaleValueChanged(float value)
    {
        this.characterGameObject.transform.localScale = new Vector3(value, value, value);
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
}
