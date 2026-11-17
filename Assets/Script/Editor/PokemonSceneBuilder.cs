#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UIElements;

//Builds or updates Assets/Scenes/PokemonScene.unity.
//  New scene: copied from the nature Scene.unity, the dancing GanzSe character is added next to the camp fire.
//  Existing scene: only the UI, controllers and rain are updated, so a hand-placed character keeps its position.
public static class PokemonSceneBuilder
{
    const string MenuPath = "Tools/建立或更新寶可夢場景（PokeAPI + 卡片時價）";
    const string SourceScene = "Assets/Free Low Poly Nature Pack/Scene/Scene.unity";
    const string TargetScene = "Assets/Scenes/PokemonScene.unity";
    const string UxmlPath = "Assets/UI Toolkit/PokemonScene.uxml";
    const string CharacterPrefab = "Assets/URP GanzSe Free Modular Character Pack/Prefabs/Modular Character/GanzSe Free Modular Character Update 1_1.prefab";
    const string AnimatorPath = "Assets/URP GanzSe Free Modular Character Pack/Animation/Character.controller";
    const string RainMaterialPath = "Assets/Materials/RainDrop.mat";
    const string CharacterName = "Trainer";

    [MenuItem(MenuPath)]
    public static void Build()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            return;

        bool isNew = AssetDatabase.LoadAssetAtPath<SceneAsset>(TargetScene) == null;
        if (isNew && !AssetDatabase.CopyAsset(SourceScene, TargetScene))
        {
            Debug.LogError("無法複製場景：" + SourceScene);
            return;
        }

        var scene = EditorSceneManager.OpenScene(TargetScene, OpenSceneMode.Single);

        EnvironmentManager envManager = Object.FindFirstObjectByType<EnvironmentManager>();
        NetworkManager networkManager = Object.FindFirstObjectByType<NetworkManager>();
        UIDocument uiDocument = Object.FindFirstObjectByType<UIDocument>();
        Camera cam = Camera.main;

        //1. Keep the initial environment, then remove the scripts this scene does not use:
        //   weather API, the left environment panel and the camera move buttons
        PokemonController pokemon = Object.FindFirstObjectByType<PokemonController>();
        EnvironmentData initialData = pokemon != null ? pokemon.initialData : null;
        foreach (EnvUIController e in Object.FindObjectsByType<EnvUIController>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (initialData == null) initialData = e.initialData;
            Object.DestroyImmediate(e);
        }
        foreach (WeatherController w in Object.FindObjectsByType<WeatherController>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            Object.DestroyImmediate(w);
        foreach (CameraUIController c in Object.FindObjectsByType<CameraUIController>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            Object.DestroyImmediate(c);

        //2. UI
        uiDocument.visualTreeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(UxmlPath);

        //3. Character (only created once)
        GameObject character = GameObject.Find(CharacterName);
        if (character == null)
        {
            character = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(CharacterPrefab), scene);
            character.name = CharacterName;
            pickOutfit(character);
            placeCharacter(character, envManager, cam);
        }
        Animator animator = character.GetComponentInChildren<Animator>();
        if (animator == null)
            animator = character.AddComponent<Animator>();
        if (animator.runtimeAnimatorController == null)
            animator.runtimeAnimatorController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(AnimatorPath);
        animator.applyRootMotion = false;

        //4. Rain (only created once)
        GameObject rainObject = GameObject.Find("Rain");
        ParticleSystem rain = rainObject != null ? rainObject.GetComponent<ParticleSystem>() : createRain(cam, envManager);

        //5. Controller
        if (pokemon == null)
            pokemon = new GameObject("PokemonController").AddComponent<PokemonController>();
        pokemon.uiDocument = uiDocument;
        pokemon.networkManager = networkManager;
        pokemon.envManager = envManager;
        pokemon.initialData = initialData;
        pokemon.character = character.transform;
        pokemon.characterAnimator = animator;
        pokemon.rain = rain;
        if (pokemon.typeLight == null)
            pokemon.typeLight = Object.FindObjectsByType<Light>(FindObjectsSortMode.None).FirstOrDefault(l => l.type == LightType.Point);
        EditorUtility.SetDirty(pokemon);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        addToBuildSettings();

        Selection.activeGameObject = pokemon.gameObject;
        Debug.Log((isNew ? "寶可夢場景已建立：" : "寶可夢場景已更新：") + TargetScene +
                  (initialData == null ? "\n（沒有找到 EnvironmentData，可在 PokemonController 的 Initial Data 指定）" : "") +
                  "\n雨量、價格門檻可以在 PokemonController 上調整。");
    }

