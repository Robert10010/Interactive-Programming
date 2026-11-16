#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UIElements;

//Builds Assets/Scenes/PokemonScene.unity from the nature Scene.unity:
//copies the scene, swaps the UI to PokemonScene.uxml, replaces the weather API with PokeAPI
//and adds the dancing GanzSe character next to the camp fire.
public static class PokemonSceneBuilder
{
    const string MenuPath = "Tools/建立寶可夢場景（PokeAPI）";
    const string SourceScene = "Assets/Free Low Poly Nature Pack/Scene/Scene.unity";
    const string TargetScene = "Assets/Scenes/PokemonScene.unity";
    const string UxmlPath = "Assets/UI Toolkit/PokemonScene.uxml";
    const string CharacterPrefab = "Assets/URP GanzSe Free Modular Character Pack/Prefabs/Modular Character/GanzSe Free Modular Character Update 1_1.prefab";
    const string AnimatorPath = "Assets/URP GanzSe Free Modular Character Pack/Animation/Character.controller";

    [MenuItem(MenuPath)]
    public static void Build()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            return;

        if (AssetDatabase.LoadAssetAtPath<SceneAsset>(TargetScene) != null &&
            !EditorUtility.DisplayDialog("Pokemon Scene", TargetScene + " 已存在，要重新建立嗎？（會覆蓋）", "重新建立", "取消"))
            return;

        AssetDatabase.DeleteAsset(TargetScene);
        if (!AssetDatabase.CopyAsset(SourceScene, TargetScene))
        {
            Debug.LogError("無法複製場景：" + SourceScene);
            return;
        }

        var scene = EditorSceneManager.OpenScene(TargetScene, OpenSceneMode.Single);

        //1. Remove the weather API so it does not fight with PokeAPI over EnvironmentData
        foreach (WeatherController w in Object.FindObjectsByType<WeatherController>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            Object.DestroyImmediate(w);

        //2. Swap the UI
        UIDocument uiDocument = Object.FindFirstObjectByType<UIDocument>();
        uiDocument.visualTreeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(UxmlPath);

        NetworkManager networkManager = Object.FindFirstObjectByType<NetworkManager>();
        EnvUIController envUI = Object.FindFirstObjectByType<EnvUIController>();
        EnvironmentManager envManager = Object.FindFirstObjectByType<EnvironmentManager>();

        //3. Character next to the camp fire, facing the camera
        GameObject character = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(CharacterPrefab), scene);
        character.name = "Trainer";
        pickOutfit(character);
        placeCharacter(character, envManager, Camera.main);

        Animator animator = character.GetComponentInChildren<Animator>();
        if (animator == null)
            animator = character.AddComponent<Animator>();
        animator.runtimeAnimatorController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(AnimatorPath);
        animator.applyRootMotion = false;

        //4. Controller
        Light pointLight = Object.FindObjectsByType<Light>(FindObjectsSortMode.None).FirstOrDefault(l => l.type == LightType.Point);

        GameObject go = new GameObject("PokemonController");
        PokemonController pokemon = go.AddComponent<PokemonController>();
        pokemon.uiDocument = uiDocument;
        pokemon.networkManager = networkManager;
        pokemon.envUIController = envUI;
        pokemon.character = character.transform;
        pokemon.characterAnimator = animator;
        pokemon.typeLight = pointLight;

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        addToBuildSettings();

        Selection.activeGameObject = character;
        Debug.Log("寶可夢場景已建立：" + TargetScene +
                  (pointLight == null ? "（找不到 Point Light，屬性燈光顏色不會變）" : "") +
                  "\n角色位置 / 大小可以在 Trainer 物件上微調。");
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
