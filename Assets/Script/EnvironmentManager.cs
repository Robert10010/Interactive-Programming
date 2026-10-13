using UnityEngine;
using System.Collections.Generic;

public class EnvironmentManager : MonoBehaviour
{
    [Header("Reference to environment data")]
    public GameObject directionalLight;
    public List<GameObject> rocks = new List<GameObject>();
    public List<GameObject> grass = new List<GameObject>();
    public List<GameObject> flowers = new List<GameObject>();
    public List<GameObject> woods = new List<GameObject>();
    public List<GameObject> plants = new List<GameObject>();
    public List<GameObject> fence = new List<GameObject>();
    public GameObject campFire_Object;
    public GameObject campFireParticle;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void activateRocks(bool v){
        for(int i = 0 ; i < rocks.Count ; i++){
            rocks[i].SetActive(v);
        }
    }
    public void activateGrass(bool v){
        for(int i = 0 ; i < grass.Count ; i++){
            grass[i].SetActive(v);
        }
    }
    public void activateFlowers(bool v){
        for(int i = 0 ; i < flowers.Count ; i++){
            flowers[i].SetActive(v);
        }
    }
    public void activateWoods(bool v){
        for(int i = 0 ; i < woods.Count ; i++){
            woods[i].SetActive(v);
        }
    }
    public void activatePlants(bool v){
        for(int i = 0 ; i < plants.Count ; i++){
            plants[i].SetActive(v);
        }
    }
    public void activateFence(bool v){
        for(int i = 0 ; i < fence.Count ; i++){
            fence[i].SetActive(v);
        }
    }
    public void setCampFireSize(float scale){
        campFire_Object.transform.localScale = Vector3.one * scale;
    }
    public void setCampFireIntensity(float vlaue){
        campFireParticle.transform.localScale = Vector3.one * vlaue;
    }
    public void setSunLightIntensity(float value){
        directionalLight.GetComponent<Light>().intensity = value;
    }

    public void setSunLightRotation(float angle){
        Vector3 rot = directionalLight.transform.localEulerAngles;
        rot.y = angle;
        directionalLight.transform.localEulerAngles = rot;
    }

    public void setSkyboxTheme(string option){
        if(string.Compare(option, "Day") == 0){
            RenderSettings.skybox.SetFloat("_AtmosphereThickness", 1);
        }
        else if(string.Compare(option, "Sunset") == 0){
            RenderSettings.skybox.SetFloat("_AtmosphereThickness", 2.2f);
        }
        else if(string.Compare(option, "Night") == 0){
            RenderSettings.skybox.SetFloat("_AtmosphereThickness", 0.2f);
        }
    }
}
