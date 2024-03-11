using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using UnityEngine;


public class Map<T>
{
    public T[,] Data { get; set; }
    public int Width { get; }
    public int Depth { get; }

    public Map(int width, int depth)
    {
        Data = new T[width, depth];
        Width = width;
        Depth = depth;
    }

    public void FillWithDefault(T defaultValue)
    {
        for (int i = 0; i < Width; i++)
        {
            for (int j = 0; j < Depth; j++)
            {
                Data[i, j] = defaultValue;
            }
        }
    }

    public static implicit operator Map<T>(T[,] data)
    {
        return new Map<T>(data.GetLength(0), data.GetLength(1)) { Data = data };
    }

    public static implicit operator Map<T>(T[][] data)
    {
        int width = data.Length;
        int maxDepth = data.Max(row => row.Length); // get the maximum length

        Map<T> map = new Map<T>(width, maxDepth);

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < data[i].Length; j++) // only iterate to the length of the current row
                map.Data[i, j] = data[i][j];
        }

        return map;
    }

    public T[,] To2DArray()
    {
        return Data;
    }

    public T[][] ToJaggedArray()
    {
        T[][] jagged = new T[Width][];
        for (int i = 0; i < Width; i++)
        {
            jagged[i] = new T[Depth];
            for (int j = 0; j < Depth; j++)
                jagged[i][j] = Data[i, j];
        }
        return jagged;
    }
}

public interface IMapGenerator<T>
{
    Map<T> Generate();
}

public static class MapExtensions
{
    // Normalizes all heights to be between 0 and 1
    public static Map<float> Normalize(this Map<float> map)
    {
        // Find min and max values
        float minValue = float.MaxValue;
        float maxValue = float.MinValue;

        for (int x = 0; x < map.Width; x++)
        {
            for (int y = 0; y < map.Depth; y++)
            {
                if (map.Data[x, y] < minValue)
                {
                    minValue = map.Data[x, y];
                }

                if (map.Data[x, y] > maxValue)
                {
                    maxValue = map.Data[x, y];
                }
            }
        }

        // Scale the values between 0 and 1
        for (int x = 0; x < map.Width; x++)
        {
            for (int y = 0; y < map.Depth; y++)
            {
                map.Data[x, y] = (map.Data[x, y] - minValue) / (maxValue - minValue);
            }
        }

        return map;
    }

    // Reduce the height of the neighbors of waterMap by the beach factor
    public static Map<float> ReduceByBeachFactor(this Map<float> map, Map<int> waterMap, float beachFactor)
    {
        for (int x = 3; x < map.Width - 3; x++)
        {
            for (int y = 3; y < map.Depth - 3; y++)
            {
                if (waterMap.Data[x, y] == 1)
                {
                    // TODO experiment with -2/2 -3/3, random etc. 
                    for (int dx = -3; dx <= 3; dx++)
                    {
                        for (int dy = -3; dy <= 3; dy++)
                        {
                            map.Data[x + dx, y + dy] *= (1 - beachFactor);
                        }
                    }
                }
            }
        }

        return map.Normalize();
    }

    // Blur or Smooth Map: This could be used to make transitions between heights or other features more gradual.
    public static Map<float> Smooth(this Map<float> map, int iterations = 1)
    {
        for (int iter = 0; iter < iterations; iter++)
        {
            for (int x = 1; x < map.Width - 1; x++)
            {
                for (int y = 1; y < map.Depth - 1; y++)
                {
                    float avg = (
                        map.Data[x - 1, y - 1] + map.Data[x, y - 1] + map.Data[x + 1, y - 1] +
                        map.Data[x - 1, y] + map.Data[x, y] + map.Data[x + 1, y] +
                        map.Data[x - 1, y + 1] + map.Data[x, y + 1] + map.Data[x + 1, y + 1]
                    ) / 9f;

                    map.Data[x, y] = avg;
                }
            }
        }
        return map;
    }

