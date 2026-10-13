using System;
using System.IO;
using UnityEngine;

public static class OutfitSaveService
{
    const string FileName = "equipment_outfit_saves.json";

    static OutfitSaveCollection cachedCollection;

    public static string SaveFilePath => Path.Combine(Application.persistentDataPath, FileName);

    public static OutfitSaveCollection Collection
    {
        get
        {
            if (cachedCollection == null)
                LoadFromDisk();
            return cachedCollection;
        }
    }

    public static void Reload()
    {
        cachedCollection = null;
        LoadFromDisk();
    }

    public static void AddEntry(string displayName, CharacterOutfitSnapshot snapshot)
    {
        var entry = new OutfitSaveEntry
        {
            id = Guid.NewGuid().ToString("N"),
            displayName = displayName,
            savedUtc = DateTime.UtcNow.ToString("o"),
            snapshot = snapshot
        };

        Collection.entries.Insert(0, entry);
        WriteToDisk();
    }

    public static bool TryGetEntry(string id, out OutfitSaveEntry entry)
    {
        entry = Collection.entries.Find(e => e.id == id);
        return entry != null;
    }

    public static void RemoveEntry(string id)
    {
        Collection.entries.RemoveAll(e => e.id == id);
        WriteToDisk();
    }

    public static void ClearAllEntries()
    {
        Collection.entries.Clear();
        WriteToDisk();
    }

    static void LoadFromDisk()
    {
        cachedCollection = new OutfitSaveCollection();

        if (!File.Exists(SaveFilePath))
            return;

        try
        {
            string json = File.ReadAllText(SaveFilePath);
            if (string.IsNullOrWhiteSpace(json))
                return;

            OutfitSaveCollection loaded = JsonUtility.FromJson<OutfitSaveCollection>(json);
            if (loaded?.entries != null)
                cachedCollection = loaded;
        }
        catch (Exception ex)
        {
            Debug.LogError($"讀取裝備存檔失敗：{ex.Message}\n路徑：{SaveFilePath}");
        }
    }

    static void WriteToDisk()
    {
        try
        {
            string directory = Path.GetDirectoryName(SaveFilePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            string json = JsonUtility.ToJson(Collection, true);
            File.WriteAllText(SaveFilePath, json);
        }
        catch (Exception ex)
        {
            Debug.LogError($"寫入裝備存檔失敗：{ex.Message}\n路徑：{SaveFilePath}");
        }
    }
}
