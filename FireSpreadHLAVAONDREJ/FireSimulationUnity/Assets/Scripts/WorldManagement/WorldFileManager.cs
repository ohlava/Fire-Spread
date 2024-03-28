using System.IO;
using UnityEngine;

public class WorldFileManager
{
    public void SaveWorld(World world, string fullPath)
    {
        string json = GetWorldSerialized(world);
        string finalFilePath = fullPath;
        File.WriteAllText(finalFilePath, json);
    }

    public World LoadWorld(string fullPath)
    {
        string finalFilePath = fullPath;

        if (File.Exists(finalFilePath))
        {
            string json = File.ReadAllText(finalFilePath);
            SerializableWorld serializableWorld = JsonUtility.FromJson<SerializableWorld>(json);
            if (serializableWorld != null)
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

    public string GetWorldSerialized(World world)
    {
        SerializableWorld serializableWorld = SerializableConversion.ConvertToWorldSerializable(world);
        string json = JsonUtility.ToJson(serializableWorld);
        return json;
    }
}