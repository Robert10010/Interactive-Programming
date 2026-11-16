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
