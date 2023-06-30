using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Visulizer : MonoBehaviour
{
    public GameObject tilePrefab;
    public Camera mainCamera;

    private Dictionary<Tile, GameObject> tileToInstanceDict = new Dictionary<Tile, GameObject>();

    // Add the layer mask for the tileInstances
    public LayerMask tileLayer;

    public void CreateWorld(World world)
    {
        // Generate tileInstances
        for (int x = 0; x < world.Width; x++)
        {
            for (int y = 0; y < world.Depth; y++)
            {
                Tile worldTile = world.GetTileAt(x, y);
                float height = worldTile.Height;

                GameObject tileInstance = Instantiate(tilePrefab, new Vector3(x, height * 10, y), Quaternion.identity);

                tileInstance.transform.localScale = new Vector3(1, height, 1);
                tileInstance.transform.position = new Vector3(x, tileInstance.transform.localScale.y / 2, y);

                // If tile is a water tile, color it blue
                if (height == 0) // or check from moisture?
                {
                    Renderer renderer = tileInstance.GetComponent<Renderer>();
                    if (renderer != null)
                    {
                        renderer.material.color = Color.blue;
                    }
                }

                // dictinary to connect tiles to their instances
                tileToInstanceDict[worldTile] = tileInstance;
            }
        }
    }

    public GameObject firePrefab; // Assign a prefab for the "something" you want to create

    public void createFireOnTile(Tile t)
    {
        // Check if there's a GameObject instance corresponding to the tile
        if (tileToInstanceDict.TryGetValue(t, out GameObject ti))
        {
            // Get the position and height of the tile instance
            Vector3 tilePosition = ti.transform.position;
            float tileHeight = ti.transform.localScale.y;

            // Create a new "something" at the position of the tile + height along the Y axis
            GameObject something = Instantiate(firePrefab, tilePosition + new Vector3(0, tileHeight, 0), Quaternion.identity);
        }
        else
        {
            Debug.LogError("No instance found for the given tile.");
        }
    }


    public void DeleteAllTiles()
    {
        // Clear any existing tile instances
        foreach (var item in tileToInstanceDict)
        {
            Destroy(item.Value);
        }
        tileToInstanceDict.Clear();
    }

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