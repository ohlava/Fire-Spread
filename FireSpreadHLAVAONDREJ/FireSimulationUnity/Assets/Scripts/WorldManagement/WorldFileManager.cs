using System.IO;
using UnityEngine;

public static class WorldFileManager
{
    public static void SaveWorld(World world, string fullPath)
    {
        string json = GetWorldSerialized(world);
        string finalFilePath = fullPath;
        File.WriteAllText(finalFilePath, json);
    }

    public static World LoadWorld(string fullPath)
    {
        string finalFilePath = fullPath;

        if (File.Exists(finalFilePath))
        {
            string json = File.ReadAllText(finalFilePath);
            SerializableWorld serializableWorld = JsonUtility.FromJson<SerializableWorld>(json);
            if (serializableWorld != null && serializableWorld.Depth != 0 && serializableWorld.Width != 0)
            {
                // The JSON was valid and deserialized correctly.
                return SerializableConversion.ConvertFromWorldSerializable(serializableWorld);
            }
            else
            {
                Debug.LogError("Invalid JSON format.");
                return null;
            }
        }
        else
        {
            throw new FileNotFoundException($"Save file not found at {finalFilePath}");
        }
    }

    public static string GetWorldSerialized(World world)
    {
        SerializableWorld serializableWorld = SerializableConversion.ConvertToWorldSerializable(world);
        string json = JsonUtility.ToJson(serializableWorld);
        return json;
    }

    private static readonly object _fileLock = new object(); // This object doesn't have particular value. Its purpose is to serve as a token for the lock statement. Used as a mutex for synchronization purposes

    public static void AppendSimulationDataToFile(World world, Map<float> heatMap, string filePath)
    {
        SerializableWorld serializableWorld = SerializableConversion.ConvertToWorldSerializable(world);
        OutputData serializedHeatMap = SerializableConversion.ConvertMapToOutputData(heatMap);
        WorldAndHeatMapData worldAndHeatMapData = new WorldAndHeatMapData(serializableWorld, serializedHeatMap);
        string jsonData = JsonUtility.ToJson(worldAndHeatMapData);

        // Append the data to the file in a thread-safe manner
        lock (_fileLock)
        {
            using (StreamWriter file = new StreamWriter(filePath, true))
            {
                file.WriteLine(jsonData);
            }
        }
    }
}