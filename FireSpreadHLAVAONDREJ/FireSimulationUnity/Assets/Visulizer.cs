using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Visulizer : MonoBehaviour
{
    //List<GameObject> tileInstances;
    public GameObject tilePrefab;
    
    public void CreateWorld(World world)
    {
        // Generate tiles
        for (int x = 0; x < world.Width; x++)
        {
            for (int y = 0; y < world.Height; y++)
            {
                float height = world.GetTileAt(x,y).Height;

                GameObject tileInstance = Instantiate(tilePrefab, new Vector3(x, height * 5, y), Quaternion.identity);
                //tileInstances.Add(tileInstance);

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
            }
        }
    }

    /*
    public void DeleteWorld()
    {
        foreach (GameObject tile in tileInstances)
        {
            GameObject.Destroy(tile);
        }
    }
    */

}
