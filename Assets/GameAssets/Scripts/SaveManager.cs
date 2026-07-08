using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    private const string saveName = "save.json";
    private static SaveManager instance;
    public static SaveData saveData;

    [Serializable]
    public struct SaveData
    {
        public HashSet<string> completedLevels;
    }

    [Serializable]
    private struct SaveDataJson
    {
        public List<string> completedLevels;
    }

    public static SaveManager Instance => instance;

    [ContextMenu("Save")]
    public static Task SaveGame()
    {
        var jsonData = new SaveDataJson
        {
            completedLevels = saveData.completedLevels.ToList(),
        };

        var filepath = Path.Combine(Application.persistentDataPath, saveName);
        var saveContents = JsonUtility.ToJson(jsonData, true);
        return File.WriteAllTextAsync(filepath, saveContents);
    }

    [ContextMenu("Load")]
    public static void LoadSave()
    {
        var filepath = Path.Combine(Application.persistentDataPath, saveName);
        if (!File.Exists(filepath))
        {
            throw new FileNotFoundException("Could not find save file", saveName);
        }

        var jsonData = JsonUtility.FromJson<SaveDataJson>(File.ReadAllText(filepath));

        saveData = new SaveData
        {
            completedLevels = new HashSet<string>(jsonData.completedLevels),
        };
    }

    public static int LevelsCompleted => saveData.completedLevels.Count;

    public static void MarkLevelCompleted(string levelName)
    {
        if (saveData.completedLevels.Contains(levelName)) return;
        saveData.completedLevels.Add(levelName);
        SaveGame();
    }

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        try { LoadSave(); }
        catch (FileNotFoundException)
        {
            saveData = new()
            {
                completedLevels = new HashSet<string>(),
            };
        }
    }
}