using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class WorldBuilder
{
    public static void ApplyHeightMapToWorld(World world, Map<float> heightMap)
    {
        for (int i = 0; i < world.Width; i++)
        {
            for (int j = 0; j < world.Depth; j++)
            {
                float height = heightMap.Data[i, j];
                world.Grid[i, j].Height = height;
            }
        }
    }

    public static void ApplyMoistureMapToWorld(World world, Map<int> moistureMap)
    {
        for (int i = 0; i < world.Width; i++)
        {
            for (int j = 0; j < world.Depth; j++)
            {
                int moisture = moistureMap.Data[i, j];
                world.Grid[i, j].Moisture = moisture;
            }
        }
    }

    public static void ApplyVegetationMapToWorld(World world, Map<VegetationType> vegetationMap)
    {
        for (int i = 0; i < world.Width; i++)
        {
            for (int j = 0; j < world.Depth; j++)
            {
                VegetationType vegetation = vegetationMap.Data[i, j];
                world.Grid[i, j].Vegetation = vegetation;
            }
        }
    }

    public static World CreateWorld(int width, int depth, Map<float> heightMap = null, Map<int> moistureMap = null, Map<VegetationType> vegetationMap = null)
    {
        World world = new World(width, depth);

        if (heightMap != null)
        {
            ApplyHeightMapToWorld(world, heightMap);
        }

        if (moistureMap != null)
        {
            ApplyMoistureMapToWorld(world, moistureMap);
        }

        if (vegetationMap != null)
        {
            ApplyVegetationMapToWorld(world, vegetationMap);
        }

        return world;
    }
}

