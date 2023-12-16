using System.IO;
using UnityEngine;

public class DataManager : MonoBehaviour
{
    // To your JSON files
    [SerializeField] private TextAsset[] initialJsonFiles; 

    private void Start()
    {
        InitializeData();
    }

    // Save all JSON files to application persistentDataPath - mainly tutorial worlds
    private void InitializeData()
    {
        foreach (TextAsset jsonFile in initialJsonFiles)
        {
            string filePath = Path.Combine(Application.persistentDataPath, jsonFile.name + ".json");

            if (!File.Exists(filePath))
            {
                File.WriteAllText(filePath, jsonFile.text);
            }
        }
    }
}