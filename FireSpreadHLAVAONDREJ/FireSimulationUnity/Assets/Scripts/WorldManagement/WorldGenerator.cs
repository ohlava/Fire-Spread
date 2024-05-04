using System;
using UnityEngine;

public interface IMapGenerator<T>
{
    Map<T> Generate();
}

// Generates a base height map using Perlin noise for terrain generation.
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
        Map<float> map = new Map<float>(width, depth);

        int octaves = 5;
        float persistence = 0.4f;

        float offsetX = RandomUtility.Range(0, 10000f); ;
        float offsetY = RandomUtility.Range(0, 10000f); ;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < depth; y++)
            {
                float amplitude = 1;
                float frequency = 1;
                float noiseHeight = 0;

                for (int i = 0; i < octaves; i++) // multi-octave
                {
                    // Adjust the range to between -1 and 1, Mathf.PerlinNoise generates a value between 0 and 1
                    float perlinValue = Mathf.PerlinNoise((x + offsetX) / 20f * frequency, (y + offsetY) / 20f * frequency) * 2 - 1;
                    noiseHeight += perlinValue * amplitude;

                    amplitude *= persistence; // decrease the amplitude
                    frequency *= 2;
                }

                map.Data[x, y] = noiseHeight;
            }
        }
        return map.Normalize();
    }
}

// Identify valleys below the threshold, then group them into clusters, and from those clusters, decide which ones to fill based on the lakeThreshold parameter.
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

// Very simple river generation logic of river paths across the terrain.
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

        for (int i = 0; i < rivers; i++)
        {
            int riverStartX = RandomUtility.Range(0, heightMap.Width);
            int riverStartY = RandomUtility.Range(0, heightMap.Depth);

            int x = riverStartX;
            int y = riverStartY;
            int direction = RandomUtility.Range(0, 4);

            while (x < heightMap.Width && y < heightMap.Depth)
            {
                riverMap.Data[x, y] = 1;

                // Move in a semi-random direction
                switch (direction)
                {
                    case 0: if (RandomUtility.NextFloat() < 0.5f) x++; else y++; break;
                    case 1: if (RandomUtility.NextFloat() < 0.5f) y--; else x++; break;
                    case 2: if (RandomUtility.NextFloat() < 0.5f) x--; else y--; break;
                    case 3: if (RandomUtility.NextFloat() < 0.5f) y++; else x--; break;
                }

                if (x < 0 || y < 0 || x >= heightMap.Width || y >= heightMap.Depth || lakeMap.Data[x, y] == 1) // end in lake or out of the map
                    break;
            }
        }
        return riverMap;
    }
}

// Generator for a moisture map influenced by lakes and rivers.
public class MoistureMapGenerator : IMapGenerator<int>
{
    private Map<int> lakeMap;
    private Map<int> riverMap;

    private int moistureRadius = 2;  // How far the moisture influence reaches from the water body
    private int maxMoisture = 100;

    public MoistureMapGenerator(Map<int> lakeMap, Map<int> riverMap)
    {
        this.lakeMap = lakeMap;
        this.riverMap = riverMap;
    }

    public Map<int> Generate()
    {
        Map<int> moistureMap = new Map<int>(lakeMap.Width, lakeMap.Depth);

        float offsetX = RandomUtility.Range(0, 10000f);
        float offsetY = RandomUtility.Range(0, 10000f);

        for (int x = 0; x < lakeMap.Width; x++)
        {
            for (int y = 0; y < lakeMap.Depth; y++)
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

                if (nx >= 0 && nx < lakeMap.Width && ny >= 0 && ny < lakeMap.Depth)
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

// Generator for a vegetation map based on just the moisture levels.
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

                vegetationMap.Data[x, y] = VegetationType.Grass; // base default vegetation

                if (RandomUtility.Range(0f, 1f) <= 0.85f) // call 85% of the time
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
    }

    public World Generate()
    {
        IMapGenerator<float> heightMapGenerator = new BaseTerrainGenerator(width, depth);

        Map<float> heightMap = heightMapGenerator.Generate();

        Map<int> lakeMap = new LakeMapGenerator(heightMap, lakeThreshold).Generate(); // 0/1
        Map<int> riverMap = new RiverMapGenerator(heightMap, lakeMap, rivers).Generate(); // 0/1 - rivers can end in lakes

        heightMap = heightMap.ReduceByBeachFactor(lakeMap, 0.15f); // 0-1
        heightMap = heightMap.ReduceByBeachFactor(riverMap, 0.25f); // 0-1

        heightMap.Smooth();
        heightMap.GaussianBlur();

        // Combine maps differently or call other methods on those maps from MapExtensions.

        Map<int> moistureMap = new MoistureMapGenerator(lakeMap, riverMap).Generate(); // 0-100 range

        Map<VegetationType> vegetationMap = new VegetationMapGenerator(moistureMap).Generate();

        return WorldBuilder.CreateWorld(width, depth, heightMap, moistureMap, vegetationMap);
    }
}