    // Amplify(or Reduce) Map: Modify the values in the map by multiplying them by a factor.
    public static Map<float> Amplify(this Map<float> map, float factor)
    {
        for (int x = 0; x < map.Width; x++)
        {
            for (int y = 0; y < map.Depth; y++)
            {
                map.Data[x, y] *= factor;
            }
        }

        return map;
    }

    public static Map<float> RaiseElevation(this Map<float> map, float amount)
    {
        for (int x = 0; x < map.Width; x++)
        {
            for (int y = 0; y < map.Depth; y++)
            {
                map.Data[x, y] += amount;
            }
        }

        return map;
    }

    // Set Border: Set a fixed value for a border of a given width around the map. This can be useful if you want a consistent edge, like a sea level around islands.
    public static Map<float> SetBorder(this Map<float> map, int borderWidth, float value)
    {
        for (int x = 0; x < map.Width; x++)
        {
            for (int y = 0; y < borderWidth; y++) map.Data[x, y] = value;
            for (int y = map.Depth - borderWidth; y < map.Depth; y++) map.Data[x, y] = value;
        }
        for (int y = 0; y < map.Depth; y++)
        {
            for (int x = 0; x < borderWidth; x++) map.Data[x, y] = value;
            for (int x = map.Width - borderWidth; x < map.Width; x++) map.Data[x, y] = value;
        }

        return map;
    }

    public static Map<float> GaussianBlur(this Map<float> map)
    {
        float[,] blurredData = new float[map.Width, map.Depth];
        int blurSize = 1;  // this is a simple 3x3 kernel

        for (int x = 0; x < map.Width; x++)
        {
            for (int y = 0; y < map.Depth; y++)
            {
                float sum = 0;
                int count = 0;

                for (int i = -blurSize; i <= blurSize; i++)
                {
                    for (int j = -blurSize; j <= blurSize; j++)
                    {
                        int xi = x + i;
                        int yj = y + j;

                        if (xi >= 0 && xi < map.Width && yj >= 0 && yj < map.Depth)
                        {
                            sum += map.Data[xi, yj];
                            count++;
                        }
                    }
                }

                blurredData[x, y] = sum / count;
            }
        }

        return blurredData;  // due to the implicit conversion we defined earlier
    }
}

public static class Array2DExtensions
{
    public static IEnumerable<(int, int)> GetNeighbours(this Array array, int x, int y)
    {
        int lowerBoundX = array.GetLowerBound(0);
        int upperBoundX = array.GetUpperBound(0);

        int lowerBoundY = array.GetLowerBound(1);
        int upperBoundY = array.GetUpperBound(1);

        for (int i = Math.Max(lowerBoundX, x - 1); i <= Math.Min(upperBoundX, x + 1); i++)
        {
            for (int j = Math.Max(lowerBoundY, y - 1); j <= Math.Min(upperBoundY, y + 1); j++)
            {
                if (i != x || j != y)
                    yield return (i, j);
            }
        }
    }
}

public class BaseTerrainGenerator : IMapGenerator<float>
{
    private int width;
    private int depth;

    public BaseTerrainGenerator(int width, int depth)
    {
        this.width = width;
        this.depth = depth;
    }

    public Map<float> Generate()
    {
        // Code for generating base terrain here...
        Map<float> map = new Map<float>(width, depth);

        int octaves = 5;
        float persistence = 0.4f;

        float offsetX = UnityEngine.Random.Range(0, 10000f);
        float offsetY = UnityEngine.Random.Range(0, 10000f);

        // Generate multi-octave Perlin noise for the height map
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < depth; y++)
            {
                float amplitude = 1;
                float frequency = 1;
                float noiseHeight = 0;

                for (int i = 0; i < octaves; i++)
                {
                    // to adjust the range to between -1 and 1, Mathf.PerlinNoise generates a value between 0 and 1
                    float perlinValue = Mathf.PerlinNoise((x + offsetX) / 20f * frequency, (y + offsetY) / 20f * frequency) * 2 - 1;
                    noiseHeight += perlinValue * amplitude;

                    amplitude *= persistence; // decrease the amplitude
                    frequency *= 2; // double the frequency
                }

                map.Data[x, y] = noiseHeight;
            }
        }
        return map.Normalize();
    }
}

