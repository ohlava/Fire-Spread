using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class World
{
    public int Width { get; set; }
    public int Height { get; set; }
    public Tile[,] Grid { get; set; }
}

public class Tile
{
    public float Moisture { get; set; }
    public float Hight { get; set; }
}


public class WorldGenerator : MonoBehaviour
{
    public World world;
    public GameObject tilePrefab;
    public bool useCustomMap;
    public float[,] customMap;

    private void Start()
    {
        if (useCustomMap)
        {
            GenerateWorld(customMap);
        }
        else
        {
            GenerateWorld();
        }
    }

    private void GenerateWorld(float[,] map = null)
    {
        world = new World
        {
            Width = map != null ? map.GetLength(0) : 100,
            Height = map != null ? map.GetLength(1) : 100,
            Grid = new Tile[map != null ? map.GetLength(0) : 100, map != null ? map.GetLength(1) : 100]
        };

        for (int x = 0; x < world.Width; x++)
        {
            for (int y = 0; y < world.Height; y++)
            {
                float hight = map != null ? map[x, y] : Mathf.PerlinNoise(x / 10.0f, y / 10.0f);
                world.Grid[x, y] = new Tile { Hight = hight };

                GameObject tileInstance = Instantiate(tilePrefab, new Vector3(x, hight * 5, y), Quaternion.identity);
                tileInstance.transform.localScale = new Vector3(1, hight * 10, 1);
                tileInstance.transform.position = new Vector3(x, tileInstance.transform.localScale.y / 2, y);
            }
        }
    }

}