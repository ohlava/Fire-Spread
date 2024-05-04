public static class WorldBuilder
{
    // Applies the height map to the world class. Sets corresponding height for tiles from map.
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

    // Applies the moisture map to the world. Sets corresponding moisture for tiles from map.
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

    // Applies the vegetation map to the world. Sets corresponding vegetation for tiles from map.
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

    // Creates a new world based on provided size and some optional maps.
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