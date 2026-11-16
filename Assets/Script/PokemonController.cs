using System;
using System.Collections;
using UnityEngine;
using UIRandom = UnityEngine.Random;
using UnityEngine.UIElements;

//Looks up a Pokémon with PokeAPI, shows it in the UI and lets its data change the scene:
//  height      -> character size
//  speed stat  -> dance speed (Animator.speed)
//  type        -> environment theme (fire / water / grass ...) and light colour
//  weight      -> camp fire intensity
public class PokemonController : MonoBehaviour
{
    const string PokemonUrl = "https://pokeapi.co/api/v2/pokemon/";
    const int MaxPokemonId = 1025;

    public UIDocument uiDocument;
    public NetworkManager networkManager;
    public EnvUIController envUIController;

    [Header("Scene objects the data changes")]
    public Transform character;
    public Animator characterAnimator;
    public Light typeLight;

    [Header("Tuning")]
    public string defaultPokemon = "pikachu";
    public float minScale = 0.6f, maxScale = 1.8f;      //multiplier of the character's original size
    public float minAnimSpeed = 0.5f, maxAnimSpeed = 2.2f;
    public float scaleLerpSpeed = 3f;

    TextField searchField;
    Button searchButton, randomButton;
    VisualElement spriteImage;
    Label nameLabel, typeLabel, heightLabel, weightLabel, speedLabel, infoLabel;
    Toggle usePokemonToggle;

    PokemonData currentPokemon;
    Vector3 baseScale, targetScale;
    int requestVersion;     //ignores an older request if the user searches again quickly

    void Start()
    {
        VisualElement root = uiDocument.rootVisualElement;

        searchField = root.Q<TextField>("SearchField");
        searchButton = root.Q<Button>("SearchButton");
        randomButton = root.Q<Button>("RandomButton");
        spriteImage = root.Q<VisualElement>("SpriteImage");
        nameLabel = root.Q<Label>("NameLabel");
        typeLabel = root.Q<Label>("TypeLabel");
        heightLabel = root.Q<Label>("HeightLabel");
        weightLabel = root.Q<Label>("WeightLabel");
        speedLabel = root.Q<Label>("SpeedLabel");
        infoLabel = root.Q<Label>("InfoLabel");
        usePokemonToggle = root.Q<Toggle>("UsePokemonToggle");

        if (character != null)
            baseScale = targetScale = character.localScale;

        searchField.SetValueWithoutNotify(defaultPokemon);
        searchButton.clicked += search;
        randomButton.clicked += searchRandom;
        searchField.RegisterCallback<KeyDownEvent>(evt =>
        {
            if (evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.KeypadEnter)
                search();
        });
        usePokemonToggle.RegisterValueChangedCallback(evt =>
        {
            if (evt.newValue && currentPokemon != null)
                applyPokemon(currentPokemon);
            else if (!evt.newValue)
                resetCharacter();
        });

        search();
    }

    void Update()
    {
        //Grow / shrink smoothly instead of jumping to the new size
        if (character != null)
            character.localScale = Vector3.Lerp(character.localScale, targetScale, Time.deltaTime * scaleLerpSpeed);
    }

    public void search()
    {
        //PokeAPI names are lowercase with '-' instead of spaces (e.g. "mr-mime")
        string query = searchField.value.Trim().ToLowerInvariant().Replace(' ', '-');
        if (query.Length == 0)
        {
            infoLabel.text = "Type a Pokemon name or number.";
            return;
        }
        StartCoroutine(requestPokemon(query));
    }

    public void searchRandom()
    {
        string id = UIRandom.Range(1, MaxPokemonId + 1).ToString();
        searchField.SetValueWithoutNotify(id);
        StartCoroutine(requestPokemon(id));
    }

    IEnumerator requestPokemon(string query)
    {
        int version = ++requestVersion;
        infoLabel.text = "Searching \"" + query + "\"...";

        PokemonData data = null;
        string error = null;
        yield return networkManager.SendRequest(PokemonUrl + Uri.EscapeDataString(query),
            json => data = JsonUtility.FromJson<PokemonData>(json),
            e => error = e);
        if (version != requestVersion) yield break;

        if (data == null || data.id == 0)
        {
            infoLabel.text = error != null && error.Contains("404")
                ? "No Pokemon called \"" + query + "\"."
                : "PokeAPI error: " + error;
            yield break;
        }

        currentPokemon = data;
        showPokemon(data);
        if (usePokemonToggle.value)
            applyPokemon(data);

        //Sprite image
        if (data.sprites != null && !string.IsNullOrEmpty(data.sprites.front_default))
        {
            yield return networkManager.SendTextureRequest(data.sprites.front_default,
                tex =>
                {
                    tex.filterMode = FilterMode.Point;  //keep the pixel art sharp
                    spriteImage.style.backgroundImage = new StyleBackground(tex);
                },
                e => Debug.LogWarning("Sprite error: " + e));
        }
        else
        {
            spriteImage.style.backgroundImage = StyleKeyword.None;
        }

        if (version == requestVersion)
            infoLabel.text = "Updated " + DateTime.Now.ToString("HH:mm:ss");
    }