// TODO We'll identify valleys below the threshold, then group them into clusters, and from those clusters, decide which ones to fill based on the lakeAmountParameter.
public class LakeMapGenerator : IMapGenerator<int>
{
    private Map<float> heightMap;
    private float lakeThreshold;

    public LakeMapGenerator(Map<float> heightMap, float lakeThreshold)
    {
        this.heightMap = heightMap;
        this.lakeThreshold = lakeThreshold;
    }

    public Map<int> Generate()
    {
        Map<int> lakeMap = new Map<int>(heightMap.Width, heightMap.Depth);

        for (int x = 0; x < heightMap.Width; x++)
        {
            for (int y = 0; y < heightMap.Depth; y++)
            {
                lakeMap.Data[x, y] = heightMap.Data[x, y] < lakeThreshold ? 1 : 0;
            }
        }

        return lakeMap;
    }
}

public class RiverMapGenerator : IMapGenerator<int>
{
    private Map<float> heightMap;
    private Map<int> lakeMap;
    private int rivers;

    public RiverMapGenerator(Map<float> heightMap, Map<int> lakeMap, int rivers)
    {
        this.heightMap = heightMap;
        this.lakeMap = lakeMap;
        this.rivers = rivers;
    }

    public Map<int> Generate()
    {
        Map<int> riverMap = new Map<int>(heightMap.Width, heightMap.Depth);

        // Simple river generation logic - More sophisticated logic would be required for realistic rivers

        // Generate rivers
        for (int i = 0; i < rivers; i++)
        {
            int riverStartX = UnityEngine.Random.Range(0, heightMap.Width);
            int riverStartY = UnityEngine.Random.Range(0, heightMap.Depth);

            int x = riverStartX;
            int y = riverStartY;
            int direction = UnityEngine.Random.Range(0, 4);

            while (x < heightMap.Width && y < heightMap.Depth)
            {
                riverMap.Data[x, y] = 1;

                // Move in a semi-random direction
                switch (direction)
                {
                    case 0: if (UnityEngine.Random.value < 0.5f) x++; else y++; break;
                    case 1: if (UnityEngine.Random.value < 0.5f) y--; else x++; break;
                    case 2: if (UnityEngine.Random.value < 0.5f) x--; else y--; break;
                    case 3: if (UnityEngine.Random.value < 0.5f) y++; else x--; break;
                }

                if (x < 0 || y < 0 || x >= heightMap.Width || y >= heightMap.Depth || lakeMap.Data[x, y] == 1)
                    break;
            }
        }
        return riverMap;
    }
}

public class MoistureMapGenerator : IMapGenerator<int>
{
    // Simplified moisture generation based on water bodies

    private Map<float> heightMap;
    private Map<int> lakeMap;
    private Map<int> riverMap;

    private int moistureRadius = 2;  // How far the moisture influence reaches from the water body
    private int maxMoisture = 100;

    public MoistureMapGenerator(Map<float> heightMap, Map<int> lakeMap, Map<int> riverMap)
    {
        this.heightMap = heightMap;
        this.lakeMap = lakeMap;
        this.riverMap = riverMap;
    }

    public Map<int> Generate()
    {
        Map<int> moistureMap = new Map<int>(heightMap.Width, heightMap.Depth);

        float offsetX = UnityEngine.Random.Range(0, 10000f);
        float offsetY = UnityEngine.Random.Range(0, 10000f);

        for (int x = 0; x < heightMap.Width; x++)
        {
            for (int y = 0; y < heightMap.Depth; y++)
            {
                if (lakeMap.Data[x, y] == 1 || riverMap.Data[x, y] == 1)
                {
                    moistureMap.Data[x, y] = maxMoisture;
                    SpreadMoisture(x, y, moistureMap);
                }
                else
                {
                    float noise = Mathf.PerlinNoise((x + offsetX) / 10.0f, (y + offsetY) / 10.0f); // Adjust the division value to change the noise scale
                    moistureMap.Data[x, y] = (int)(noise * 100);
                }
            }
        }

        return moistureMap;
    }

