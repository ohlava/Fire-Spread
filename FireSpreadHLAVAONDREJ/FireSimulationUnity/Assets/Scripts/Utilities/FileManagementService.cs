using System;
using System.IO;
using UnityEngine;

public class FileManagementService
{
    private FileBrowserHandler fileBrowserHandler;
    private IMapImporter<float> heightMapImporter;
    private WorldGenerator worldGenerator;

    public FileManagementService(
        FileBrowserHandler fileBrowserHandler,
        WorldGenerator worldGenerator)
    {
        this.fileBrowserHandler = fileBrowserHandler;
        this.heightMapImporter = new HeightMapImporter();
        this.worldGenerator = worldGenerator ?? throw new ArgumentNullException(nameof(worldGenerator));
    }

    // Initiates the file import process and handles the imported file.
    public void ImportFile(Action<World> onWorldImported)
    {
        fileBrowserHandler.ImportFile(FileImportedCallback);

        void FileImportedCallback(string filePath)
        {
            if (filePath != null)
            {
                Debug.Log("Loading file from path: " + filePath);
                World world = LoadWorldFromFile(filePath);
                if (world != null)
                {
                    onWorldImported?.Invoke(world);
                }
            }
            else
            {
                Debug.Log("File loading was canceled.");
            }
        }
    }

    // Loads a world from a file based on the file extension.
    private World LoadWorldFromFile(string filePath)
    {
        if (filePath != null)
        {
            string fileExtension = Path.GetExtension(filePath).ToLower();

            if (fileExtension == ".png" || fileExtension == ".jpg" || fileExtension == ".jpeg")
            {
                Debug.Log("Importing height map from " + fileExtension + " file.");

                int requiredWidth = worldGenerator.width;
                int requiredDepth = worldGenerator.depth;

                Map<float> customHeightMap = heightMapImporter.GetMap(requiredWidth, requiredDepth, filePath);

                // Get and apply other maps differently if desired. Smooth, GaussianBlur and other MapExtensions methods.

                Map<int> customMoistureMap = new Map<int>(requiredWidth, requiredDepth);
                customMoistureMap.FillWithDefault(0);

                Map<VegetationType> customVegetationMap = new Map<VegetationType>(requiredWidth, requiredDepth);
                customVegetationMap.FillWithDefault(VegetationType.Grass);

                World world = WorldBuilder.CreateWorld(requiredWidth, requiredDepth, customHeightMap, customMoistureMap, customVegetationMap);

                return world;
            }
            else if (fileExtension == ".json") // Using world serialization with all the maps at once.
            {
                Debug.Log("Loading serialized world from JSON file.");
                return WorldFileManager.LoadWorld(filePath);
            }
            else
            {
                Debug.LogError("Unsupported file format: " + fileExtension);
            }
        }
        else
        {
            Debug.Log("File loading was canceled.");
        }

        return null;
    }

    // Initiates the process to save a world to a file.
    public void SaveWorld(World world)
    {
        fileBrowserHandler.SaveFile(filePath =>
        {
            if (filePath != null)
            {
                Debug.Log("Saving file path: " + filePath);
                WorldFileManager.SaveWorld(world, filePath);
            }
            else
            {
                Debug.Log("File saving was canceled.");
            }
        });
    }

    // Saves the world to a new file with automatic numbering - incremented name - to avoid overwriting.
    public void SaveWorldAutomatically(World world)
    {
        string saveDirectory = Path.Combine(Application.streamingAssetsPath, "SavedWorlds");
        if (!Directory.Exists(saveDirectory))
        {
            Directory.CreateDirectory(saveDirectory);
        }

        int nextWorldNumber = GetNextWorldNumber(saveDirectory);
        string savePath = Path.Combine(saveDirectory, $"World_{nextWorldNumber}.json");
        WorldFileManager.SaveWorld(world, savePath);
        Debug.Log($"World saved automatically to: {savePath}");
    }

    // Gets the next available world number for naming saved worlds. Lowest number of the current repository files.
    private int GetNextWorldNumber(string directoryPath)
    {
        var worldFiles = Directory.GetFiles(directoryPath, "World_*.json");
        int highestNumber = 0;

        foreach (string filePath in worldFiles)
        {
            string fileName = Path.GetFileNameWithoutExtension(filePath);
            if (int.TryParse(fileName.Split('_')[1], out int worldNumber) && worldNumber > highestNumber)
            {
                highestNumber = worldNumber;
            }
        }

        return highestNumber + 1;
    }
}