    void showPokemon(PokemonData p)
    {
        nameLabel.text = "#" + p.id + " " + capitalize(p.name);

        string types = "";
        if (p.types != null)
            foreach (PokemonTypeSlot t in p.types)
                types += (types.Length > 0 ? " / " : "") + t.type.name;
        typeLabel.text = "Type: " + types;

        heightLabel.text = "Height: " + (p.height / 10f).ToString("0.0") + " m";
        weightLabel.text = "Weight: " + (p.weight / 10f).ToString("0.0") + " kg";
        speedLabel.text = "Speed: " + p.getStat("speed");
    }

    //Data mapping: what the Pokémon means for our scene
    void applyPokemon(PokemonData p)
    {
        //Height -> character size (0.2 m Pokémon is small, 3 m+ is big)
        float heightT = Mathf.InverseLerp(2f, 30f, p.height);
        targetScale = baseScale * Mathf.Lerp(minScale, maxScale, heightT);

        //Speed stat -> dance speed
        if (characterAnimator != null)
            characterAnimator.speed = Mathf.Lerp(minAnimSpeed, maxAnimSpeed, Mathf.InverseLerp(20f, 150f, p.getStat("speed")));

        //Weight -> camp fire intensity (heavier, stronger fire)
        EnvironmentData env = envUIController.CurrentEnvData;
        env.FireIntensity = Mathf.Lerp(0.5f, 5f, Mathf.InverseLerp(10f, 2000f, p.weight));

        //Type -> environment theme
        applyType(p.primaryType, env);
        if (typeLight != null)
            typeLight.color = typeColor(p.primaryType);
    }

    void applyType(string type, EnvironmentData env)
    {
        //Start from a normal day, then each type changes what it cares about
        env.SkyTheme = "Day";
        env.SunIntensity = 1f;
        env.CampFire = true;
        env.FireSize = 1.5f;
        env.Rock = env.Grass = env.Flower = env.Plant = env.Wood = env.Fence = true;

        switch (type)
        {
            case "fire":
            case "dragon":
                env.SkyTheme = "Sunset";
                env.FireSize = 4f;
                break;
            case "water":
            case "ice":
                env.CampFire = false;   //water puts the fire out
                env.SunIntensity = 0.6f;
                break;
            case "grass":
            case "bug":
            case "fairy":
                env.FireSize = 0.8f;
                env.SunIntensity = 1.4f;
                break;
            case "rock":
            case "ground":
            case "steel":
                env.Flower = env.Plant = env.Grass = false;   //bare, rocky land
                break;
            case "ghost":
            case "dark":
            case "psychic":
            case "poison":
                env.SkyTheme = "Night";
                env.SunIntensity = 0.15f;
                break;
            case "electric":
                env.SunIntensity = 1.8f;
                break;
        }
    }

    void resetCharacter()
    {
        targetScale = baseScale;
        if (characterAnimator != null)
            characterAnimator.speed = 1f;
    }

    static Color typeColor(string type)
    {
        switch (type)
        {
            case "fire": return new Color(1f, 0.45f, 0.1f);
            case "water": return new Color(0.25f, 0.5f, 1f);
            case "grass": return new Color(0.35f, 0.85f, 0.3f);
            case "electric": return new Color(1f, 0.9f, 0.2f);
            case "ice": return new Color(0.6f, 0.9f, 1f);
            case "psychic": return new Color(1f, 0.35f, 0.6f);
            case "ghost": return new Color(0.45f, 0.35f, 0.75f);
            case "dark": return new Color(0.35f, 0.25f, 0.3f);
            case "poison": return new Color(0.65f, 0.3f, 0.7f);
            case "dragon": return new Color(0.45f, 0.3f, 1f);
            case "fairy": return new Color(1f, 0.6f, 0.85f);
            case "bug": return new Color(0.65f, 0.75f, 0.15f);
            case "rock": return new Color(0.7f, 0.6f, 0.35f);
            case "ground": return new Color(0.85f, 0.7f, 0.4f);
            case "steel": return new Color(0.7f, 0.7f, 0.8f);
            case "fighting": return new Color(0.8f, 0.25f, 0.2f);
            case "flying": return new Color(0.65f, 0.6f, 0.95f);
            default: return new Color(1f, 0.85f, 0.65f);
        }
    }

    static string capitalize(string s)
    {
        return string.IsNullOrEmpty(s) ? s : char.ToUpperInvariant(s[0]) + s.Substring(1);
    }
}
