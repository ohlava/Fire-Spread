using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class SerializableWorld
{
    public int Width;
    public int Depth;
    public List<SerializableTile> GridTiles;
}

[System.Serializable]
public class SerializableTile
{
    public int widthPosition;
    public int depthPosition;
    public int moisture;
    public VegetationType Vegetation;
    public float Height;
}

[System.Serializable]
public class InputDataSerializationPackage
{
    public SerializableWorld World;
    public bool[] InitialBurnMap;

    public InputDataSerializationPackage(SerializableWorld world, bool[] initialBurnMap)
    {
        World = world;
        InitialBurnMap = initialBurnMap;
    }
}

[System.Serializable]
public class OutputData
{
    public List<RowData> data;
}

[System.Serializable]
public class RowData
{
    public float[] rowData;
}

[System.Serializable]
public class WorldAndHeatMapData
{
    public SerializableWorld World;
    public OutputData HeatMap;

    public WorldAndHeatMapData(SerializableWorld world, OutputData heatMap)
    {
        World = world;
        HeatMap = heatMap;
    }
}


public class SerializableConversion
{
    public static SerializableWorld ConvertToWorldSerializable(World world)
    {
        SerializableWorld serializableWorld = new SerializableWorld
        {
            Width = world.Width,
            Depth = world.Depth,
            GridTiles = new List<SerializableTile>()
        };

        for (int i = 0; i < world.Width; i++)
        {
            for (int j = 0; j < world.Depth; j++)
            {
                serializableWorld.GridTiles.Add(ConvertToTileSerializable(world.Grid[i, j]));
            }
        }
        return serializableWorld;
    }

    public static InputDataSerializationPackage ConvertToInputDataSerializationPackage(World world, List<Tile> initBurningTiles)
    {
        SerializableWorld serializedWorld = ConvertToWorldSerializable(world);

        bool[] initialBurnMap = new bool[serializedWorld.Width * serializedWorld.Depth];
        foreach (var tile in initBurningTiles)
        {
            // flattened row by row (row-major order)
            int index = tile.WidthPosition * serializedWorld.Depth + tile.DepthPosition;

            // Safely check if the index is within bounds
            if (index >= 0 && index < initialBurnMap.Length)
            {
                initialBurnMap[index] = true;
            }
        }

        return new InputDataSerializationPackage(serializedWorld, initialBurnMap);
    }

    public static Map<float> ConvertToMap(OutputData outputData)
    {
        int depth = outputData.data.Count;
        int width = outputData.data.Max(row => row.rowData.Length);

        Map<float> map = new Map<float>(width, depth);

        float defaultValue = 0f;
        map.FillWithDefault(defaultValue);

        // Iterate through each RowData to fill the Map's Data.
        for (int i = 0; i < depth; i++)
        {
            for (int j = 0; j < width; j++)
            {
                map.Data[j, i] = outputData.data[i].rowData[j];
            }
        }

        return map;
    }

    public static OutputData ConvertMapToOutputData(Map<float> map)
    {
        OutputData outputData = new OutputData { data = new List<RowData>() };

        for (int y = 0; y < map.Depth; y++)
        {
            float[] rowData = new float[map.Width];
            for (int x = 0; x < map.Width; x++)
            {
                rowData[x] = map.Data[x, y];
            }
            outputData.data.Add(new RowData { rowData = rowData });
        }

        return outputData;
    }

    public static World ConvertFromWorldSerializable(SerializableWorld serializableWorld)
    {
        World world = new World(serializableWorld.Width, serializableWorld.Depth);
        for (int i = 0; i < serializableWorld.Width; i++)
        {
            for (int j = 0; j < serializableWorld.Depth; j++)
            {
                int index = i * serializableWorld.Depth + j;
                world.Grid[i, j] = ConvertFromTileSerializable(serializableWorld.GridTiles[index]);
            }
        }
        return world;
    }

    public static SerializableTile ConvertToTileSerializable(Tile tile)
    {
        return new SerializableTile
        {
            widthPosition = tile.WidthPosition,
            depthPosition = tile.DepthPosition,
            moisture = tile.Moisture,
            Vegetation = tile.Vegetation,
            Height = tile.Height,
        };
    }

    public static Tile ConvertFromTileSerializable(SerializableTile serializableTile)
    {
        Tile tile = new Tile(
            serializableTile.Height,
            serializableTile.moisture,
            serializableTile.Vegetation,
            serializableTile.widthPosition,
            serializableTile.depthPosition
        );

        return tile;
    }
}