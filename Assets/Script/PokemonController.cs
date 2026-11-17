using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

//Uses two Web APIs to change the scene:
//  PokeAPI -> Pokémon stats and sprite
//     height      -> character size
//     speed stat  -> dance speed
//     type        -> light colour, camp fire size
//     weight      -> camp fire intensity
//  TCGdex  -> trading card image and market price
//     price       -> mood: cheap = sunny and happy, expensive = rain and sad
public class PokemonController : MonoBehaviour
{
    const string PokemonUrl = "https://pokeapi.co/api/v2/pokemon/";
    const string CardSearchUrl = "https://api.tcgdex.net/v2/en/cards?name=";
    const string CardUrl = "https://api.tcgdex.net/v2/en/cards/";
    const int MaxPokemonId = 1025;
    const int MaxCards = 30;

    public UIDocument uiDocument;
    public NetworkManager networkManager;
    public EnvironmentManager envManager;
    public EnvironmentData initialData;

    [Header("Scene objects the data changes")]
    public Transform character;
    public Animator characterAnimator;
    public Light typeLight;
    public ParticleSystem rain;

    [Header("Tuning")]
    public string defaultPokemon = "pikachu";
    public float minScale = 0.6f, maxScale = 1.8f;      //multiplier of the character's original size
    public float scaleLerpSpeed = 3f;
    [Tooltip("At or below this price the scene is fully happy")]
    public float cheapPrice = 1f;
    [Tooltip("At or above this price the scene is fully sad and raining")]
    public float expensivePrice = 50f;
    public float maxRainRate = 2000f;

    TextField searchField;
    Button searchButton, randomButton;
    VisualElement spriteImage, cardImage;
    Label nameLabel, typeLabel, heightLabel, weightLabel, speedLabel;
    Label cardNameLabel, rarityLabel, priceLabel, moodLabel, infoLabel;
    DropdownField cardDropdown;
    Toggle usePokemonToggle;

    EnvironmentData env;
    PokemonData currentPokemon;
    TcgCardBrief[] cards = new TcgCardBrief[0];
    float mood = 0.5f;          //0 = happy (cheap), 1 = sad (expensive)
    bool hasPrice;
    float statDanceSpeed = 1f;
    Vector3 baseScale, targetScale;
    int pokemonVersion, cardVersion;    //ignore older requests when the user searches again quickly

    void Awake()
    {
        //Runtime copy, so the scene never changes the initialData asset
        env = ScriptableObject.CreateInstance<EnvironmentData>();
        if (initialData != null)
            env.setData(initialData);
    }

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
        cardDropdown = root.Q<DropdownField>("CardDropdown");
        cardImage = root.Q<VisualElement>("CardImage");
        cardNameLabel = root.Q<Label>("CardNameLabel");
        rarityLabel = root.Q<Label>("RarityLabel");
        priceLabel = root.Q<Label>("PriceLabel");
        moodLabel = root.Q<Label>("MoodLabel");
        infoLabel = root.Q<Label>("InfoLabel");
        usePokemonToggle = root.Q<Toggle>("UsePokemonToggle");

        if (character != null)
            baseScale = targetScale = character.localScale;
        if (rain != null)
            setRain(0f);

        searchField.SetValueWithoutNotify(defaultPokemon);
        searchButton.clicked += search;
        randomButton.clicked += searchRandom;
        searchField.RegisterCallback<KeyDownEvent>(evt =>
        {
            if (evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.KeypadEnter)
                search();
        });
        cardDropdown.RegisterValueChangedCallback(evt =>
        {
            int index = cardDropdown.index;
            if (index >= 0 && index < cards.Length)
                StartCoroutine(loadCard(cards[index].id));
        });
        usePokemonToggle.RegisterValueChangedCallback(evt => applyAll());

