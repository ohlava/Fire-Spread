using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class WorldGenerator : MonoBehaviour
{
    public int worldWidth = 10;
    public int worldDepth = 10;

    private IMapImporter mapImporter; // This will be set dynamically
    public bool useCustomMap = false;
    public string mapFilePath;
    public int mapSeed;

    // TODO combine all to WorldGenerationSettings
    public int octaves = 5;
    public float persistence = 0.4f;
    public float lakeThreshold = 0.12f; // 0-1 adjust this to control how often lakes appear
    public int rivers = 1; 

    public World GetWorld()
    {
        World world;

        if (useCustomMap)
        {
            mapImporter = new FileMapImporter(mapFilePath);
        }
        else
        {
            mapImporter = new SeedMapImporter(mapSeed);
        }


        // get heightMap different ways
        float[,] heightMap;
        if (useCustomMap)
        {
            heightMap = mapImporter.GetMap(); // returns different size that default worldWidth and worldDepth
            worldWidth = heightMap.GetLength(0);
            worldDepth = heightMap.GetLength(1);
        }
        else
        {
            heightMap = GenerateBaseTerrain();
        }

        // water is represented by 1 in lake and river maps, rest is zero
        int[,] lakeMap = GenerateLakes(heightMap); // rivers can end in lakes
        int[,] riverMap = GenerateRivers(heightMap, lakeMap); 

        heightMap = GenerateCombinedMap(heightMap, lakeMap, riverMap);

        int[,] moistureMap = GenerateMoistureMap(heightMap, lakeMap, riverMap);

        VegetationType[,] vegetationMap = GenerateVegetationMap(moistureMap);

        world = GenerateWorldFromMaps(heightMap, moistureMap, vegetationMap);

        return world;
    }

    private int[,] GenerateMoistureMap(float[,] heightMap, int[,] lakeMap, int[,] riverMap)
    {
        int[,] moistureMap = new int[worldWidth, worldDepth];

        int seed = (int)System.DateTime.Now.Ticks;
        UnityEngine.Random.InitState(seed);

        float offsetX = UnityEngine.Random.Range(0, 10000f);
        float offsetY = UnityEngine.Random.Range(0, 10000f);

        for (int x = 0; x < worldWidth; x++)
        {
            for (int y = 0; y < worldDepth; y++)
            {
                // TODO moisture along water tiles is usually increased
                if (heightMap[x, y] == 0 || lakeMap[x, y] == 1 || riverMap[x, y] == 1)
                {
                    moistureMap[x, y] = 100;
                }
                else
                {
                    // Use Perlin noise to generate a moisture value
                    float noise = Mathf.PerlinNoise((x + offsetX) / 10.0f, (y + offsetY) / 10.0f); // Adjust the division value to change the noise scale
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

                vegetationMap[x, y] = VegetationType.Grass; // base vegetation

                if (UnityEngine.Random.Range(0f, 1f) <= 0.85f) // call 85% of the time
                {
                    // Adjust these moisture thresholds to change the distribution and amount of certain vegetation
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
                Tile currTile = new Tile(height, moisture, vegetation, x, y);

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

        int seed = (int)System.DateTime.Now.Ticks;
        UnityEngine.Random.InitState(seed);

        float offsetX = UnityEngine.Random.Range(0, 10000f);
        float offsetY = UnityEngine.Random.Range(0, 10000f);

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
                    float perlinValue = Mathf.PerlinNoise((x + offsetX) / 20f * frequency, (y + offsetY) / 20f * frequency) * 2 - 1;
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

        for (int x = 0; x < worldWidth; x++)
        {
            for (int y = 0; y < worldDepth; y++)
            {
                if (lakeMap[x, y] == 1 || riverMap[x, y] == 1) // is the current tile water tile? (i.e., its height is 0)
                {
                    // all water is 0
                    heightMap[x, y] = 0;

                    // Iterate over only some of the neighbors - pseudo randomly
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