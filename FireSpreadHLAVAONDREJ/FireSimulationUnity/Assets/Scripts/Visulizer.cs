using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class GameObjectExtensions
{
    // set color for GameObject instance
    public static void setColorTo(this GameObject instance, Color color)
    {
        Renderer renderer = instance.transform.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = color;
        }
    }

    public static Vector3 GetPosition(this GameObject instance)
    {
        return instance.transform.position;
    }

    public static float GetHeight(this GameObject instance)
    {
        return instance.transform.localScale.y;
    }
}

public enum VisulizerMode
{
    Standard,
    Simplified
}

public class Visulizer : MonoBehaviour
{
    public VisulizerMode mode = VisulizerMode.Simplified; // Do NOT change during the simulation TODO make it so we can change

    public GameObject TilePrefab;
    public float TileHeightMultiplier = 4.0f;
    private Dictionary<Tile, GameObject> tileToInstanceDict = new Dictionary<Tile, GameObject>();

    // Add the layer mask for the tileInstances - for handling Raycasting
    public LayerMask tileLayer;

    // Generate tileInstances - is called no matter the mode
    public void CreateWorldTiles(World world)
    {
        for (int x = 0; x < world.Width; x++)
        {
            for (int y = 0; y < world.Depth; y++)
            {
                Tile worldTile = world.GetTileAt(x, y);
                float height = worldTile.Height;

                GameObject tileInstance = Instantiate(TilePrefab, new Vector3(x, height, y), Quaternion.identity);

                tileInstance.transform.localScale = new Vector3(1, height * TileHeightMultiplier, 1);
                tileInstance.transform.position = new Vector3(x, tileInstance.transform.localScale.y / 2, y);

                // dictinary to connect tiles to their instances
                tileToInstanceDict[worldTile] = tileInstance;

                // after tileInstance is in the tileToInstanceDict, set its color
                SetAppropriateColor(worldTile);
            }
        }
    }

    public void MakeTileBurned(Tile tile)
    {
        SetColorOnTile(tile, new Color32(92, 64, 51, 255));
    }

    private void SetColorOnTile(Tile tile, Color color)
    {
        if (tileToInstanceDict.TryGetValue(tile, out GameObject tileInstance))
        {
            tileInstance.setColorTo(color);
        }
        else
        {
            Debug.LogError("No created instance found for the given tile to set color. It doesnt exist anymore in vizulizer data.");
        }
    }

    private void SetAppropriateColor(Tile tile)
    {
        int maxVegetationType = Enum.GetNames(typeof(VegetationType)).Length;

        if (tile.Moisture == 100) // If tile is a water tile, color it blue
        {
            SetColorOnTile(tile, Color.blue);
        }
        else // Set tile color based on vegetation level
        {
            float greenValue = 0.4f + (0.2f / (maxVegetationType - 1)) * (int)tile.Vegetation;
            SetColorOnTile(tile, new Color(0, greenValue, 0));  // RGB color with variable green value/shade
        }
    }



    // Generate vegetationInstances - for the standard mode
    public void CreateAllVegetation(World world)
    {
        if (mode == VisulizerMode.Standard)
        {
            foreach (var tile in world.Grid)
            {
                if (tile.Moisture != 100)
                {
                    CreateVegetationOnTile(tile, tile.Vegetation);
                }
            }
        }
    }

    // Create a new Dictionary to keep track of GameObjects created on Tiles
    private Dictionary<Tile, GameObject> tileToVegetationInstanceDict = new Dictionary<Tile, GameObject>();

    // corresponds to VegetationTypes
    public GameObject grassPrefab;
    public GameObject forestPrefab;
    public GameObject sparsePrefab;
    public GameObject swampPrefab;
    private void CreateVegetationOnTile(Tile tile, VegetationType vegetation)
    {
        GameObject tileInstance = tileToInstanceDict[tile];

        // Get the position and height of the tile instance
        Vector3 tilePosition = tileInstance.GetPosition();
        float tileHeight = tileInstance.GetHeight();

        GameObject chosenPrefab = null;

        switch (vegetation)
        {
            case VegetationType.Grass:
                chosenPrefab = grassPrefab;
                break;
            case VegetationType.Forest:
                chosenPrefab = forestPrefab;
                break;
            case VegetationType.Sparse:
                chosenPrefab = sparsePrefab;
                break;
            case VegetationType.Swamp:
                chosenPrefab = swampPrefab;
                break;
            default:
                // Code to handle unknown vegetation type
                Debug.LogError("Unkwown vegetationType, add corresponding prefab.");
                break;
        }

        if (chosenPrefab != null)
        {
            // Create a new vegetation at the top of the tile
            GameObject vegetationInstance = Instantiate(chosenPrefab, tilePosition + new Vector3(0, tileHeight / 2, 0), Quaternion.identity);

            // Make a random rotation
            vegetationInstance.transform.rotation = Quaternion.Euler(0f, UnityEngine.Random.Range(0f, 360f), 0f);

            // Add the created GameObject to the Dictionary
            tileToVegetationInstanceDict[tile] = vegetationInstance;
        }
    }

