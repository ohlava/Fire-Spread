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

    // Change to NoiseSettings
    public int octaves = 5;
    public float persistence = 0.4f;

    public int rivers = 1;

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

        float[,] heightMap = new float[world.Width, world.Height];

        int[,] lakeMap = new int[world.Width, world.Height]; // for rivers to end on those
        int[,] riverMap = new int[world.Width, world.Height]; 

        // Generate multi-octave Perlin noise for the height map
        for (int x = 0; x < world.Width; x++)
        {
            for (int y = 0; y < world.Height; y++)
            {
                float amplitude = 1;
                float frequency = 1;
                float noiseHeight = 0;
                
                for (int i = 0; i < octaves; i++)
                {
                    float perlinValue = Mathf.PerlinNoise(x / 20f * frequency, y / 20f * frequency) * 2 - 1;
                    noiseHeight += perlinValue * amplitude;
                    
                    amplitude *= persistence; // decrease the amplitude
                    frequency *= 2; // double the frequency
                }

                if (noiseHeight > 1)
                {
                    noiseHeight = 1;
                }
                else if (noiseHeight < -1)
                {
                    noiseHeight = -1;
                }
                
                heightMap[x, y] = (noiseHeight + 1) / 2; // normalize to 0-1
            }
        }

        // Identify and create lakes and adjust rivers
        float lakeThreshold = 0.2f;  // You can adjust this value to control how often lakes appear
        bool[,] visited = new bool[world.Width, world.Height];

        for (int x = 1; x < world.Width - 1; x++)
        {
            for (int y = 1; y < world.Height - 1; y++)
            {
                // If a point is a local minimum and its height is below the lake threshold, create a lake
                if (!visited[x, y] && heightMap[x, y] < lakeThreshold
                    && heightMap[x, y] <= heightMap[x + 1, y] 
                    && heightMap[x, y] <= heightMap[x - 1, y]
                    && heightMap[x, y] <= heightMap[x, y + 1] 
                    && heightMap[x, y] <= heightMap[x, y - 1])
                {
                    CreateLake(x, y, heightMap, visited, lakeThreshold);
                }
            }
        }

        // Recursive function to propagate the lake to neighbors
        void CreateLake(int x, int y, float[,] heightMap, bool[,] visited, float lakeThreshold)
        {
            if (x < 0 || x >= world.Width || y < 0 || y >= world.Height || visited[x, y] || heightMap[x, y] >= lakeThreshold)
            {
                return;
            }

            visited[x, y] = true;
            heightMap[x, y] = 0;
            lakeMap[x, y] = 1;

            CreateLake(x + 1, y, heightMap, visited, lakeThreshold);
            CreateLake(x - 1, y, heightMap, visited, lakeThreshold);
            CreateLake(x, y + 1, heightMap, visited, lakeThreshold);
            CreateLake(x, y - 1, heightMap, visited, lakeThreshold);
        }


        // Generate rivers
        System.Random rand = new System.Random();
        for (int i = 0; i < rivers; i++)
        {
            int riverStartX = rand.Next(world.Width);
            int riverStartY = rand.Next(world.Height);

            int x = riverStartX;
            int y = riverStartY;
            int direction = rand.Next(4);

            while (x < world.Width && y < world.Height)
            {
                // Create a valley around the river
                for (int dx = -1; dx <= 1; dx++)
                {
                    for (int dy = -1; dy <= 1; dy++)
                    {
                        int nx = x + dx;
                        int ny = y + dy;

                        if (nx >= 0 && ny >= 0 && nx < world.Width && ny < world.Height)
                        {
                            float valleyDepth = 1f - Mathf.Max(Mathf.Abs(dx), Mathf.Abs(dy)) * 0.7f;  // Adjust depth factor as needed
                            heightMap[nx, ny] = Mathf.Min(heightMap[nx, ny], valleyDepth);
                        }
                    }
                }

                // Set height to 0 for river
                heightMap[x, y] = 0;
                riverMap[x, y] = 1;

                // Move in a semi-random direction
                switch (direction)
                {
                    case 0: if (rand.NextDouble() < 0.5) x++; else y++; break;
                    case 1: if (rand.NextDouble() < 0.5) y--; else x++; break;
                    case 2: if (rand.NextDouble() < 0.5) x--; else y--; break;
                    case 3: if (rand.NextDouble() < 0.5) y++; else x--; break;
                }

                if (x < 0 || y < 0 || x >= world.Width || y >= world.Height || lakeMap[x, y] == 1)
                    break;
            }
        }




        float beachFactor = 0.1f; // adjust this to control how much the terrain around water tiles is reduced

        // Iterate over each tile in the world
        for (int x = 0; x < world.Width; x++)
        {
            for (int y = 0; y < world.Height; y++)
            {
                // Check if the current tile is a water tile (i.e., its height is 0)
                if (heightMap[x, y] == 0)
                {
                    // Iterate over the neighbors of the current tile
                    for (int dx = -1; dx <= 1; dx++)
                    {
                        for (int dy = -1; dy <= 1; dy++)
                        {
                            // Check if the neighbor is within the world bounds
                            int nx = x + dx;
                            int ny = y + dy;
                            if (nx >= 0 && nx < world.Width && ny >= 0 && ny < world.Height)
                            {
                                // Reduce the height of the neighbor by the beach factor, but don't let it go below 0
                                heightMap[nx, ny] = Mathf.Max(heightMap[nx, ny] - beachFactor, 0);
                            }
                        }
                    }
                }
            }
        }



        // repeat with smoothing multiple times?
        for (int x = 0; x < world.Width; x++)
        {
            for (int y = 0; y < world.Height; y++)
            {
                if (riverMap[x, y] == 1) 
                {
                    heightMap[x, y] = 0;
                }
            }
        }








        // Generate tiles
        for (int x = 0; x < world.Width; x++)
        {
            for (int y = 0; y < world.Height; y++)
            {
                float height = map != null ? map[x, y] : heightMap[x, y];
                world.Grid[x, y] = new Tile { Height = height };

                GameObject tileInstance = Instantiate(tilePrefab, new Vector3(x, height * 5, y), Quaternion.identity);
                tileInstance.transform.localScale = new Vector3(1, height * 10, 1);
                tileInstance.transform.position = new Vector3(x, tileInstance.transform.localScale.y / 2, y);
                
                // If tile is a water tile, color it blue
                if (height == 0)
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

}