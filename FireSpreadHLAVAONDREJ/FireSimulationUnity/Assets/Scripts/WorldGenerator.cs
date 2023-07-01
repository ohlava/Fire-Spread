using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class WorldGenerator : MonoBehaviour
{
    public int worldWidth = 50;
    public int worldDepth = 50;

    MapImporter mapImporter;
    [SerializeField] GameObject mapImporterObj;
    void Awake()
    {
        mapImporter = mapImporterObj.GetComponent<MapImporter>();
    }

    // Combine all to WorldGenerationSettings
    public bool useCustomMap;
    public int octaves = 5;
    public float persistence = 0.4f;
    public int rivers = 1;

    public World GetWorld()
    {
        World world;

        // get heightMap different ways
        float[,] heightMap;
        if (useCustomMap)
        {
            heightMap = mapImporter.GetMap(); // returns different size that defualt worldWidth and worldDepth
            worldWidth = heightMap.GetLength(0);
            worldDepth = heightMap.GetLength(1);
        }
        else
        {
            heightMap = GenerateBaseTerrain();
        }

        int[,] lakeMap = GenerateLakes(heightMap); // rivers can end in there

        int[,] riverMap = GenerateRivers(heightMap, lakeMap); //

        heightMap = GenerateCombinedMap(heightMap, lakeMap, riverMap);

        int[,] moistureMap = GenerateMoistureMap(heightMap, lakeMap, riverMap);

        VegetationType[,] vegetationMap = GenerateVegetationMap(moistureMap);

        world = GenerateWorldFromMaps(heightMap, moistureMap, vegetationMap);
        return world;
    }

    private int[,] GenerateMoistureMap(float[,] heightMap, int[,] lakeMap, int[,] riverMap)
    {
        int[,] moistureMap = new int[worldWidth, worldDepth];
        for (int x = 0; x < worldWidth; x++)
        {
            for (int y = 0; y < worldDepth; y++)
            {
                if (heightMap[x, y] == 0 || lakeMap[x, y] == 1 || riverMap[x, y] == 1) // water is represented by 1 in lake and river maps
                {
                    moistureMap[x, y] = 100;
                }
                else
                {
                    // Use Perlin noise to generate a moisture value
                    float noise = Mathf.PerlinNoise(x / 10.0f, y / 10.0f); // Adjust the division value to change the noise scale
                    moistureMap[x, y] = (int)(noise * 100);
                }
            }
        }
        return moistureMap;
    }

    private VegetationType[,] GenerateVegetationMap(int[,] moistureMap)
    {
        VegetationType[,] vegetationMap = new VegetationType[worldWidth, worldDepth];

        for (int x = 0; x < worldWidth; x++)
        {
            for (int y = 0; y < worldDepth; y++)
            {
                int moisture = moistureMap[x, y];

                // Adjust these moisture thresholds to change the distribution of vegetation
                if (moisture < 30)
                {
                    vegetationMap[x, y] = VegetationType.Sparse;
                }
                else if (moisture < 50)
                {
                    vegetationMap[x, y] = VegetationType.Grass;
                }
                else if (moisture < 70)
                {
                    vegetationMap[x, y] = VegetationType.Forest;
                }
                else
                {
                    vegetationMap[x, y] = VegetationType.Swamp;
                }
            }
        }
        return vegetationMap;
    }

    private World GenerateWorldFromMaps(float[,] heightMap, int[,] moistureMap, VegetationType[,] vegetationMap)
    {
        World world = new World(worldWidth, worldDepth);
        float heighestPoint = 0f;

        for (int x = 0; x < worldWidth; x++)
        {
            for (int y = 0; y < worldDepth; y++)
            {
                float height = heightMap[x,y];
                int moisture = moistureMap[x, y];
                VegetationType vegetation = vegetationMap[x, y];

                // Assign the height, moisture and vegetation values to the tile
                Tile currTile = new Tile { Height = height, Moisture = moisture, Vegetation = vegetation};

                // Put that tile in the world on correct position
                world.Grid[x, y] = currTile;

                // for the HighestTile of the world
                if (height > heighestPoint)
                {
                    world.HighestTile = currTile;
                    heighestPoint = height;
                } 
            }
        }
        return world;
    }

    private float[,] GenerateBaseTerrain()
    {
        float[,] heightMap = new float[worldWidth, worldDepth];

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
        float lakeThreshold = 0.2f;  // You can adjust this value to control how often lakes appear

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