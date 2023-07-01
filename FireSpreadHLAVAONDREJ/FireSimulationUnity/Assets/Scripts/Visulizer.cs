using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.PackageManager;
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

public class Visulizer : MonoBehaviour
{
    public GameObject tilePrefab;
    private Dictionary<Tile, GameObject> tileToInstanceDict = new Dictionary<Tile, GameObject>();

    // Add the layer mask for the tileInstances
    public LayerMask tileLayer;

    // Generate tileInstances
    public void CreateWorldTiles(World world)
    {
        for (int x = 0; x < world.Width; x++)
        {
            for (int y = 0; y < world.Depth; y++)
            {
                Tile worldTile = world.GetTileAt(x, y);
                float height = worldTile.Height;

                GameObject tileInstance = Instantiate(tilePrefab, new Vector3(x, height, y), Quaternion.identity);

                tileInstance.transform.localScale = new Vector3(1, height * 4, 1);
                tileInstance.transform.position = new Vector3(x, tileInstance.transform.localScale.y / 2, y);

                // If tile is a water tile, color it blue
                if (worldTile.Moisture == 100)
                {
                    tileInstance.setColorTo(Color.blue);
                }

                // dictinary to connect tiles to their instances
                tileToInstanceDict[worldTile] = tileInstance;
            }
        }
    }

    // Generate vegetationInstances
    public void CreateVegetation(World world)
    {
        foreach (var tile in world.Grid)
        {
            if (tile.Moisture != 100)
            {
                CreateVegetationOnTile(tile, tile.Vegetation);
            }
        }
    }

    // Create a new Dictionary to keep track of GameObjects created on Tiles
    private Dictionary<Tile, GameObject> tileToFireInstanceDict = new Dictionary<Tile, GameObject>();

    public GameObject firePrefab; // Assign a prefab for the fire you want to create
    public void CreateFireOnTile(Tile tile)
    {
        GameObject tileInstance = tileToInstanceDict[tile];

        tileInstance.setColorTo(Color.red);

        // Get the position and height of the tile instance
        Vector3 tilePosition = tileInstance.GetPosition();
        float tileHeight = tileInstance.GetHeight();

        // Create a new fire at the top of the tile
        GameObject fireInstance = Instantiate(firePrefab, tilePosition + new Vector3(0, tileHeight / 2, 0), Quaternion.Euler(-90, 0, 0));

        // Add the created GameObject to the Dictionary
        tileToFireInstanceDict[tile] = fireInstance;
    }

    // Method to destroy an instance on a specific tile
    public void DestroyFireOnTile(Tile tile)
    {
        // Check that there is something to destroy
        if (tileToFireInstanceDict.TryGetValue(tile, out GameObject instance))
        {
            Destroy(instance);
            tileToFireInstanceDict.Remove(tile); // Remove the entry from the dictionary
        }
        else
        {
            Debug.LogError("No created instance found for the given tile.");
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

        tileInstance.setColorTo(new Color(0f, 0.5f, 0f)); // dark green color

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

            // Add the created GameObject to the Dictionary
            tileToVegetationInstanceDict[tile] = vegetationInstance;
        }
    }

    // Method to destroy an instance on a specific tile
    public void DestroyVegetationOnTile(Tile tile)
    {
        // Check that there is something to destroy
        if (tileToVegetationInstanceDict.TryGetValue(tile, out GameObject instance))
        {
            Destroy(instance);
            tileToVegetationInstanceDict.Remove(tile); // Remove the entry from the dictionary
        }
        else
        {
            Debug.LogError("No created instance found for the given tile.");
        }
    }

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
    public void SetCameraPositionAndOrientation(World world)
    {
        // The diagonal of the world in Unity units
        float diagonal = Mathf.Sqrt(world.Width * world.Width + world.Depth * world.Depth);

        // Calculate the optimal distance required to view the entire map based on the diagonal.
        // This is derived from the formula of tangent in a right-angled triangle, tan(FOV/2) = (diagonal/2) / distance,
        // hence distance = (diagonal/2) / tan(FOV/2).
        float getCloserPar = 0.4f;
        float cameraDistance = (diagonal / 2)* getCloserPar / Mathf.Tan(mainCamera.fieldOfView * 0.5f * Mathf.Deg2Rad);

        // Set the camera's position to the center of the world, and above it at the calculated distance.
        mainCamera.transform.position = new Vector3(world.Width / 2f, cameraDistance, -world.Depth / 2f);

        // Adjust the camera's orientation to look towards the center of the world.
        mainCamera.transform.LookAt(new Vector3(world.Width / 2f, 0, world.Depth / 2f));
    }
}