using System.IO;
using UnityEngine;

[System.Serializable]
public class Settings
{
    public bool UseSimplifiedWorldVisualization;
    public bool SaveTerrainAutomatically;

    public Settings(bool useSimplifiedWorldVisualization, bool saveTerrainAutomatically)
    {
        UseSimplifiedWorldVisualization = useSimplifiedWorldVisualization;
        SaveTerrainAutomatically = saveTerrainAutomatically;
    }
}

// Manages loading and saving of Settings instances to a JSON file
public class SettingsManager
{
    private static string settingsPath = Path.Combine(Application.streamingAssetsPath, "settings.json");

    public static void SaveSettings(Settings settings)
    {
        string json = JsonUtility.ToJson(settings);
        File.WriteAllText(settingsPath, json);
    }

    public static Settings LoadSettings()
    {
        if (File.Exists(settingsPath))
        {
            string json = File.ReadAllText(settingsPath);
            Settings settings = JsonUtility.FromJson<Settings>(json);
            return settings;
        }
        else
        {
            Debug.LogWarning("Settings file not found, creating a new one with default values.");
            Settings defaultSettings = new Settings(false, false);
            SaveSettings(defaultSettings);
            return defaultSettings;
        }
    }
}