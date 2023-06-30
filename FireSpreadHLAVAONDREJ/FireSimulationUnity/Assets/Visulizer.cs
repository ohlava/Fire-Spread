using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Visulizer : MonoBehaviour
{
    public GameObject tilePrefab;

    private List<GameObject> tileInstances = new List<GameObject>(); // Corresponding tiles - change to dict?
    private List<Tile> actualTiles = new List<Tile>();

    // Add the layer mask for the tileInstances
    public LayerMask tileLayer;

    public void CreateWorld(World world)
    {
        // Clear any existing tileInstances
        DeleteAllTiles();

        // Generate tileInstances
        for (int x = 0; x < world.Width; x++)
        {
            for (int y = 0; y < world.Height; y++)
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
        foreach (var tile in tileInstances)
        {
            Destroy(tile);
        }
        tileInstances.Clear();
    }

    public Tile GetWorldTileFromInstance(GameObject instance)
    {
        int index = tileInstances.FindIndex(a => a == instance);
        return actualTiles[index];
    }
}