    //A falling-streak particle system above the camera, used when the card is expensive
    static ParticleSystem createRain(Camera cam, EnvironmentManager envManager)
    {
        //How big the scene is from the camera's point of view
        float size = 20f;
        if (cam != null && envManager != null && envManager.campFire_Object != null)
            size = Mathf.Max(5f, Vector3.Distance(cam.transform.position, envManager.campFire_Object.transform.position));

        GameObject go = new GameObject("Rain");
        if (cam != null)
        {
            go.transform.SetParent(cam.transform, false);
            go.transform.localPosition = new Vector3(0f, size * 0.6f, size * 0.7f);
            go.transform.rotation = Quaternion.Euler(90f, 0f, 0f);  //emit straight down
        }

        ParticleSystem ps = go.AddComponent<ParticleSystem>();
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        var main = ps.main;
        main.playOnAwake = false;
        main.loop = true;
        main.startLifetime = 1.6f;
        main.startSpeed = size * 1.2f;
        main.startSize = size * 0.006f;
        main.startColor = new Color(0.75f, 0.8f, 0.9f, 0.6f);
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.maxParticles = 5000;

        var emission = ps.emission;
        emission.rateOverTime = 0f;

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = new Vector3(size * 2f, size * 1.5f, 0.1f);

        var renderer = go.GetComponent<ParticleSystemRenderer>();
        renderer.renderMode = ParticleSystemRenderMode.Stretch;
        renderer.lengthScale = 1f;
        renderer.velocityScale = 0.05f;
        renderer.sharedMaterial = getRainMaterial();

        return ps;
    }

    static Material getRainMaterial()
    {
        Material mat = AssetDatabase.LoadAssetAtPath<Material>(RainMaterialPath);
        if (mat != null)
            return mat;

        Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit") ?? Shader.Find("Particles/Standard Unlit");
        mat = new Material(shader);
        mat.SetColor("_BaseColor", new Color(0.8f, 0.85f, 0.95f, 0.5f));
        //Transparent surface so the drops look light
        if (mat.HasProperty("_Surface"))
        {
            mat.SetFloat("_Surface", 1f);
            mat.SetFloat("_Blend", 0f);
            mat.SetOverrideTag("RenderType", "Transparent");
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_ZWrite", 0);
            mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
        }

        Directory.CreateDirectory(Path.GetDirectoryName(RainMaterialPath));
        AssetDatabase.CreateAsset(mat, RainMaterialPath);
        return mat;
    }

    //Every part in the prefab is active; keep one item per category (the first one).
    //The helmet (HEADS) is hidden so the face and hair can be seen while dancing.
    static void pickOutfit(GameObject character)
    {
        foreach (string rootName in new[] { "ARMOR PARTS", "FACE DETAILS PARTS" })
        {
            Transform root = character.GetComponentsInChildren<Transform>(true).FirstOrDefault(t => t.name == rootName);
            if (root == null)
                continue;

            foreach (Transform category in root)
            {
                bool hide = category.name == "HEADS";
                for (int i = 0; i < category.childCount; i++)
                    category.GetChild(i).gameObject.SetActive(!hide && i == 0);
            }
        }
    }

    static void placeCharacter(GameObject character, EnvironmentManager envManager, Camera cam)
    {
        if (envManager == null || envManager.campFire_Object == null || cam == null)
            return;

        Vector3 fire = envManager.campFire_Object.transform.position;
        Vector3 toCam = cam.transform.position - fire;
        toCam.y = 0f;
        float distance = toCam.magnitude;
        Vector3 right = Vector3.Cross(Vector3.up, toCam.normalized);

        //Make the character about 30% of the camera distance tall, so it is clearly visible
        Bounds b = getBounds(character);
        if (b.size.y > 0.001f)
            character.transform.localScale *= distance * 0.3f / b.size.y;

        //Stand a bit to the side of the fire and slightly towards the camera
        Bounds fireBounds = getBounds(envManager.campFire_Object);
        float side = Mathf.Max(fireBounds.extents.x, distance * 0.08f) * 1.8f;
        character.transform.position = fire - right * side + toCam.normalized * distance * 0.1f;
        character.transform.rotation = Quaternion.LookRotation(toCam.normalized, Vector3.up);

        //Put the feet on the fire's ground level
        character.transform.position += Vector3.up * (fireBounds.min.y - getBounds(character).min.y);
    }

    static Bounds getBounds(GameObject go)
    {
        Renderer[] renderers = go.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0)
            return new Bounds(go.transform.position, Vector3.zero);
        Bounds b = renderers[0].bounds;
        foreach (Renderer r in renderers)
            b.Encapsulate(r.bounds);
        return b;
    }

    static void addToBuildSettings()
    {
        List<EditorBuildSettingsScene> scenes = EditorBuildSettings.scenes.ToList();
        if (scenes.Any(s => s.path == TargetScene))
            return;
        scenes.Add(new EditorBuildSettingsScene(TargetScene, true));
        EditorBuildSettings.scenes = scenes.ToArray();
    }
}
#endif
