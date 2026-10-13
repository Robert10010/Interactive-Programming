using System;
using System.Collections.Generic;

[Serializable]
public class CharacterOutfitSnapshot
{
    public int hairType;
    public int hairColor;
    public int faceHairType;
    public int faceHairColor;
    public int eyesType;
    public int eyesColor;
    public int eyebrowType;
    public int eyebrowColor;
    public int noseType;
    public int earType;
    public int headType;
    public int headColor;
    public int chestType;
    public int chestColor;
    public int armType;
    public int armColor;
    public int beltType;
    public int beltColor;
    public int legsType;
    public int legsColor;
    public int feetType;
    public int feetColor;
    public float scale;
}

[Serializable]
public class OutfitSaveEntry
{
    public string id;
    public string displayName;
    public string savedUtc;
    public CharacterOutfitSnapshot snapshot;
}

[Serializable]
public class OutfitSaveCollection
{
    public List<OutfitSaveEntry> entries = new List<OutfitSaveEntry>();
}
