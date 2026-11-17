using System;

//PokeAPI (no key needed): https://pokeapi.co/docs/v2#pokemon
//  https://pokeapi.co/api/v2/pokemon/{name or id}
[Serializable]
public class NamedApiResource
{
    public string name;
    public string url;
}

[Serializable]
public class PokemonTypeSlot
{
    public int slot;
    public NamedApiResource type;
}

[Serializable]
public class PokemonStat
{
    public int base_stat;
    public NamedApiResource stat;   //hp, attack, defense, special-attack, special-defense, speed
}

[Serializable]
public class PokemonSprites
{
    public string front_default;
    public string front_shiny;
}

[Serializable]
public class PokemonData
{
    public int id;
    public string name;
    public int height;  //decimetres (pikachu = 4 -> 0.4 m)
    public int weight;  //hectograms (pikachu = 60 -> 6.0 kg)
    public PokemonTypeSlot[] types;
    public PokemonStat[] stats;
    public PokemonSprites sprites;

    public string primaryType => types != null && types.Length > 0 ? types[0].type.name : "normal";

    public int getStat(string statName)
    {
        if (stats != null)
            foreach (PokemonStat s in stats)
                if (s.stat != null && s.stat.name == statName)
                    return s.base_stat;
        return 0;
    }
}

//TCGdex (no key needed): https://tcgdex.dev
//  Search: https://api.tcgdex.net/v2/en/cards?name=charizard  -> JSON array of TcgCardBrief
//  Card:   https://api.tcgdex.net/v2/en/cards/{id}            -> TcgCard (with market prices)
//  Image:  {image}/high.png
[Serializable]
public class TcgCardBrief
{
    public string id;
    public string name;
    public string image;    //missing on some cards
}

//JsonUtility cannot read a top-level array, so the array is wrapped as {"cards": [...]}
[Serializable]
public class TcgCardList
{
    public TcgCardBrief[] cards;
}

[Serializable]
public class TcgSet
{
    public string name;
}

[Serializable]
public class CardmarketPrice
{
    public string unit;     //EUR
    public float avg;
    public float low;
    public float trend;
}

[Serializable]
public class TcgplayerVariant
{
    public float marketPrice;
}

[Serializable]
public class TcgplayerPrice
{
    public string unit;     //USD
    public TcgplayerVariant normal;
    public TcgplayerVariant holofoil;
}

[Serializable]
public class TcgPricing
{
    public CardmarketPrice cardmarket;
    public TcgplayerPrice tcgplayer;
}

[Serializable]
public class TcgCard
{
    public string id;
    public string name;
    public string image;
    public string rarity;
    public TcgSet set;
    public TcgPricing pricing;

    //Pick the best price we have: Cardmarket trend, then average, then TCGplayer market price
    public bool tryGetPrice(out float price, out string unit)
    {
        price = 0f;
        unit = "";
        if (pricing == null)
            return false;

        CardmarketPrice cm = pricing.cardmarket;
        if (cm != null && (cm.trend > 0f || cm.avg > 0f))
        {
            price = cm.trend > 0f ? cm.trend : cm.avg;
            unit = string.IsNullOrEmpty(cm.unit) ? "EUR" : cm.unit;
            return true;
        }

        TcgplayerPrice tp = pricing.tcgplayer;
        if (tp != null)
        {
            float p = tp.holofoil != null && tp.holofoil.marketPrice > 0f ? tp.holofoil.marketPrice
                    : tp.normal != null ? tp.normal.marketPrice : 0f;
            if (p > 0f)
            {
                price = p;
                unit = string.IsNullOrEmpty(tp.unit) ? "USD" : tp.unit;
                return true;
            }
        }
        return false;
    }
}
