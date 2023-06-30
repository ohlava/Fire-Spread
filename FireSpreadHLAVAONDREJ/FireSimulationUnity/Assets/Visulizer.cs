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

                // dictinary to connect tiles to their instances
                tileToInstanceDict[worldTile] = tileInstance;
            }
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
        Debug.Log("No tile found");
        return ft;
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