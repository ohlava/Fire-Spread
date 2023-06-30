using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Visulizer : MonoBehaviour
{
    public GameObject tilePrefab;
    public Camera mainCamera;

    private List<GameObject> tileInstances = new List<GameObject>(); // Corresponding tiles - change to dict?
    private List<Tile> actualTiles = new List<Tile>();

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

                GameObject tileInstance = Instantiate(tilePrefab, new Vector3(x, height * 5, y), Quaternion.identity);

                tileInstance.transform.localScale = new Vector3(1, height * 10, 1);
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

                tileInstances.Add(tileInstance);
                actualTiles.Add(worldTile);
            }
        }
    }

    public void DeleteAllTiles()
    {
        // Clear any existing tileInstances
        foreach (var tile in tileInstances)
        {
            Destroy(tile);
        }
        tileInstances.Clear();
        actualTiles.Clear();
    }

    public Tile GetWorldTileFromInstance(GameObject instance)
    {
        int index = tileInstances.FindIndex(a => a == instance);
        return actualTiles[index];
    }

    public void SetCameraPositionAndOrientation(int worldWidth, int worldDepth)
    {
        int cameraHeight = 50;
        // Set camera's position to the center of the world.
        mainCamera.transform.position = new Vector3(worldWidth / 2f, cameraHeight, -worldDepth / 2f);

        // Adjust the camera's position and orientation based on world size.
        mainCamera.transform.position += new Vector3(0, cameraHeight * 0.5f, 0);
        mainCamera.transform.rotation = Quaternion.Euler(45, 0, 0);
    }

}