    // Method to destroy an instance on a specific tile
    public void DestroyVegetationOnTile(Tile tile)
    {
        if (mode == VisulizerMode.Standard)
        {
            // Check that there is something to destroy
            if (tileToVegetationInstanceDict.TryGetValue(tile, out GameObject instance))
            {
                Destroy(instance);
                tileToVegetationInstanceDict.Remove(tile); // Remove the entry from the dictionary
            }
            else
            {
                Debug.LogError("No created instance found for the given tile. It doesnt exist anymore in vizulizer data. VEGETATION");
            }
        }
    }



    // Create a new Dictionary to keep track of GameObjects created on Tiles
    private Dictionary<Tile, GameObject> tileToFireInstanceDict = new Dictionary<Tile, GameObject>();

    public GameObject firePrefab; // Assign a prefab for the fire you want to create
    public void CreateFireOnTile(Tile tile)
    {
        GameObject tileInstance = tileToInstanceDict[tile];
        if (mode == VisulizerMode.Standard)
        {
            tileInstance.setColorTo(Color.red);

            // Get the position and height of the tile instance
            Vector3 tilePosition = tileInstance.GetPosition();
            float tileHeight = tileInstance.GetHeight();

            // Create a new fire at the top of the tile
            GameObject fireInstance = Instantiate(firePrefab, tilePosition + new Vector3(0, tileHeight / 2, 0), Quaternion.Euler(-90, 0, 0));

            // Add the created GameObject to the Dictionary
            tileToFireInstanceDict[tile] = fireInstance;
        }
        else
        {
            tileInstance.setColorTo(Color.red);
        }
    }

    // Method to destroy an instance on a specific tile
    public void DestroyFireOnTile(Tile tile)
    {
        if (mode == VisulizerMode.Standard)
        {
            // Check that there is something to destroy
            if (tileToFireInstanceDict.TryGetValue(tile, out GameObject instance))
            {
                Destroy(instance);
                tileToFireInstanceDict.Remove(tile); // Remove the entry from the dictionary
            }
            else
            {
                Debug.LogError("No created instance found for the given tile. It doesnt exist anymore in vizulizer data. FIRE");
            }
        }
    }


    //
    // Destruction Methods
    //

    // Method to destroy all created vegetation instances
    public void DestroyAllVegetation()
    {
        foreach (GameObject instance in tileToVegetationInstanceDict.Values)
        {
            Destroy(instance);
        }
        // Clear the dictionary
        tileToVegetationInstanceDict.Clear(); 
    }
    // Method to destroy all created fire instances
    public void DestroyAllFire()
    {
        foreach (GameObject instance in tileToFireInstanceDict.Values)
        {
            Destroy(instance);
        }
        // Clear the dictionary
        tileToFireInstanceDict.Clear(); 
    }
    // Method to destroy all created tile instances
    public void DestroyAllTile()
    {
        foreach (GameObject instance in tileToInstanceDict.Values)
        {
            Destroy(instance);
        }
        // Clear the dictionary
        tileToInstanceDict.Clear();
    }

    // for clicking on some tile instance = to see which we clicked in combination with RayCast
    public Tile GetWorldTileFromInstance(GameObject instance)
    {
        Tile ft = tileToInstanceDict.Keys.First();
        foreach (Tile tile in tileToInstanceDict.Keys)
        {
            if (tileToInstanceDict[tile] == instance)
            {
                return tile;
            }
        }
        // should never happen - every tile has its own representation
        Debug.LogError("No Tile found for the given instance.");
        return ft;
    }

    public Camera mainCamera;
    public void SetCameraPositionAndOrientation(int worldWidth, int worldDepth)
    {
        // TODO calculate also based on the world height

        // The diagonal of the world in Unity units
        float diagonal = Mathf.Sqrt(worldWidth * worldWidth + worldDepth * worldDepth);
        float zoom = 0.7f;

        // Set the camera's position to the center of the world, and above it at the calculated distance.
        mainCamera.transform.position = new Vector3(worldWidth / 2f, diagonal * zoom, 0);

        // Adjust the camera's orientation to look towards the center of the world.
        mainCamera.transform.LookAt(new Vector3(worldWidth / 2f, 0, worldDepth / 2f));
    }
}