        envManager.applyEnvironmentData(env);
        search();
    }

    void Update()
    {
        //Grow / shrink smoothly instead of jumping to the new size
        if (character != null)
            character.localScale = Vector3.Lerp(character.localScale, targetScale, Time.deltaTime * scaleLerpSpeed);
    }

    // ---------- Search ----------

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
        string id = Random.Range(1, MaxPokemonId + 1).ToString();
        searchField.SetValueWithoutNotify(id);
        StartCoroutine(requestPokemon(id));
    }

    IEnumerator requestPokemon(string query)
    {
        int version = ++pokemonVersion;
        infoLabel.text = "Searching \"" + query + "\"...";

        //1. PokeAPI: stats
        PokemonData data = null;
        string error = null;
        yield return networkManager.SendRequest(PokemonUrl + Uri.EscapeDataString(query),
            json => data = JsonUtility.FromJson<PokemonData>(json),
            e => error = e);
        if (version != pokemonVersion) yield break;

        if (data == null || data.id == 0)
        {
            infoLabel.text = error != null && error.Contains("404")
                ? "No Pokemon called \"" + query + "\"."
                : "PokeAPI error: " + error;
            yield break;
        }

        currentPokemon = data;
        showPokemon(data);
        applyAll();

        //2. Sprite
        if (data.sprites != null && !string.IsNullOrEmpty(data.sprites.front_default))
            yield return networkManager.SendTextureRequest(data.sprites.front_default,
                tex =>
                {
                    tex.filterMode = FilterMode.Point;  //keep the pixel art sharp
                    spriteImage.style.backgroundImage = new StyleBackground(tex);
                },
                e => Debug.LogWarning("Sprite error: " + e));
        if (version != pokemonVersion) yield break;

        //3. TCGdex: trading cards of this Pokémon
        yield return requestCards(data.name.Replace('-', ' '));
    }

    IEnumerator requestCards(string pokemonName)
    {
        int version = pokemonVersion;
        infoLabel.text = "Looking for " + pokemonName + " cards...";

        TcgCardList list = null;
        string error = null;
        yield return networkManager.SendRequest(CardSearchUrl + Uri.EscapeDataString(pokemonName),
            json => list = JsonUtility.FromJson<TcgCardList>("{\"cards\":" + json + "}"),
            e => error = e);
        if (version != pokemonVersion) yield break;

        //Only cards with an image (the end of the list is mostly expensive promo cards, so keep the API order)
        List<TcgCardBrief> found = new List<TcgCardBrief>();
        if (list != null && list.cards != null)
            foreach (TcgCardBrief c in list.cards)
                if (found.Count < MaxCards && !string.IsNullOrEmpty(c.image))
                    found.Add(c);

        cards = found.ToArray();
        if (cards.Length == 0)
        {
            clearCard(error != null ? "Card API error: " + error : "No cards found for " + pokemonName + ".");
            yield break;
        }

        List<string> names = new List<string>();
        foreach (TcgCardBrief c in cards)
            names.Add(c.name + " (" + c.id + ")");
        cardDropdown.choices = names;
        cardDropdown.SetValueWithoutNotify(names[0]);

        yield return loadCard(cards[0].id);
    }

    IEnumerator loadCard(string cardId)
    {
        int version = ++cardVersion;
        infoLabel.text = "Loading card " + cardId + "...";

        TcgCard card = null;
        string error = null;
        yield return networkManager.SendRequest(CardUrl + Uri.EscapeDataString(cardId),
            json => card = JsonUtility.FromJson<TcgCard>(json),
            e => error = e);
        if (version != cardVersion) yield break;

        if (card == null || string.IsNullOrEmpty(card.id))
        {
            clearCard("Card API error: " + error);
            yield break;
        }

        //Price -> mood
        cardNameLabel.text = card.name + (card.set != null && !string.IsNullOrEmpty(card.set.name) ? " - " + card.set.name : "");
        rarityLabel.text = "Rarity: " + (string.IsNullOrEmpty(card.rarity) ? "-" : card.rarity);
        hasPrice = card.tryGetPrice(out float price, out string unit);
        if (hasPrice)
        {
            priceLabel.text = "Market price: " + price.ToString("0.00") + " " + unit;
            mood = Mathf.InverseLerp(Mathf.Log(cheapPrice), Mathf.Log(expensivePrice), Mathf.Log(Mathf.Max(price, 0.01f)));
        }
        else
        {
            priceLabel.text = "Market price: no data";
            mood = 0.5f;
        }
        applyAll();

        //Card image
        if (!string.IsNullOrEmpty(card.image))
            yield return networkManager.SendTextureRequest(card.image + "/high.png",
                tex => cardImage.style.backgroundImage = new StyleBackground(tex),
                e => Debug.LogWarning("Card image error: " + e));

        if (version == cardVersion)
            infoLabel.text = "Updated " + DateTime.Now.ToString("HH:mm:ss");
    }

    void clearCard(string message)
    {
        cardDropdown.choices = new List<string>();
        cardDropdown.SetValueWithoutNotify("");
        cardImage.style.backgroundImage = StyleKeyword.None;
        cardNameLabel.text = "-";
        rarityLabel.text = "Rarity: -";
        priceLabel.text = "Market price: -";
        hasPrice = false;
        mood = 0.5f;
        applyAll();
        infoLabel.text = message;
    }

    // ---------- UI ----------

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

    // ---------- Data -> scene ----------

    void applyAll()
    {
        if (!usePokemonToggle.value)
        {
            //Back to the original scene
            env.setData(initialData != null ? initialData : ScriptableObject.CreateInstance<EnvironmentData>());
            targetScale = baseScale;
            statDanceSpeed = 1f;
            setRain(0f);
            if (characterAnimator != null)
                characterAnimator.speed = 1f;
            moodLabel.text = "";
            envManager.applyEnvironmentData(env);
            return;
        }

        if (currentPokemon != null)
            applyPokemon(currentPokemon);
        applyMood();
        envManager.applyEnvironmentData(env);
    }

    void applyPokemon(PokemonData p)
    {
        //Height -> character size (0.2 m Pokémon is small, 3 m+ is big)
        targetScale = baseScale * Mathf.Lerp(minScale, maxScale, Mathf.InverseLerp(2f, 30f, p.height));

        //Speed stat -> dance speed
        statDanceSpeed = Mathf.Lerp(0.6f, 1.6f, Mathf.InverseLerp(20f, 150f, p.getStat("speed")));

        //Weight -> camp fire intensity, fire type -> bigger fire
        env.FireIntensity = Mathf.Lerp(0.5f, 5f, Mathf.InverseLerp(10f, 2000f, p.weight));
        env.FireSize = p.primaryType == "fire" || p.primaryType == "dragon" ? 4f : 1.5f;

        //Type -> light colour
        if (typeLight != null)
            typeLight.color = typeColor(p.primaryType);
    }

    //Card price -> how the whole scene feels
    void applyMood()
    {
        float sad = mood;

        //Sky and light: cheap is a bright sunny day, expensive is a dark, gloomy night
        env.SkyTheme = sad < 0.35f ? "Day" : sad < 0.7f ? "Sunset" : "Night";
        env.SunIntensity = Mathf.Lerp(1.6f, 0.15f, sad);

        //Happy: flowers bloom. Sad: flowers are gone
        env.Flower = sad < 0.5f;
        env.Plant = sad < 0.8f;
        env.Grass = true;

        //Rain starts in the expensive half and puts the camp fire out when it is heavy
        float rainAmount = Mathf.InverseLerp(0.5f, 1f, sad);
        setRain(rainAmount);
        env.CampFire = rainAmount < 0.6f;

        //Happy: fast, bouncy dance. Sad: slow, tired dance
        if (characterAnimator != null)
            characterAnimator.speed = statDanceSpeed * Mathf.Lerp(1.5f, 0.3f, sad);

        moodLabel.text = !hasPrice ? "Mood: -"
            : sad < 0.25f ? "So cheap! Everyone is happy!"
            : sad < 0.5f ? "Not bad, a nice day."
            : sad < 0.8f ? "Pricey... it starts to rain."
            : "Way too expensive... so sad.";
    }

    void setRain(float amount)
    {
        if (rain == null)
            return;

        ParticleSystem.EmissionModule emission = rain.emission;
        emission.rateOverTime = amount * maxRainRate;
        if (amount > 0f && !rain.isPlaying)
            rain.Play();
        else if (amount <= 0f && rain.isPlaying)
            rain.Stop(true, ParticleSystemStopBehavior.StopEmitting);
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
