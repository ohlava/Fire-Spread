using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Core data structures
public class World
{
    public int Width { get; set; }
    public int Depth { get; set; }
    public Tile[,] Grid { get; set; }
    public Tile HighestTile { get; set; }
    public Weather Weather { get; set; }

    public World(int width, int depth)
    {
        Width = width;
        Depth = depth;
        Grid = new Tile[width, depth];
    }

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
    public float Moisture { get; set; } // 100% for water
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

public class WorldGenerator : MonoBehaviour
{
    public World world;

    public int worldWidth = 50;
    public int worldDepth = 50;

    HeightMapImporter mapImporter;
    [SerializeField] GameObject mapImporterObj;
    void Awake()
    {
        mapImporter = mapImporterObj.GetComponent<HeightMapImporter>();
    }

    // Change to NoiseSettings / WorldGenerationSettings
    public bool useCustomMap;
    private float[,] customMap;
    public int octaves = 5;
    public float persistence = 0.4f;
    public int rivers = 1;

    public World GenerateNewWorld()
    {
        if (useCustomMap)
        {
            customMap = mapImporter.GetMap();
            worldWidth = customMap.GetLength(0);
            worldDepth = customMap.GetLength(1);

            GenerateWorldFromHeightMap(customMap);
        }
        else
        {
            GenerateWorld();
        }
        return world;
    }

    private void GenerateWorldFromHeightMap(float[,] map)
    {
        world = new World(worldWidth, worldDepth);
        float heighestPoint = 0f;

        for (int x = 0; x < worldWidth; x++)
        {
            for (int y = 0; y < worldDepth; y++)
            {
                float height = map[x,y];
                Tile currTile = new Tile { Height = height, Moisture = (map[x, y] == 0) ? 100 : 0 };

                if (height > heighestPoint)
                {
                    world.HighestTile = currTile;
                    heighestPoint = height;
                }

                world.Grid[x, y] = currTile;
            }
        }
        return;
    }

    private void GenerateWorld()
    {
        float[,] heightMap = new float[worldWidth, worldDepth];
        int[,] lakeMap; // rivers can end in there
        int[,] riverMap;

        heightMap = GenerateBaseTerrain(heightMap);
        lakeMap = GenerateLakes(heightMap);
        riverMap = GenerateRivers(heightMap, lakeMap);

        heightMap = GenerateCombinedMap(heightMap, lakeMap, riverMap);
        GenerateWorldFromHeightMap(heightMap);
        return;
    }

    private float[,] GenerateBaseTerrain(float[,] heightMap)
    {
        // Generate multi-octave Perlin noise for the height map
        for (int x = 0; x < worldWidth; x++)
        {
            for (int y = 0; y < worldDepth; y++)
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
        return heightMap;
    }

    private int[,] GenerateLakes(float[,] heightMap)
    {
        // Identify and create lakes
        float lakeThreshold = 0.15f;  // You can adjust this value to control how often lakes appear

        int[,] lakeMap = new int[worldWidth, worldDepth];
        bool[,] visited = new bool[worldWidth, worldDepth];

        for (int x = 1; x < worldWidth - 1; x++)
        {
            for (int y = 1; y < worldDepth - 1; y++)
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
            if (x < 0 || x >= worldWidth || y < 0 || y >= worldDepth || visited[x, y] || heightMap[x, y] >= lakeThreshold)
            {
                return;
            }

            visited[x, y] = true;
            lakeMap[x, y] = 1;

            CreateLake(x + 1, y, heightMap, visited, lakeThreshold);
            CreateLake(x - 1, y, heightMap, visited, lakeThreshold);
            CreateLake(x, y + 1, heightMap, visited, lakeThreshold);
            CreateLake(x, y - 1, heightMap, visited, lakeThreshold);
        }
        return lakeMap;

    }

    private int[,] GenerateRivers(float[,] heightMap, int[,] lakeMap)
    {
        int[,] riverMap = new int[worldWidth, worldDepth]; 

        // Generate rivers
        System.Random rand = new System.Random();
        for (int i = 0; i < rivers; i++)
        {
            int riverStartX = rand.Next(worldWidth);
            int riverStartY = rand.Next(worldDepth);

            int x = riverStartX;
            int y = riverStartY;
            int direction = rand.Next(4);

            while (x < worldWidth && y < worldDepth)
            {
                riverMap[x, y] = 1;

                // Move in a semi-random direction
                switch (direction)
                {
                    case 0: if (rand.NextDouble() < 0.5) x++; else y++; break;
                    case 1: if (rand.NextDouble() < 0.5) y--; else x++; break;
                    case 2: if (rand.NextDouble() < 0.5) x--; else y--; break;
                    case 3: if (rand.NextDouble() < 0.5) y++; else x--; break;
                }

                if (x < 0 || y < 0 || x >= worldWidth || y >= worldDepth || lakeMap[x, y] == 1)
                    break;
            }

        }
        return riverMap;
    }

    private float[,] GenerateCombinedMap(float[,] heightMap, int[,] lakeMap, int[,] riverMap)
    {
        float beachFactor = 0.15f; // adjust this to control how much the terrain around water tiles is reduced

        // Iterate over each tile in the world
        for (int x = 0; x < worldWidth; x++)
        {
            for (int y = 0; y < worldDepth; y++)
            {
                // Check if the current tile is a water tile (i.e., its height is 0)
                if (lakeMap[x, y] == 1 || riverMap[x, y] == 1)
                {
                    // all water is 0
                    heightMap[x, y] = 0;

                    // Iterate over the neighbors of the current tile
                    for (int dx = -1; dx <= 2; dx++)
                    {
                        for (int dy = -1; dy <= 2; dy++)
                        {
                            // Check if the neighbor is within the world bounds
                            int nx = x + dx;
                            int ny = y + dy;
                            if (nx >= 0 && nx < worldWidth && ny >= 0 && ny < worldDepth)
                            {
                                // Reduce the height of the neighbor by the beach factor, but don't let it go below 0
                                heightMap[nx, ny] = Mathf.Max(heightMap[nx, ny] - beachFactor, 0);
                            }
                        }
                    }
                }
            }
        }
        return heightMap;
    }
}