    private void SpreadMoisture(int x, int y, Map<int> moistureMap)
    {
        for (int dx = -moistureRadius; dx <= moistureRadius; dx++)
        {
            for (int dy = -moistureRadius; dy <= moistureRadius; dy++)
            {
                int nx = x + dx;
                int ny = y + dy;

                if (nx >= 0 && nx < heightMap.Width && ny >= 0 && ny < heightMap.Depth)
                {
                    int distance = Mathf.Abs(dx) + Mathf.Abs(dy);

                    if (distance <= moistureRadius)
                    {
                        int influence = maxMoisture - (distance * (maxMoisture / moistureRadius));
                        moistureMap.Data[nx, ny] = Mathf.Min(moistureMap.Data[nx, ny] + influence, maxMoisture);
                    }
                }
            }
        }
    }
}

public class VegetationMapGenerator : IMapGenerator<VegetationType>
{
    private Map<int> moistureMap;

    public VegetationMapGenerator(Map<int> moistureMap)
    {
        this.moistureMap = moistureMap;
    }

    public Map<VegetationType> Generate()
    {
        Map<VegetationType> vegetationMap = new Map<VegetationType>(moistureMap.Width, moistureMap.Depth);

        for (int x = 0; x < moistureMap.Width; x++)
        {
            for (int y = 0; y < moistureMap.Depth; y++)
            {
                int moisture = moistureMap.Data[x, y];

                vegetationMap.Data[x, y] = VegetationType.Grass; // base vegetation

                if (UnityEngine.Random.Range(0f, 1f) <= 0.85f) // call 85% of the time
                {
                    // Adjust these moisture thresholds to change the distribution and amount of certain vegetation
                    if (moisture < 30)
                    {
                        vegetationMap.Data[x, y] = VegetationType.Sparse;
                    }
                    else if (moisture < 50)
                    {
                        vegetationMap.Data[x, y] = VegetationType.Grass;
                    }
                    else if (moisture < 70)
                    {
                        vegetationMap.Data[x, y] = VegetationType.Forest;
                    }
                    else
                    {
                        vegetationMap.Data[x, y] = VegetationType.Swamp;
                    }
                }
            }
        }
        return vegetationMap;
    }
}

public class WorldGenerator
{
    public int width;
    public int depth;
    public int rivers;
    public float lakeThreshold;

    public WorldGenerator()
    {
        UnityEngine.Random.InitState((int)System.DateTime.Now.Ticks);
    }

    public World Generate()
    {
        IMapGenerator<float> heightMapGenerator = new BaseTerrainGenerator(width, depth);

        Map<float> heightMap = heightMapGenerator.Generate();

        Map<int> lakeMap = new LakeMapGenerator(heightMap, lakeThreshold).Generate(); // 0/1 - rivers can end in lakes
        Map<int> riverMap = new RiverMapGenerator(heightMap, lakeMap, rivers).Generate(); // 0/1 

        heightMap = heightMap.ReduceByBeachFactor(lakeMap, 0.15f); // 0-1
        heightMap = heightMap.ReduceByBeachFactor(riverMap, 0.25f); // 0-1

        heightMap.Smooth();
        heightMap.GaussianBlur();

        Map<int> moistureMap = new MoistureMapGenerator(heightMap, lakeMap, riverMap).Generate(); // 0-100 range

        Map<VegetationType> vegetationMap = new VegetationMapGenerator(moistureMap).Generate(); 

        return GenerateWorldFromMaps(heightMap, moistureMap, vegetationMap);
    }

    public World GenerateWorldFromMaps(Map<float> heightMap, Map<int> moistureMap, Map<VegetationType> vegetationMap)
    {
        World world = new World(width, depth);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < depth; y++)
            {
                float height = heightMap.Data[x, y];
                int moisture = moistureMap.Data[x, y];
                VegetationType vegetation = vegetationMap.Data[x, y];

                // Assign the height, moisture and vegetation values to the tile
                Tile currTile = new Tile(height, moisture, vegetation, x, y);

                // Put that tile in the world on correct position
                world.Grid[x, y] = currTile;
            }
        }
        return world;
    }
}