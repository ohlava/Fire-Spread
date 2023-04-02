using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class WorldGenerator : MonoBehaviour
{
    // Core data structures
    public class World
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public Tile[,] Grid { get; set; }
        public Weather Weather { get; set; }

        public void UpdateWeather(Weather newWeather)
        {
            // Update the weather conditions in the world.
        }

        public Tile GetTileAt(int x, int y)
        {
            return Grid[x,y];
            // Return the tile at the specified position in the grid.
        }
    }

    public class Tile
    {
        public float Moisture { get; set; }
        public VegetationType Vegetation { get; set; }
        public float Height { get; set; }
        public bool IsBurning { get; set; }
        public bool HasBurned { get; set; }

        public void Ignite()
        {
            // Start burning this tile if it's not already burning or burned.
        }

        public void Extinguish()
        {
            // Extinguish the fire on this tile and set its state to burned.
        }
    }

    public class Weather
    {
        public float WindDirection { get; set; }
        public float WindStrength { get; set; }

        public Weather(float windDirection, float windStrength)
        {
            // Initialize the weather conditions.
        }
    }

    public enum VegetationType
    {
        Grass,
        Forest,
        Sparse
        // Add other types of vegetation as needed.
    }

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
                float height = map != null ? map[x, y] : Mathf.PerlinNoise(x / 10.0f, y / 10.0f);
                world.Grid[x, y] = new Tile { Height = height };

                GameObject tileInstance = Instantiate(tilePrefab, new Vector3(x, height * 5, y), Quaternion.identity);
                tileInstance.transform.localScale = new Vector3(1, height * 10, 1);
                tileInstance.transform.position = new Vector3(x, tileInstance.transform.localScale.y / 2, y);
            }
        }
    }

}



